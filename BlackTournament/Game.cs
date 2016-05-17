using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using TiledSharp;
using SFML.System;
using SFML.Graphics;
using SFML.Window;
using BlackTournament.Net;
using BlackCoat.Entities.Shapes;
using System.IO;
using BlackTournament.GameStates;

namespace BlackTournament
{
    public class Game
    {
        public static Font DefaultGameFont = null;
        private static readonly String DefaultGameFontName = "HighlandGothicLightFLF";

        private static Server _Server;
        private static Client _Client;
        private static Core _Core;
        private static Rectangle _Other;
        private static Map _CurrentMap;

        public static void Main(string[] args)
        {
            // Init Black Coat Engine
            _Core = new Core(Core.DefaultDevice);
            _Core.Debug = true;
            Log.OnLog += m => File.AppendAllText("Log.txt", m + Environment.NewLine);
            _Core.AssetManager.RootFolder = "Assets";
            Log.Debug(String.Empty);
            Log.Debug("################", "New Session:", DateTime.Now.ToLongTimeString(), "################");

            // Init Game
            DefaultGameFont = _Core.AssetManager.LoadFont(DefaultGameFontName);
            _Core.ConsoleCommand += HandleConsoleCommand;

            // Start Game
            _Core.StateManager.ChangeState(new Intro(_Core));


            /*_Other = new Rectangle(_Core);
            _Other.View = zoomView;
            _Other.Size = new Vector2f(15, 15);
            _Other.Color = Color.Blue;
            _Core.Layer_Game.AddChild(_Other);*/


            _Core.Run();
            _Core.AssetManager.FreeFont(DefaultGameFontName);
        }

        static bool HandleConsoleCommand(string cmd)
        {
            var commandData = cmd.Split(' ');
            try
            {
                switch (commandData[0].ToLower())
                {
                    case "loadmap":
                        if (commandData.Length == 2)
                        {
                            _Core.StateManager.ChangeState(new MapState(_Core, commandData[1]));
                        }
                        else
                        {
                            Log.Debug("invalid usage of loadmap filename", cmd);
                        }
                        return true;
                    case "startserver":
                    case "start server":
                        if (commandData.Length == 2)
                        {
                            _Server = new Server();
                            var port = UInt32.Parse(commandData[1]);
                            _Server.Host(port);
                            _Server.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
                            //TransmitMove(_Server);
                            Log.Debug("Host is listening on", port);
                            return true;
                        }
                        Log.Debug("Invalid host data", cmd);
                    return true;

                    case "connect":
                        if (commandData.Length == 3)
                        {
                            _Client = new Client();
                            var port = UInt32.Parse(commandData[2]);
                            _Client.Connect(commandData[1], port);
                            _Client.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
                            //TransmitMove(_Client);
                            Log.Debug("Connection to", commandData[1], ":", port);
                            return true;
                        }
                        Log.Debug("Invalid client data", cmd);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Game Command", cmd, "failed. Reason:", ex);
                return true;
            }
            return false;
        }

        /*
        private static void TransmitMove(Server server)
        {
            server.BroadcastMove(0, _Player.Position.X, _Player.Position.Y);
            _Core.AnimationManager.Wait(1 / 60f, a => TransmitMove(server));
        }

        private static void TransmitMove(Client client)
        {
            client.DoMove(0, _Player.Position.X, _Player.Position.Y);
            _Core.AnimationManager.Wait(1 / 60f, a => TransmitMove(client));
        }*/
    }
}