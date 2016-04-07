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
        public const int PLAYER_MAX_SPEED = 125;
        public const float PLAYER_ACCELERATION = 750f;

        private Vector2f Direction;
        private ValueAnimation _VerticalAccellerator;
        private ValueAnimation _HorizontalAccellerator;

        public Player(Core core):base(core)
        {
            Texture = _Core.AssetManager.CreateTexture(10, 10, 0xFFFF0000, "PlayerDummy");
            Origin = (Vector2f)(Texture.Size / 2);
            Input.KeyPressed += HandleKeyDown;
            Input.KeyReleased += HandleKeyUp;
        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);

            Position += Direction * deltaT;
            Rotation = new Vector2f(_Core.DeviceSize.X / 2, _Core.DeviceSize.Y / 2).AngleTowards(Input.MousePosition);
        }

        private void HandleKeyDown(KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.W)
            {
                if (_VerticalAccellerator != null) _VerticalAccellerator.Cancel();
                _VerticalAccellerator = _Core.AnimationManager.Run(() => Direction.Y, v => Direction = new Vector2f(Direction.X, v), -PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
            else if (args.Code == Keyboard.Key.S)
            {
                if (_VerticalAccellerator != null) _VerticalAccellerator.Cancel();
                _VerticalAccellerator = _Core.AnimationManager.Run(() => Direction.Y, v => Direction = new Vector2f(Direction.X, v), PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
            if (args.Code == Keyboard.Key.A)
            {
                if (_HorizontalAccellerator != null) _HorizontalAccellerator.Cancel();
                _HorizontalAccellerator = _Core.AnimationManager.Run(() => Direction.X, v => Direction = new Vector2f(v, Direction.Y), -PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
            else if (args.Code == Keyboard.Key.D)
            {
                if (_HorizontalAccellerator != null) _HorizontalAccellerator.Cancel();
                _HorizontalAccellerator = _Core.AnimationManager.Run(() => Direction.X, v => Direction = new Vector2f(v, Direction.Y), PLAYER_MAX_SPEED, PLAYER_ACCELERATION);
            }
        }

        private void HandleKeyUp(KeyEventArgs args)
        {
            if (args.Code == Keyboard.Key.W || args.Code == Keyboard.Key.S)
            {
                if (_VerticalAccellerator != null)
                {
                    if (args.Code == Keyboard.Key.W && _VerticalAccellerator.TargetValue > 0) return;
                    if (args.Code == Keyboard.Key.S && _VerticalAccellerator.TargetValue < 0) return;
                    _VerticalAccellerator.Cancel();
                }
                _VerticalAccellerator = _Core.AnimationManager.Run(() => Direction.Y, v => Direction = new Vector2f(Direction.X, v), 0, PLAYER_ACCELERATION);
            }

            if (args.Code == Keyboard.Key.A || args.Code == Keyboard.Key.D)
            {
                if (_HorizontalAccellerator != null)
                {
                    if (args.Code == Keyboard.Key.A && _HorizontalAccellerator.TargetValue > 0) return;
                    if (args.Code == Keyboard.Key.D && _HorizontalAccellerator.TargetValue < 0) return;
                    _HorizontalAccellerator.Cancel();
                }
                _HorizontalAccellerator = _Core.AnimationManager.Run(() => Direction.X, v => Direction = new Vector2f(v, Direction.Y), 0, PLAYER_ACCELERATION);
            }
        }
    }
}