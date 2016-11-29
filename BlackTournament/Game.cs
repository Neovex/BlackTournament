using System;
using System.IO;
using System.Linq;

using SFML.Window;
using SFML.Graphics;

using BlackCoat;

using BlackTournament.Properties;
using BlackTournament.GameStates;
using BlackTournament.Net;
using BlackTournament.Controller;

namespace BlackTournament
{
    public class Game
    {
        public const String DEFAULT_FONT = "HighlandGothicLightFLF";
        public const String DEFAULT_HOST = "localhost";
        public const UInt32 DEFAULT_PORT = 123;


        private FontManager _GlobalFonts;
        private GameServer _Server;
        private GameClient _Client;

        public Core Core { get; private set; }

        public static Font DefaultFont { get; private set; } // de-static?
        public ConnectController ConnectController { get; private set; }
        public MapController MapController { get; private set; }

        public Game()
        {
            //?
        }


        public void Run(String arguments)
        {
            // Init Black Coat Engine
            var device = Core.CreateDevice(800, 600, "Black Tournament", Styles.Close, 8);
            using (Core = new Core(device))
            {
                // Init Core
                Core.Debug = true;
                Core.ConsoleCommand += ExecuteCommand;

                // Init Logging
                Log.OnLog += m => File.AppendAllText("Log.txt", m + Environment.NewLine);
                Log.Info(String.Empty);
                Log.Info("################", "New Session:", DateTime.Now.ToLongTimeString(), "################");

                // Init Game Font
                _GlobalFonts = new FontManager();
                DefaultFont = _GlobalFonts.Load(DEFAULT_FONT, Resources.HighlandGothicLightFLF);
                // todo: test text blur issue (might need round)

                // Init Game
                ConnectController = new ConnectController(this);
                MapController = new MapController(this);
                _Server = new GameServer();

                // Start Game
                if (String.IsNullOrWhiteSpace(arguments))
                {
                    Core.StateManager.ChangeState(new MainMenu(Core));
                    //_Core.StateManager.ChangeState(new BlackCoatIntro(_Core, new Intro(_Core)));
                }
                else
                {
                    ExecuteCommand(arguments);
                }

                Core.Run();
                _GlobalFonts.Release(DEFAULT_FONT);
            }
        }

        public void StartNewGame(String map = null, String host = null, UInt32 port = 0)
        {
            if (map == null && host == null) throw new ArgumentException($"Failed to start with {map} - {host}");
            host = host ?? Game.DEFAULT_HOST;
            port = port == 0 ? Game.DEFAULT_PORT : port;

            // Setup Server
            _Server.Shutdown();
            if(host == Game.DEFAULT_HOST)
            {
                _Server.HostGame(map, port);
            }

            // Setup Client
            _Client?.Dispose();
            _Client = new GameClient(host, port, Settings.Default.PlayerName);
            ConnectController.Activate(_Client);
        }

        private bool ExecuteCommand(string cmd)
        {
            var separator = ' ';
            var commandData = cmd.Split(separator);
            try
            {
                switch (commandData[0].ToLower())
                {
                    case "state":
                        Log.Info("Current State:", Core.StateManager.CurrentState);
                        return true;

                    case "disconnect":
                        if (_Client != null && _Client.Connected)
                        {
                            _Client.Disconnect();
                            Log.Info("Disconnected");
                        }
                        else
                        {
                            Log.Info("No connection");
                        }
                        return true;

                    case "m":
                    case "msg":
                    case "message":
                        if (commandData.Length > 1)
                        {
                            if (_Client != null && _Client.Connected)
                            {
                                _Client.SendMessage(String.Join(separator.ToString(), commandData.Skip(1)));
                            }
                            else
                            {
                                Log.Info("Cannot send any messages, not connected to any server");
                            }
                        }
                        return true;

                    case "ld":
                    case "load":
                    case "loadmap":
                        if (commandData.Length == 2)
                        {
                            StartNewGame(map: commandData[1]);
                        }
                        else
                        {
                            Log.Info("invalid usage of loadmap filename", cmd);
                        }
                        return true;

                    case "con":
                    case "connect":
                        if (commandData.Length > 2)
                        {
                            var port = commandData.Length == 3 ? UInt32.Parse(commandData[2]) : Game.DEFAULT_PORT;
                            StartNewGame(host: commandData[1], port: port);
                        }
                        Log.Info("Invalid connect command. Use connect [hostname] optional:[port]", cmd);
                        return true;

                    case "srv":
                    case "startserver":
                    case "start server":
                        if (commandData.Length > 2)
                        {
                            var port = commandData.Length == 3 ? UInt32.Parse(commandData[2]) : Game.DEFAULT_PORT;
                            StartNewGame(map: commandData[1], port: port);
                        }
                        Log.Info("Invalid host command. Use host [mapname] optional:[port]", cmd);
                        return true;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Game Command", cmd, "failed. Reason:\n", ex);
                return true;
            }
            return false;
        }
    }
}