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
        private Graphic _Player;
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
            TextureManager.RootFolder = "Assets";
            _Player = new Graphic(_Core)
            {
                Texture = TextureManager.Load("CharacterBase"),
                Color = Color.Red,
                View = _View,
                Scale = new Vector2f(0.5f, 0.5f) // FIXME
            };
            _Player.Origin = _Player.Texture.Size.ToVector2f() / 2;
            Layer_Game.AddChild(_Player);

            // Load Map
            _Map = new Map(_Core, _MapName);
            _Map.View = _View;
            var mapLoaded = _Map.Load();
            if (mapLoaded)
            {
                Layer_BG.AddChild(_Map);
                Log.Debug(_Map.Polys.Count);
                foreach (var entity in _Map.Polys)
                {
                    entity.View = _View;
                    entity.Alpha = 0.5f;
                    Layer_BG.AddChild(entity);
                }
            }
            else
            {
                Log.Debug("failed to load map", _MapName);
            }
            return mapLoaded;
        }

        protected override void Update(float deltaT)
        {
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

        public void UpdatePosition(int id, float x, float y)
        {
            _Player.Position = new Vector2f(x, y);
            _View.Center = _Player.Position;
        }

        public void Rotate(float angle)
        {
            _Player.Rotation = angle;
            //_View.Rotation = -_Player.Rotation; // cool effect - see later if this can be used (with gamepads maybe)
        }

        public override string ToString()
        {
            return $"Map: \"{Name}\"";
        }
    }
}