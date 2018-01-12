using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities.Shapes;
using SFML.System;

namespace BlackTournament.GameStates
{
    class TestState : BaseGamestate
    {
        private Line _Line;
        private Line[] _Lines;
        private Circle _Circle;

        public TestState(Core core) : base(core)
        {
            _Lines = new Line[1];
        }

        protected override bool Load()
        {
            Layer_Overlay.AddChild(_Line = new Line(_Core, new Vector2f(), new Vector2f(), Color.Red));
            _Line.Start.Color = Color.Yellow;

            for (int i = 0; i < _Lines.Length; i++)
            {
                _Lines[i] = new Line(_Core, _Core.Random.NextVector(0, _Core.DeviceSize.X), _Core.Random.NextVector(0, _Core.DeviceSize.X), Color.Cyan);
                Layer_Game.AddChild(_Lines[i]);
            }

            _Circle = new Circle(_Core)
            {
                OutlineThickness = 1,
                OutlineColor = Color.White,
                FillColor = Color.Transparent,
                Radius = 200,
                Position = _Core.DeviceSize.ToVector2f() / 2
            };
            Layer_Game.AddChild(_Circle);

            Input.MouseButtonPressed += Input_MouseButtonPressed;

            return true;
        }

        private void Input_MouseButtonPressed(SFML.Window.Mouse.Button btn)
        {
            switch (btn)
            {
                case SFML.Window.Mouse.Button.Left:
                    SetPoint(Input.MousePosition);
                    break;
                case SFML.Window.Mouse.Button.Right:
                    RandomizeLines();
                    break;
                case SFML.Window.Mouse.Button.Middle:
                    break;
                case SFML.Window.Mouse.Button.XButton1:
                    break;
                case SFML.Window.Mouse.Button.XButton2:
                    break;
                case SFML.Window.Mouse.Button.ButtonCount:
                    break;
                default:
                    break;
            }
        }

        private void SetPoint(Vector2f mousePosition)
        {
            Layer_Particles.Clear();

            if (Input.IsKeyDown(SFML.Window.Keyboard.Key.LControl))
                 _Line.Start.Position = mousePosition;
            else _Line.End.Position = mousePosition;

            foreach (var intersect in _Lines.SelectMany(ln => _Core.CollisionSystem.Raycast(_Line.Start.Position, _Line.Start.Position.AngleTowards(_Line.End.Position), ln)))
            {
                Layer_Particles.AddChild(new Rectangle(_Core) { Size = new Vector2f(3, 8), Color = Color.Magenta, Position = intersect });
            }

            foreach (var intersect in _Core.CollisionSystem.Raycast(_Line.Start.Position, _Line.Start.Position.AngleTowards(_Line.End.Position), _Circle))
            {
                Layer_Particles.AddChild(new Rectangle(_Core) { Size = new Vector2f(3, 8), Color = Color.Yellow, Position = intersect });
            }

        }

        private void RandomizeLines()
        {
            foreach (var line in _Lines)
            {
                line.Start.Position = _Core.Random.NextVector(0, _Core.DeviceSize.X);
                line.End.Position = _Core.Random.NextVector(0, _Core.DeviceSize.X);
            }
        }

        protected override void Update(float deltaT)
        {
        }

        protected override void Destroy()
        {
        }
    }
}
