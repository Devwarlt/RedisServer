﻿using System;
using System.Collections.Generic;
using wServer.networking.cliPackets;

namespace wServer.networking
{
    internal interface IPacketHandler
    {
        PacketID ID { get; }

        void Handle(Client client, ClientPacket packet);
    }

    internal abstract class PacketHandlerBase<T> : IPacketHandler where T : ClientPacket
    {
        protected abstract void HandlePacket(Client client, T packet);

        public abstract PacketID ID { get; }

        public void Handle(Client client, ClientPacket packet)
        {
            HandlePacket(client, (T)packet);
        }

        protected void SendFailure(Client cli, string text)
        {
            cli.SendPacket(new svrPackets.TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "*Error*",
                Text = text
            });
            cli.SendPacket(new svrPackets.FailurePacket() { ErrorId = 0, ErrorDescription = text });
        }
    }

    internal class PacketHandlers
    {
        public static Dictionary<PacketID, IPacketHandler> Handlers = new Dictionary<PacketID, IPacketHandler>();

        static PacketHandlers()
        {
            foreach (var i in typeof(Packet).Assembly.GetTypes())
                if (typeof(IPacketHandler).IsAssignableFrom(i) &&
                    !i.IsAbstract && !i.IsInterface)
                {
                    IPacketHandler pkt = (IPacketHandler)Activator.CreateInstance(i);
                    Handlers.Add(pkt.ID, pkt);
                }
        }
    }
}
