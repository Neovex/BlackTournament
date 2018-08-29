using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;
using BlackTournament.Particles;
using BlackCoat.Entities.Shapes;
using BlackCoat.Entities;
using BlackTournament.Systems;
using BlackCoat.Tools;
using SFML.Window;

namespace BlackTournament.GameStates
{
    class TestState : Gamestate
    {
        private Shader _Shader;
        private Graphic _BlurTest;
        private float _Blurryness = 0;
        private Line _Ray;
        private Circle _Circle;
        private Line _Line;
        private float _LineAngle = 320;
        private Rectangle _Rect;

        public TestState(Core core) : base(core, "TEST", Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            IntersectionTest();
            //ShaderTest();
            return true;
        }

        protected override void Update(float deltaT)
        {
            _Ray.End.Position = Create.Vector2fFromAngle(_Ray.Start.Position.AngleTowards(_Core.Input.MousePosition), 1000).ToGlobal(_Ray.Start.Position);
        }

        protected override void Destroy()
        {
        }



        private void IntersectionTest()
        {
            _Ray = new Line(_Core, new Vector2f(300, 300), new Vector2f(), Color.Green);
            Layer_Game.AddChild(_Ray);

            _Circle = new Circle(_Core)
            {
                Position = _Ray.Start.Position + new Vector2f(150, 80),
                Radius = 50,
                Color = Color.Transparent,
                OutlineColor = Color.Cyan,
                OutlineThickness = 0.5f
            };
            Layer_Game.AddChild(_Circle);


            _Rect = new Rectangle(_Core)
            {
                Position = new Vector2f(450, 100),
                Size = Create.Vector2f(200),
                Color = Color.Yellow,
                Alpha = 0.3f
            };
            Layer_Game.AddChild(_Rect);

            _Line = new Line(_Core, _Circle.Position, _Circle.Position + Create.Vector2fFromAngle(_LineAngle, 230), Color.Blue);
            Layer_Game.AddChild(_Line);

            _Core.Input.MouseButtonPressed += RayMouseButtonPressed;
        }

        private void RayMouseButtonPressed(Mouse.Button btn)
        {
            if (btn == Mouse.Button.Left)
            {
                float rayAngle = _Ray.Start.Position.AngleTowards(_Ray.End.Position);
                Log.Info("Ray Angle:", rayAngle);
                var intersections = _Core.CollisionSystem.Raycast(_Ray.Start.Position, rayAngle, _Rect)
                            .Concat(_Core.CollisionSystem.Raycast(_Ray.Start.Position, rayAngle, _Line));

                foreach (var (position, angle) in intersections)
                {
                    Log.Info("Intersection Angle:", angle);

                    var r = new Rectangle(_Core)
                    {
                        Size = new Vector2f(10, 10),
                        Origin = new Vector2f(5, 5),
                        Position = position,
                        Color = Color.Red,
                        Alpha = 0.75f
                    };
                    Layer_Game.AddChild(r);

                    var l = new Line(_Core, position, position+ Create.Vector2fFromAngle(MathHelper.CalculateReflectionAngle(rayAngle, angle), 100), Color.Cyan);
                    Layer_Game.AddChild(l);

                    _Core.AnimationManager.Run(0, 2000, 5f, v => r.Rotation = v, () => { Layer_Game.RemoveChild(r); Layer_Game.RemoveChild(l); });
                }
            }
            else
            {
                _Ray.Start.Position = _Core.Input.MousePosition;
            }
        }

        private void ShaderTest()
        {
            TextureLoader.RootFolder = "Assets";
            var tex = TextureLoader.Load("AztekTiles");
            Log.Fatal("Shader Available: ", Shader.IsAvailable); // fixme - isnt this done already?

            _Shader = new Shader(null, @"C:\Users\Fox\Desktop\blur.frag"); // Wrap into effect class?
            _Shader.SetParameter("texture", tex);
            _Shader.SetParameter("blur_radius", _Blurryness);

            _BlurTest = new Graphic(_Core);
            _BlurTest.Position = new Vector2f(100, 100);
            _BlurTest.Texture = tex;
            var state = _BlurTest.RenderState;
            state.Shader = _Shader;
            _BlurTest.RenderState = state;
            Layer_Game.AddChild(_BlurTest);

            _Core.Input.KeyPressed += (k) =>
            {
                if (k == Keyboard.Key.Up)
                {
                    _Blurryness += 0.001f;
                    Log.Debug(_Blurryness);
                }
                else if (k == Keyboard.Key.Down)
                {
                    _Blurryness -= 0.001f;
                    Log.Debug(_Blurryness);
                }
                _Shader.SetParameter("blur_radius", _Blurryness);
            };
        }
    }
}