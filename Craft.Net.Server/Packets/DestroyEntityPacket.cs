﻿using System;
using System.Linq;
using Craft.Net.Data;

namespace Craft.Net.Server.Packets
{
    public sealed class DestroyEntityPacket : Packet
    {
        private int[] EntityIds;

        public DestroyEntityPacket(params int[] entities) // TODO: I don't really like this
        {
            EntityIds = entities;
        }

        public override byte PacketId
        {
            get { return 0x1D; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            throw new InvalidOperationException();
        }

        public override void HandlePacket(MinecraftServer server, MinecraftClient client)
        {
            throw new InvalidOperationException();
        }

        public override void SendPacket(MinecraftServer server, MinecraftClient client)
        {
            byte[] payload = new[] {(byte)EntityIds.Length};
            foreach (int id in EntityIds)
                payload = payload.Concat(DataUtility.CreateInt32(id)).ToArray(); //TODO make this nicer

            client.SendData(payload);
        }
    }
}
