﻿using System;
using System.IO;
using System.Linq;

using SFML.Window;
using SFML.Graphics;

using BlackCoat;

using BlackTournament.Properties;
using BlackTournament.Net;
using BlackTournament.Controller;
using BlackTournament.Systems;

namespace BlackTournament
{
    public class Game
    {
        public const String ID = "BlackTournament";
        public const String DEFAULT_FONT = "HighlandGothicLightFLF";
        private const String _LOGFILE = "Log.txt";


        private FontManager _GlobalFonts;
        private BlackTournamentServer _Server;
        private BlackTournamentClient _Client;

        public Core Core { get; private set; }
        public InputMapper InputMapper { get; private set; }

        public static Font DefaultFont { get; private set; }
        public MenuController MenuController { get; private set; }
        public ConnectController ConnectController { get; private set; }
        public MapController MapController { get; private set; }

        public Game()
        {
            // Init Logging
            Log.Level = LogLevel.Debug;
            if (File.Exists(_LOGFILE)) File.AppendAllText(_LOGFILE, Environment.NewLine);
            Log.OnLog += m => File.AppendAllText(_LOGFILE, $"{m}{Environment.NewLine}");
            Log.Info("################", "New Session:", DateTime.Now.ToLongTimeString(), "################");
        }


        public void Run(String arguments)
        {
            // Init Black Coat Engine
            var device = Core.CreateDevice(800, 600, "Black Tournament", Styles.Close, 8);
            using (Core = new Core(device))
            {
                // Init Core
                Core.Debug = true;
                Core.OnUpdate += Update;
                Core.ConsoleCommand += ExecuteCommand;

                // Init Game Font
                _GlobalFonts = new FontManager();
                DefaultFont = _GlobalFonts.Load(DEFAULT_FONT, Resources.HighlandGothicLightFLF);
                // Todo: test text blur issue (might need round)

                // Init Input
                InputMapper = new InputMapper();

                // Init Game
                MenuController = new MenuController(this);
                ConnectController = new ConnectController(this);
                MapController = new MapController(this);
                _Server = new BlackTournamentServer(Core);
                _Client = new BlackTournamentClient(Settings.Default.PlayerName);

                // Start Game
                if (String.IsNullOrWhiteSpace(arguments))
                {
                    MenuController.Activate();
                    //_Core.StateManager.ChangeState(new BlackCoatIntro(_Core, new Intro(_Core)));
                }
                else
                {
                    ExecuteCommand(arguments);
                }

                Core.Run();

                // Cleanup
                _Client.Disconnect();
                _Client.Dispose();
                _Client = null;
                _Server.StopServer();
                _Server.Dispose();
                _Server = null;
                _GlobalFonts.Dispose();
                _GlobalFonts = null;
            }
        }

        private void Update(float deltaT)
        {
            _Server.Update(deltaT);
            _Client.Update(deltaT);
            System.Threading.Thread.Sleep(1);
        }

        public void StartNewGame(String map = null, String host = null, int port = 0)
        {
            if (map == null && host == null) throw new ArgumentException($"Failed to start with {map} - {host}");
            host = host ?? Net.Net.DEFAULT_HOST;
            port = port == 0 ? Net.Net.DEFAULT_PORT : port;

            // Setup Server
            if(host == Net.Net.DEFAULT_HOST)
            {
                if(!_Server.HostGame(map, port)) return;
            }

            // Setup Client
            _Client.Disconnect();
            _Client = new BlackTournamentClient(Settings.Default.PlayerName);
            ConnectController.Activate(_Client, host, port);
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
                        if (_Client.IsConnected)
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
                            if (_Client.IsConnected)
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
                            var port = commandData.Length == 3 ? Int32.Parse(commandData[2]) : Net.Net.DEFAULT_PORT;
                            StartNewGame(host: commandData[1], port: port);
                        }
                        Log.Info("Invalid connect command. Use connect [host name] optional:[port]", cmd);
                        return true;

                    case "srv":
                    case "startserver":
                    case "start server":
                        if (commandData.Length > 2)
                        {
                            var port = commandData.Length == 3 ? Int32.Parse(commandData[2]) : Net.Net.DEFAULT_PORT;
                            StartNewGame(map: commandData[1], port: port);
                        }
                        Log.Info("Invalid host command. Use host [map name] optional:[port]", cmd);
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