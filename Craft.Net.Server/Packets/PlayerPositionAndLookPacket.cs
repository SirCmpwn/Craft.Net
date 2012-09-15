using Craft.Net.Data;
using Craft.Net.Data.Entities;

namespace Craft.Net.Server.Packets
{
    public sealed class PlayerPositionAndLookPacket : Packet
    {
        private bool OnGround;
        private float Pitch;
        private double Stance;
        private double X, Y;
        private float Yaw;
        private double Z;

        public PlayerPositionAndLookPacket(Vector3 position, float yaw, float pitch, bool onGround)
        {
            X = position.X;
            Y = position.Y;
            Z = position.Z;
            Stance = Y + PlayerEntity.Height;
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.OnGround = onGround;
        }

        public override byte PacketId
        {
            get { return 0x0D; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out X))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out Y))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out Stance))
                return -1;
            if (!DataUtility.TryReadDouble(buffer, ref offset, out Z))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Yaw))
                return -1;
            if (!DataUtility.TryReadFloat(buffer, ref offset, out Pitch))
                return -1;
            if (!DataUtility.TryReadBoolean(buffer, ref offset, out OnGround))
                return -1;
            return offset;
        }

        public override void HandlePacket(MinecraftServer server, MinecraftClient client)
        {
            if (!client.ReadyToSpawn)
                return;
            client.Entity.Position = new Vector3(X, Y, Z);
            client.Entity.Pitch = Pitch;
            client.Entity.Yaw = Yaw;
            if (client.Entity.Position.DistanceTo(client.Entity.OldPosition) >
                client.MaxMoveDistance)
            {
                client.SendPacket(new DisconnectPacket("Hacking: You moved too fast!"));
                server.ProcessSendQueue();
                return;
            }
            client.UpdateChunksAsync();
            server.ProcessSendQueue();
            server.EntityManager.UpdateEntity(client.Entity);
        }

        public override void SendPacket(MinecraftServer server, MinecraftClient client)
        {
            client.SendData(CreateBuffer(
                DataUtility.CreateDouble(X),
                DataUtility.CreateDouble(Stance),
                DataUtility.CreateDouble(Y),
                DataUtility.CreateDouble(Z),
                DataUtility.CreateFloat(Yaw),
                DataUtility.CreateFloat(Pitch),
                DataUtility.CreateBoolean(OnGround)));
        }
    }
}