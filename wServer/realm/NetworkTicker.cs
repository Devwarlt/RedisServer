﻿using log4net;
using System;
using System.Collections.Concurrent;
using System.Threading;
using wServer.networking;

namespace wServer.realm
{
    using Work = Tuple<Client, PacketID, byte[]>;

    public class NetworkTicker //Sync network processing
    {
        private ILog log = LogManager.GetLogger(typeof(NetworkTicker));

        public RealmManager Manager { get; private set; }

        public NetworkTicker(RealmManager manager)
        {
            Manager = manager;
        }

        public void AddPendingPacket(Client client, PacketID id, byte[] packet)
        {
            pendings.Enqueue(new Work(client, id, packet));
        }

        private static ConcurrentQueue<Work> pendings = new ConcurrentQueue<Work>();
        private static SpinWait loopLock = new SpinWait();

        public void TickLoop()
        {
            log.Info("Network loop started.");
            Work work;
            while (true)
            {
                if (Manager.Terminating) break;
                loopLock.Reset();
                while (pendings.TryDequeue(out work))
                {
                    if (Manager.Terminating) return;
                    if (work.Item1.Stage == ProtocalStage.Disconnected)
                    {
                        Client client;
                        Manager.Clients.TryRemove(work.Item1.Id, out client);
                        continue;
                    }
                    try
                    {
                        Packet packet = Packet.Packets[work.Item2].CreateInstance();
                        packet.Read(work.Item1, work.Item3, 0, work.Item3.Length);
                        work.Item1.ProcessPacket(packet);
                    }
                    catch { }
                }
                while (pendings.Count == 0 && !Manager.Terminating)
                    loopLock.SpinOnce();
            }
            log.Info("Network loop stopped.");
        }
    }
}
