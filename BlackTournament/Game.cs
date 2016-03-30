using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using TiledSharp;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace BlackTournament
{
    public class Game
    {
        public static void Main(string[] args)
        {
            var core = new Core(Core.DefaultDevice, true);
            var map = new Map(core, "Maps\\SpaceArena.tmx");
            map.Load();
            core.Layer_BG.AddChild(map);

            var player = new Player(core);
            core.Layer_Game.AddChild(player);

            map.View = new SFML.Graphics.View(new FloatRect(0, 0, core.DefaultView.Size.X / 3, core.DefaultView.Size.Y / 3));

            core.OnUpdate += deltaT =>
            {
                map.View.Center = player.Position;
            };

            core.Run();
        }
    }
}