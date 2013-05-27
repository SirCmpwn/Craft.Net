using System;
using System.Globalization;
using Craft.Net.Server;
using System.Linq;
using System.Net;
using Craft.Net.Data;
using Craft.Net.Data.Generation;
using Craft.Net.Server.Channels;
using Craft.Net.Server.Events;
using System.Reflection;
using Craft.Net.Data.Blocks;
using System.IO;
using Craft.Net;
using Craft.Net.Data.Windows;
using Craft.Net.Data.Items;

namespace TestServer
{
    class MainClass
    {
        static MinecraftServer minecraftServer;

        public static void Main(string[] args)
        {
            // Create a server on 0.0.0.0:25565
            minecraftServer = new MinecraftServer(
                new IPEndPoint(IPAddress.Any, 25565));
            minecraftServer.Settings.OnlineMode = false;
            minecraftServer.Settings.EnableEncryption = true;
            CustomLeatherItem.Server = minecraftServer;
            Item.SetItemClass(new CustomLeatherItem());
            // Add a console logger
            LogProvider.RegisterProvider(new ConsoleLogWriter(LogImportance.Medium));
            LogProvider.RegisterProvider(new FileLogWriter("packetLog.txt", LogImportance.Low));
            // Add a flatland world
#if DEBUG
            // Use a fresh world each time
            if (Directory.Exists("world"))
                Directory.Delete("world", true);
#endif
            IWorldGenerator generator = new FlatlandGenerator();
            minecraftServer.AddLevel(new Level(generator, Path.Combine(Directory.GetCurrentDirectory(), "world")));
            minecraftServer.DefaultLevel.GameMode = GameMode.Survival;
            // Register the chat handler
            minecraftServer.ChatMessage += HandleOnChatMessage;
            // Start the server
            minecraftServer.Start();
            Console.WriteLine("Press 'q' key to exit.");
            while (Console.ReadKey(true).Key != ConsoleKey.Q) { }
            // Stop the server
            minecraftServer.Stop();
            minecraftServer.DefaultLevel.Save();
        }

