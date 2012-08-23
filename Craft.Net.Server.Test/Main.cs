using System;
using Craft.Net.Server;
using System.Linq;
using System.Net;
using Craft.Net.Data;
using Craft.Net.Data.Generation;
using Craft.Net.Server.Events;
using System.Reflection;
using Craft.Net.Data.Blocks;

namespace Craft.Net.Server.Test
{
    class MainClass
    {
        static MinecraftServer minecraftServer;

        public static void Main(string[] args)
        {
            // Create a server on 0.0.0.0:25565
            minecraftServer = new MinecraftServer(
		        new IPEndPoint(IPAddress.Any, 25565));
            minecraftServer.OnlineMode = false;
            minecraftServer.EncryptionEnabled = true;
            // Add a console logger
            minecraftServer.AddLogProvider(new ConsoleLogWriter(LogImportance.High));
            minecraftServer.AddLogProvider(new FileLogWriter("packetLog.txt", LogImportance.Low));
            // Add a flatland world
            minecraftServer.AddWorld(new World(new FlatlandGenerator()));
            // Register the chat handler
            minecraftServer.OnChatMessage += HandleOnChatMessage;
            // Start the server
            minecraftServer.Start();
            Console.WriteLine("Press any key to exit.");
            while (Console.ReadKey(true).Key != ConsoleKey.Q)
		    continue;

            // Stop the server
            minecraftServer.Stop();
        }

        static void HandleOnChatMessage(object sender, ChatMessageEventArgs e)
        {
            if (e.RawMessage.StartsWith("/"))
            {
                e.Handled = true;
                string command = e.RawMessage.Substring(1);
                if (command.Contains(" "))
                    command = command.Remove(command.IndexOf(' '));
                command = command.ToLower();
                switch (command)
                {
                    case "under":
                        try
                        {
                            e.Origin.SendChat("Block under you: " + 
                                minecraftServer.GetClientWorld(e.Origin).GetBlock(
                                e.Origin.Entity.Position + Vector3.Down).GetType().Name);
                        }
                        catch { }
                        break;
                    case "ping":
                        e.Origin.SendChat("Pong");
                        break;
                }
            }
        }
    }
}
