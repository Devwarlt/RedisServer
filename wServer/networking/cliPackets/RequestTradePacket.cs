﻿using common;

namespace wServer.networking.cliPackets
{
    public class RequestTradePacket : ClientPacket
    {
        public string Name { get; set; }

        public override PacketID ID
        {
            get { return PacketID.REQUESTTRADE; }
        }

        public override Packet CreateInstance()
        {
            return new RequestTradePacket();
        }

        protected override void Read(NReader rdr)
        {
            Name = rdr.ReadUTF();
        }

        protected override void Write(NWriter wtr)
        {
            wtr.WriteUTF(Name);
        }
    }
}
