using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using SFML.Graphics;
using BlackTournament.Net;

namespace BlackTournament.GameStates
{
    class MapState : BaseGamestate
    {
        private string _MapName;

        private View _View;
        private Player _Player; // TODO : replace with proper entitiy management
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

            _Player = new Player(_Core, TextureManager);
            _Player.View = _View;
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
            _View.Center = _Player.Position;
            //zoomView.Rotation = -_Player.Rotation;// cool effect - see later if this can be used
        }

        protected override void Destroy()
        {
            Layer_BG.RemoveChild(_Map);
            _Map.Destroy();
        }

        public override string ToString()
        {
            return String.Concat("Map: \"", Name, "\"");
        }
    }
}