using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;

namespace BlackTournament.GameStates
{
    class MapState : BaseGamestate
    {
        private string _MapName;

        private View _View;
        private IEntity _Player;
        private Map _Map;


        public MapState(Core core, String map) : base(core, map)
        {
            if (String.IsNullOrEmpty(map)) throw new ArgumentNullException(nameof(map));
            _MapName = map;
        }

        protected override bool Load()
        {
            // Setup View
            _View = new View(new FloatRect(0, 0, _Core.DeviceSize.X, _Core.DeviceSize.Y));

            // TESTS:
            _Player = new BlackCoat.Entities.Shapes.Rectangle(_Core)
            {
                Size = new SFML.System.Vector2f(20, 40),
                Origin = new SFML.System.Vector2f(10, 20),
                Color = Color.Red,
                View = _View
            };
            Layer_Game.AddChild(_Player);
            

            // Load Map
            _Map = new Map(_Core, _MapName);
            _Map.View = _View;
            var result = _Map.Load();
            if (result)
            {
                Layer_BG.AddChild(_Map);
            }
            else
            {
                Log.Debug("failed to load map", _MapName);
            }
            return result;
        }

        protected override void Update(float deltaT)
        {
            //_View.Center = _Player.Position;
            //_View.Rotation = -_Player.Rotation;// cool effect - see later if this can be used
        }

        protected override void Destroy()
        {
            Layer_BG.RemoveChild(_Map);
            _Map.Destroy();
        }

        public void Spawn(IEntity entity)
        {
            Layer_Game.AddChild(entity);
        }

        public void UpdatePosition(int id, float x, float y, float angle)
        {
            _Player.Position = new Vector2f(x, y);
            _Player.Rotation = angle;
            _View.Center = _Player.Position;
        }

        public override string ToString()
        {
            return String.Concat("Map: \"", Name, "\"");
        }
    }
}