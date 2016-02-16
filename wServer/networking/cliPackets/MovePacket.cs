﻿using common;

namespace wServer.networking.cliPackets
{
    public class MovePacket : ClientPacket
    {
        public int TickId { get; set; }

        public int Time { get; set; }

        public Position Position { get; set; }

        public TimedPosition[] Records { get; set; }

        public override PacketID ID
        {
            get { return PacketID.MOVE; }
        }

        public override Packet CreateInstance()
        {
            return new MovePacket();
        }

        protected override void Read(NReader rdr)
        {
            TickId = rdr.ReadInt32();
            Time = rdr.ReadInt32();
            Position = Position.Read(rdr);
            Records = new TimedPosition[rdr.ReadInt16()];
            for (int i = 0; i < Records.Length; i++)
                Records[i] = TimedPosition.Read(rdr);
        }

        protected override void Write(NWriter wtr)
        {
            wtr.Write(TickId);
            wtr.Write(Time);
            Position.Write(wtr);
            if (Records == null)
            {
                wtr.Write((ushort)0);
                return;
            }
            wtr.Write((ushort)Records.Length);
            foreach (TimedPosition i in Records)
                i.Write(wtr);
        }
    }
}
