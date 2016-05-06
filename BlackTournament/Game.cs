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
using BlackTournament.StateManagement;

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
        private static StateManager _StateManager;

        public static void Main(string[] args)
        {
            // Init Black Coat Engine
            _Core = new Core(Core.DefaultDevice, true);
            _Core.OnLog += m => File.AppendAllText("Log.txt", m + Environment.NewLine);
            _Core.AssetManager.RootFolder = "Assets";
            _Core.Log(Environment.NewLine, "################", "New Session:", DateTime.Now.ToShortTimeString(), "################");

            // Init Game
            DefaultGameFont = _Core.AssetManager.LoadFont(DefaultGameFontName);
            _StateManager = new StateManager(_Core);
            _Core.ConsoleCommand += HandleConsoleCommand;

            // Start Game
            _StateManager.ChangeState(State.Intro);


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
                            _StateManager.ChangeState(State.Map, commandData[1]);
                        }
                        else
                        {
                            _Core.Log("invalid usage of loadmap filename", cmd);
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
                            _Core.Log("Host is listening on", port);
                            return true;
                        }
                        _Core.Log("Invalid host data", cmd);
                    return true;

                    case "connect":
                        if (commandData.Length == 3)
                        {
                            _Client = new Client();
                            var port = UInt32.Parse(commandData[2]);
                            _Client.Connect(commandData[1], port);
                            _Client.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
                            //TransmitMove(_Client);
                            _Core.Log("Connection to", commandData[1], ":", port);
                            return true;
                        }
                        _Core.Log("Invalid client data", cmd);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _Core.Log("Game Command", cmd, "failed. Reason:", ex);
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