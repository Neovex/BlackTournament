﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using SFML.Graphics;

namespace BlackTournament.GameStates
{
    class MapState : BaseGameState
    {
        private string _MapName;

        private View _View;
        private Player _Player;
        private Map _Map;


        public MapState(Core core, String map) : base(core)
        {
            _MapName = map;
        }

        public override bool Load()
        {

            // Setup View
            _View = new View(new FloatRect(0, 0, _Core.DeviceSize.X / 3, _Core.DeviceSize.Y / 3));

            _Player = new Player(_Core);
            //_Player.View = ; find a solution for this
            Layer_Game.AddChild(_Player);

            // Load Map
            _Map = new Map(_Core, "Maps\\" + _MapName);
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

        public override void Update(float deltaT)
        {
            //_View.Center = _Player.Position;
            //zoomView.Rotation = -_Player.Rotation;// cool effect - see later if this can be used
        }

        public override void Destroy()
        {
            Layer_BG.RemoveChild(_Map);
        }
    }
}