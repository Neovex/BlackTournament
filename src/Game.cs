﻿using System;
using System.IO;
using System.Linq;

using SFML.Window;
using SFML.Graphics;

using BlackNet.Server;

using BlackCoat;

using BlackTournament.Net;
using BlackTournament.Properties;
using BlackTournament.Controller;
using BlackTournament.Scenes;

namespace BlackTournament
{
    public class Game
    {
        public static Font DefaultFont { get; private set; }
        public static Font StyleFont { get; private set; }

        public const String ID = nameof(BlackTournament);
        public const String MAP_ROOT = "Maps\\";
        public const String TEXTURE_ROOT = "Assets\\Textures";
        public const String MUSIC_ROOT = "Assets\\Music";
        public const String FONT_ROOT = "Assets\\Fonts";
        public const String SFX_ROOT = "Assets\\Sfx";
        private const String DEFAULT_FONT = "HighlandGothicLightFLF";
        private const String STYLE_FONT = "VTCBelialsBladeItalic";
        private const String _LOGFILE = "Log.txt";


        private FontLoader _GlobalFonts;
        private BlackTournamentServer _Server;
        private BlackTournamentClient _Client;


        public Core Core { get; private set; }
        public MenuController MenuController { get; private set; }
        public ConnectController ConnectController { get; private set; }
        public MapController MapController { get; private set; }


        public Game()
        {
            // Init Logging
            Log.Level = LogLevel.Debug;
            if (File.Exists(_LOGFILE)) File.AppendAllText(_LOGFILE, Environment.NewLine);
            // Log.OnLog += m => File.AppendAllText(_LOGFILE, $"{m}{Environment.NewLine}"); // TODO : find a better log to file solution
            Log.Info("################", "New Session:", DateTime.Now.ToLongTimeString(), "################");
        }


        public void Run(String arguments)
        {
            Device device;
#if !DEBUG
            // Show Launcher
            device = Device.Create(new Launcher(new SettingsAdapter(), new SettingsTab())
            {
                BannerImage = Resources.banner
            }, ID);
            if (device == null)
            {
                Log.Fatal("Failed to create Renderdevice");
                return;
            }
#endif
#if DEBUG
            device = Device.Create(new VideoMode(1024, 768), ID, Styles.Default, 0, false);
#endif  
            // Init Core
            using (Core = new Core(device))
            {
#if DEBUG
                Core.Debug = true;
#endif
                Core.OnUpdate += Update;
                Core.ConsoleCommand += ExecuteCommand;

                using (_GlobalFonts = new FontLoader(FONT_ROOT))
                {
                    // Init Game Fonts
                    DefaultFont = _GlobalFonts.Load(DEFAULT_FONT);
                    Core.InitializeFontHack(DefaultFont);
                    StyleFont = _GlobalFonts.Load(STYLE_FONT);
                    Core.InitializeFontHack(StyleFont);

                    // Init Game
                    MenuController = new MenuController(this);
                    ConnectController = new ConnectController(this);
                    MapController = new MapController(this);
                    _Server = new BlackTournamentServer(Core);
                    _Client = new BlackTournamentClient(Settings.Default.PlayerName);

                    // Start Game
                    if (String.IsNullOrWhiteSpace(arguments))
                    {
#if DEBUG
                        MenuController.Activate();
#endif
#if !DEBUG
                        Core.SceneManager.ChangeScene(new BlackCoatIntro(Core, new TournamentIntro(Core, MenuController))); // TODO : create global music controller?
#endif
                    }
                    else
                    {
                        ExecuteCommand(arguments);
                    }

                    // Start Rendering
                    Core.Run();

                    // Cleanup
                    _Client.Disconnect();
                    _Client.Dispose();
                    _Client = null;
                    _Server.StopServer();
                    _Server.Dispose();
                    _Server = null;
                }
            }

#if !DEBUG
            Settings.Default.Save();
#endif
        }

        private void Update(float deltaT)
        {
            _Server.Update(deltaT);
            _Client.Update(deltaT);
        }

        public bool Host(int port, String map, String serverName)
        {
            if (string.IsNullOrWhiteSpace(map)) throw new ArgumentNullException(nameof(map));
            return _Server.HostGame(port, new ServerInfo(serverName, map));
        }

        public void Connect(String host, int port)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
            ResetClient();
            ConnectController.Activate(_Client, host, port);
        }
        public void Connect(ServerInfo host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            ResetClient();
            ConnectController.Activate(_Client, host);
        }

        private void ResetClient()
        {
            _Client.Disconnect();
            _Client.Dispose();
            _Client = new BlackTournamentClient(Settings.Default.PlayerName);
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
                        Log.Info("Current State:", Core.SceneManager.CurrentSceneName);
                        return true;

                    case "disconnect":
                        if (_Client.IsConnected)
                        {
                            Log.Info("Disconnecting...");
                            _Client.Disconnect();
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
                                Log.Info("Cannot send messages, not connected to any server");
                            }
                        }
                        return true;

                    case "ld":
                    case "load":
                    case "loadmap":
                    case "srv":
                    case "startserver":
                    case "start server":
                        if (commandData.Length == 2)
                        {
                            if (Host(Net.Net.DEFAULT_PORT, commandData[1], $"{Settings.Default.PlayerName}'s Server"))
                                Connect(Net.Net.DEFAULT_HOST, Net.Net.DEFAULT_PORT);
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
                            Connect(commandData[1], port);
                        }
                        else Log.Info("Invalid connect command. Use connect [host name] [(optional)port]", cmd);
                        return true;

                    case "light":
                        if (commandData.Length > 1) new LightController(this).Activate(commandData[1]);
                        else Log.Info("Invalid light command. Use light [map name]", cmd);
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