        static void HandleOnChatMessage(object sender, ChatMessageEventArgs e)
        {
            if (e.RawMessage.StartsWith("/"))
            {
                e.Handled = true;
                string command = e.RawMessage.Substring(1);
                string[] parameters = null;
                if (command.Contains(" "))
                {
                    parameters = command.Substring(command.IndexOf(' ') + 1).Split(' ');
                    command = command.Remove(command.IndexOf(' '));
                }
                command = command.ToLower();
                switch (command)
                {
                    case "under":
                        try
                        {
                            e.Origin.SendChat("Block under you: " + 
                                e.Origin.World.GetBlock(e.Origin.Entity.Position + Vector3.Down).GetType().Name);
                        }
                        catch { }
                        break;
                    case "ping":
                        e.Origin.SendChat("Pong");
                        break;
                    case "lightning":
                        minecraftServer.GetWeatherManagerForWorld(minecraftServer.DefaultWorld)
                            .SpawnLightning(e.Origin.Entity.Position);
                        break;
                    case "velocity":
                        e.Origin.SendChat(e.Origin.Entity.Velocity.ToString());
                        break;
                    case "save":
                        minecraftServer.DefaultWorld.Regions[Vector3.Zero].Save();
                        break;
                    case "time":
                        var clients = minecraftServer.EntityManager.GetClientsInWorld(e.Origin.World);
                        minecraftServer.GetLevel(e.Origin.World).Time = 18000;
                        foreach (var minecraftClient in clients)
                            minecraftClient.SendPacket(new TimeUpdatePacket(18000, 18000));
                        break;
                    case "kill":
                        e.Origin.Entity.Health = 0;
                        break;
                    case "survival":
                        e.Origin.Entity.GameMode = GameMode.Survival;
                        break;
                    case "creative":
                        e.Origin.Entity.GameMode = GameMode.Creative;
                        break;
                    case "spawn":
                        // TODO: Why does this mess up stance
                        minecraftServer.EntityManager.TeleportEntity(e.Origin.Entity, e.Origin.Entity.SpawnPoint);
                        break;
                    case "jump":
                        e.Origin.Entity.Velocity = new Vector3(0, 10, 0);
                        break;
                    case "forward":
                        Vector3 velocity = MathHelper.RotateY(Vector3.Forwards * 5,
                            MathHelper.DegreesToRadians(e.Origin.Entity.Yaw));
                        e.Origin.Entity.Velocity = velocity;
                        e.Origin.SendChat(e.Origin.Entity.Yaw.ToString(CultureInfo.InvariantCulture));
                        break;
                    case "flying":
                        e.Origin.Entity.Abilities.MayFly = !e.Origin.Entity.Abilities.MayFly;
                        break;
                    case "instantmine":
                        e.Origin.Entity.Abilities.InstantMine = !e.Origin.Entity.Abilities.InstantMine;
                        break;
                    case "hunger":
                        e.Origin.Entity.Food--;
                        break;
                    case "damage":
                        e.Origin.Entity.Health--;
                        break;
                    case "se":
                        e.Origin.SendPacket(new NamedSoundEffectPacket(e.RawMessage.Substring(4), (int)e.Origin.Entity.Position.X,
                            (int)e.Origin.Entity.Position.Y, (int)e.Origin.Entity.Position.Z, 1.0f, 63)); // TODO: Write 63 down somewhere
                        break;
                    case "destroy":
                        minecraftServer.DefaultWorld.SetBlock(new Vector3(0, 10, 0), new AirBlock());
                        break;
                    case "give":
                        try
                        {
                            var type = ((Block)int.Parse(parameters[0])).GetType();
                            var item = (Item)Activator.CreateInstance(type);
                            e.Origin.Entity.SetSlot(InventoryWindow.HotbarIndex, new ItemStack(item.Id, 1));
                        } catch { }
                        break;
                    case "relight":
                        e.Origin.World.Relight();
                        e.Origin.SendChat("World relit.");
                        break;
                    case "createboard":
                        var board = minecraftServer.ScoreboardManager.CreateScoreboard("test", "Scoreboard");
                        minecraftServer.ScoreboardManager.DisplayScoreboard(board, DisplayScoreboardPacket.ScoreboardPosition.Sidebar);
                        board.AddScore("Test", 1234);
                        break;
                    case "updateboard":
                        minecraftServer.ScoreboardManager["test"]["Test"]++;
                        break;
                    case "removeboard":
                        minecraftServer.ScoreboardManager.RemoveScoreboard("test");
                        break;
                    case "createteam":
                        minecraftServer.ScoreboardManager.CreateTeam(parameters[0], parameters[1],
                            true, ChatColors.Delimiter + parameters[2], ChatColors.Plain);
                        break;
                    case "setteam":
                        var team = minecraftServer.ScoreboardManager.GetTeam(parameters[0]);
                        team.AddPlayers(parameters.Skip(1).ToArray());
                        break;
                    case "toggledownfall":
                        var weatherManager = minecraftServer.GetWeatherManagerForWorld(e.Origin.World);
                        weatherManager.IsRainActive = !weatherManager.IsRainActive;
                        break;
                    case "setchunkbiomesnow":
                        var chunk = e.Origin.World.GetChunk(e.Origin.Entity.Position / 16);
                        for (byte x = 0; x < 16; x++)
                        {
                            for (byte z = 0; z < 16; z++)
                            {
                                chunk.SetBiome(x, z, Biome.IcePlains);
                            }
                        }
                        foreach (var client in minecraftServer.EntityManager.GetClientsInWorld(e.Origin.World).ToArray())
                        {
                            if (client.LoadedChunks.Contains(chunk.AbsolutePosition / 16))
                            {
                                client.UnloadChunk(chunk.AbsolutePosition / 16);
                                client.LoadChunk(chunk.AbsolutePosition / 16);
                            }
                        }
                        break;
                    case "biome":
                        chunk = e.Origin.World.GetChunk(e.Origin.Entity.Position);
                        e.Origin.SendChat(chunk.GetBiome(0, 0).ToString());
                        break;
                }
            }
        }
    }
}
