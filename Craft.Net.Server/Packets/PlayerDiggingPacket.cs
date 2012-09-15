using System;
using Craft.Net.Data;
using Craft.Net.Data.Blocks;

namespace Craft.Net.Server.Packets
{
    public enum PlayerAction
    {
        StartedDigging = 0,
        FinishedDigging = 2,
        DropItem = 4,
        ShootArrow = 5
    }

    public sealed class PlayerDiggingPacket : Packet
    {
        private const double MaxDigDistance = 6;

        private PlayerAction Action;
        private byte Face;
        private Vector3 Position;

        public override byte PacketId
        {
            get { return 0x0E; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            byte action, y;
            int x, z;
            if (!DataUtility.TryReadByte(buffer, ref offset, out action))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out x))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out y))
                return -1;
            if (!DataUtility.TryReadInt32(buffer, ref offset, out z))
                return -1;
            if (!DataUtility.TryReadByte(buffer, ref offset, out Face))
                return -1;
            Position = new Vector3(x, y, z);
            Action = (PlayerAction)action;
            return offset;
        }

        public override void HandlePacket(MinecraftServer server, MinecraftClient client)
        {
            if (client.Entity.Position.DistanceTo(Position) <= MaxDigDistance)
            {
                switch (Action)
                {
                    case PlayerAction.StartedDigging:
                        if (client.Entity.GameMode == GameMode.Creative)
                            client.World.SetBlock(Position, new AirBlock());
                        break;
                    case PlayerAction.FinishedDigging:
                        client.World.SetBlock(Position, new AirBlock());
                        break;
                }
            }
        }

        public override void SendPacket(MinecraftServer server, MinecraftClient client)
        {
            throw new InvalidOperationException();
        }
    }
}