﻿using wServer.networking.cliPackets;

namespace wServer.networking.handlers
{
    internal class OtherHitHandler : PacketHandlerBase<OtherHitPacket>
    {
        public override PacketID ID { get { return PacketID.OTHERHIT; } }

        protected override void HandlePacket(Client client, OtherHitPacket packet)
        {
            //TODO: implement something
        }
    }
}
