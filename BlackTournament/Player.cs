using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat.Entities;
using BlackCoat;
using SFML.Window;
using BlackCoat.Animation;
using BlackCoat.Tools;
using SFML.System;

namespace BlackTournament
{
    class Player : Graphic
    {
        public const int PLAYER_MAX_SPEED = 400;
        public const float PLAYER_ACCELERATION = 850f;

        private Vector2f Direction;
        private ValueAnimation _VerticalAccellerator;
        private ValueAnimation _HorizontalAccellerator;

        public Player(Core core, TextureManager textures):base(core)
        {
            Texture = textures.CreateTexture(10, 10, 0xFFFF0000, "PlayerDummy");
            Origin = (Vector2f)(Texture.Size / 2);
            Input.KeyPressed += HandleKeyDown;
            Input.KeyReleased += HandleKeyUp;
            Input.MouseWheelScrolled += HandleMouseWheel;
        }

        private void HandleMouseWheel(float delta)
        {
            View.Zoom(1 - delta / 10);
        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);

            Position += Direction * deltaT;
            Rotation = new Vector2f(_Core.DeviceSize.X / 2, _Core.DeviceSize.Y / 2).AngleTowards(Input.MousePosition);
        }

        private void HandleKeyDown(Keyboard.Key key)
        {
            if (key == Keyboard.Key.W)
            {
                if (_VerticalAccellerator != null) _VerticalAccellerator.Cancel();
                _VerticalAccellerator = _Core.AnimationManager.Run(() => Direction.Y, v => Direction = new Vector2f(Direction.X, v), -PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
            else if (key == Keyboard.Key.S)
            {
                if (_VerticalAccellerator != null) _VerticalAccellerator.Cancel();
                _VerticalAccellerator = _Core.AnimationManager.Run(() => Direction.Y, v => Direction = new Vector2f(Direction.X, v), PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
            if (key == Keyboard.Key.A)
            {
                if (_HorizontalAccellerator != null) _HorizontalAccellerator.Cancel();
                _HorizontalAccellerator = _Core.AnimationManager.Run(() => Direction.X, v => Direction = new Vector2f(v, Direction.Y), -PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
            else if (key == Keyboard.Key.D)
            {
                if (_HorizontalAccellerator != null) _HorizontalAccellerator.Cancel();
                _HorizontalAccellerator = _Core.AnimationManager.Run(() => Direction.X, v => Direction = new Vector2f(v, Direction.Y), PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
        }

        private void HandleKeyUp(Keyboard.Key key)
        {
            if (key == Keyboard.Key.W || key == Keyboard.Key.S)
            {
                if (_VerticalAccellerator != null)
                {
                    if (key == Keyboard.Key.W && _VerticalAccellerator.TargetValue > 0) return;
                    if (key == Keyboard.Key.S && _VerticalAccellerator.TargetValue < 0) return;
                    _VerticalAccellerator.Cancel();
                }
                _VerticalAccellerator = _Core.AnimationManager.Run(() => Direction.Y, v => Direction = new Vector2f(Direction.X, v), 0, PLAYER_ACCELERATION);
            }

            if (key == Keyboard.Key.A || key == Keyboard.Key.D)
            {
                if (_HorizontalAccellerator != null)
                {
                    if (key == Keyboard.Key.A && _HorizontalAccellerator.TargetValue > 0) return;
                    if (key == Keyboard.Key.D && _HorizontalAccellerator.TargetValue < 0) return;
                    _HorizontalAccellerator.Cancel();
                }
                _HorizontalAccellerator = _Core.AnimationManager.Run(() => Direction.X, v => Direction = new Vector2f(v, Direction.Y), 0, PLAYER_ACCELERATION);
            }
        }
    }
}