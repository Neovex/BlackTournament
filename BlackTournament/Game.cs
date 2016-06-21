using System;
using System.IO;

using SFML.System;
using SFML.Window;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.Entities.Shapes;

using BlackTournament.Properties;
using BlackTournament.GameStates;
using BlackTournament.Net;


namespace BlackTournament
{
    public class Game
    {
        public static Font DefaultGameFont = null;
        private static readonly String DefaultGameFontName = "HighlandGothicLightFLF";
        private const String DefaultHost = "localhost";
        private const uint DefaultPort = 123;

        private static Core _Core;
        private static FontManager _GlobalFonts;

        private static NetworkManager _NetworkManager;


        public static void Main(string[] args)
        {
            // Init Black Coat Engine
            var device = Core.CreateDevice(800, 600, "Black Tournament", Styles.Close, 8);
            using (_Core = new Core(device))
            {
                _Core.Debug = true;
                Log.OnLog += m => File.AppendAllText("Log.txt", m + Environment.NewLine);
                Log.Debug(String.Empty);
                Log.Debug("################", "New Session:", DateTime.Now.ToLongTimeString(), "################");
                // todo: test text blur issue (might need round)
                // Init Game
                _GlobalFonts = new FontManager();
                DefaultGameFont = _GlobalFonts.Load(DefaultGameFontName, Resources.HighlandGothicLightFLF);
                _Core.ConsoleCommand += Execute;
                _NetworkManager = new NetworkManager();

                // Start Game
                _Core.StateManager.ChangeState(new Intro(_Core));
                //_Core.StateManager.ChangeState(new BlackCoatIntro(_Core, new Intro(_Core)));


                /*_Other = new Rectangle(_Core);
                _Other.View = zoomView;
                _Other.Size = new Vector2f(15, 15);
                _Other.Color = Color.Blue;
                _Core.Layer_Game.AddChild(_Other);*/


                _Core.Run();
                _GlobalFonts.Release(DefaultGameFontName);
            }
        }

        static bool Execute(string cmd)
        {
            var commandData = cmd.Split(' ');
            try
            {
                switch (commandData[0].ToLower())
                {
                    case "ld":
                    case "load":
                    case "loadmap":
                        if (commandData.Length == 2)
                        {
                            _Core.StateManager.ChangeState(new ConnectState(_Core, _NetworkManager, DefaultHost, DefaultPort));
                        }
                        else
                        {
                            Log.Debug("invalid usage of loadmap filename", cmd);
                        }
                        return true;

                    case "con":
                    case "connect":
                        if (commandData.Length == 3)
                        {
                            _Core.StateManager.ChangeState(new ConnectState(_Core, _NetworkManager, commandData[1], UInt32.Parse(commandData[2])));
                            return true;
                        }
                        Log.Debug("Could not connect", cmd);
                    return true;

                    case "startserver":
                    case "start server":
                        var port = commandData.Length == 2 ? UInt32.Parse(commandData[1]) : DefaultPort;
                        _Core.StateManager.ChangeState(new ConnectState(_Core, _NetworkManager, DefaultHost, port));
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
    }
}