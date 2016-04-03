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

namespace BlackTournament
{
    public class Game
    {
        private static Server _Server;
        private static Client _Client;
        private static Core _Core;
        private static Player _Player;
        private static Rectangle _Other;

        public static void Main(string[] args)
        {
            _Core = new Core(Core.DefaultDevice, true);

            var zoomView = new SFML.Graphics.View(new FloatRect(0, 0, _Core.DefaultView.Size.X / 3, _Core.DefaultView.Size.Y / 3));

            var map = new Map(_Core, "Maps\\SpaceArena.tmx");
            map.View = zoomView;
            map.Load();
            _Core.Layer_BG.AddChild(map);

            _Player = new Player(_Core);
            // fix me map view punches through to other entities
            _Core.Layer_Game.AddChild(_Player);

            _Other = new Rectangle(_Core);
            _Other.Size = new Vector2f(15, 15);
            _Other.Color = Color.Blue;
            _Core.Layer_Game.AddChild(_Other);

            _Core.OnUpdate += deltaT => zoomView.Center = _Player.Position;
            // zoomView.Rotation = -_Player.Rotation; cool effect - see later if this can be used
            _Core.ConsoleCommand += HandleConsoleCommand;

            

            _Core.Run();
        }

        static bool HandleConsoleCommand(string cmd)
        {
            var cmds = cmd.Split(' ');
            try
            {
                switch (cmds[0])
                {
                    case "startserver":
                    case "start server":
                        if (cmds.Length == 2)
                        {
                            _Server = new Server();
                            var port = UInt32.Parse(cmds[1]);
                            _Server.Host(port);
                            _Server.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
                            TrasmittMove(_Server);
                            _Core.Log("Host is listening on", port);
                            return true;
                        }
                        _Core.Log("Invalid host data", cmd);
                    return true;

                    case "connect":
                        if (cmds.Length == 3)
                        {
                            _Client = new Client();
                            var port = UInt32.Parse(cmds[2]);
                            _Client.Connect(cmds[1], port);
                            _Client.Moved += (x, y) => _Other.Position = new Vector2f(x, y);
                            TrasmittMove(_Client);
                            _Core.Log("Connection to", cmds[1], ":", port);
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

        private static void TrasmittMove(Server server)
        {
            server.BroadcastMove(0, _Player.Position.X, _Player.Position.Y);
            _Core.AnimationManager.Wait(1 / 60f, a => TrasmittMove(server));
        }

        private static void TrasmittMove(Client client)
        {
            client.DoMove(0, _Player.Position.X, _Player.Position.Y);
            _Core.AnimationManager.Wait(1 / 60f, a => TrasmittMove(client));
        }
    }
}