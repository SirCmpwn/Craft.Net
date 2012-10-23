using Craft.Net.Server.Events;
using Craft.Net.Data;

namespace Craft.Net.Server.Packets
{
    public class ChatMessagePacket : Packet
    {
        public string Message;

        public ChatMessagePacket()
        {
        }

        public ChatMessagePacket(string message)
        {
            this.Message = message;
        }

        public override byte PacketId
        {
            get { return 0x03; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!DataUtility.TryReadString(buffer, ref offset, out Message))
                return -1;
            return offset;
        }

        public override void HandlePacket(MinecraftServer server, MinecraftClient client)
        {
            LogProvider.Log("<" + client.Username + "> " + Message, LogImportance.Medium);
            var args = new ChatMessageEventArgs(client, Message);
            server.OnChatMessage(args);
            if (!args.Handled)
                server.SendChat("<" + client.Username + "> " + Message);
        }

        public override void SendPacket(MinecraftServer server, MinecraftClient client)
        {
            client.SendData(CreateBuffer(DataUtility.CreateString(Message)));
        }
    }
}