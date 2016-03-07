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
            core.Layer_Game.AddChild(map);

            map.View = new SFML.Graphics.View(new FloatRect(0, 0, core.DefaultView.Size.X / 2, core.DefaultView.Size.Y / 2));

            core.OnUpdate += deltaT =>
            {
                var speed = 400;
                if (Input.IsKeyDown(Keyboard.Key.Left))
                {
                    map.View.Center += new Vector2f(speed * -deltaT, 0);
                }
                if (Input.IsKeyDown(Keyboard.Key.Right))
                {
                    map.View.Center += new Vector2f(speed * deltaT, 0);
                }
                if (Input.IsKeyDown(Keyboard.Key.Up))
                {
                    map.View.Center += new Vector2f(0, speed * -deltaT);
                }
                if (Input.IsKeyDown(Keyboard.Key.Down))
                {
                    map.View.Center += new Vector2f(0, speed * deltaT);
                }
            };

            core.Run();
        }
    }
}