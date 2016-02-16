﻿using common;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class PlayerShootPacketHandler : PacketHandlerBase<PlayerShootPacket>
    {
        public override PacketID ID { get { return PacketID.PLAYERSHOOT; } }

        protected override void HandlePacket(Client client, PlayerShootPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, PlayerShootPacket packet)
        {
            if (player.Owner == null) return;

            Item item = player.Manager.GameData.Items[(ushort)packet.ContainerType];
            var prjDesc = item.Projectiles[0]; //Assume only one
            Projectile prj = player.PlayerShootProjectile(
                packet.BulletId, prjDesc, item.ObjectType,
                packet.Time, packet.Position, packet.Angle);
            player.Owner.EnterWorld(prj);
            player.BroadcastSync(new AllyShootPacket()
            {
                OwnerId = player.Id,
                Angle = packet.Angle,
                ContainerType = (ushort)packet.ContainerType,
                BulletId = packet.BulletId
            }, p => p != player && player.Dist(p) < 25);
            player.FameCounter.Shoot(prj);
        }
    }
}
