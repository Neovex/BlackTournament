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
using BlackCoat.UI;
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
            //IntersectionTest();
            //ShaderTest();
            UiTest();
            //TextureTests();
            Log.Info("Nobody here but us chickens");
            return true;
        }

        protected override void Update(float deltaT)
        {
            //_Ray.End.Position = Create.Vector2fFromAngle(_Ray.Start.Position.AngleTowards(_Core.Input.MousePosition), 1000).ToGlobal(_Ray.Start.Position);
        }

        protected override void Destroy()
        {
        }



        private void IntersectionTest()
        {
            _Ray = new Line(_Core, new Vector2f(300, 300), new Vector2f(), Color.Green);
            Layer_Game.Add(_Ray);

            _Circle = new Circle(_Core)
            {
                Position = _Ray.Start.Position + new Vector2f(150, 80),
                Radius = 50,
                Color = Color.Transparent,
                OutlineColor = Color.Cyan,
                OutlineThickness = 0.5f
            };
            Layer_Game.Add(_Circle);


            _Rect = new Rectangle(_Core)
            {
                Position = new Vector2f(450, 100),
                Size = Create.Vector2f(200),
                Color = Color.Yellow,
                Alpha = 0.3f
            };
            Layer_Game.Add(_Rect);

            _Line = new Line(_Core, _Circle.Position, _Circle.Position + Create.Vector2fFromAngle(_LineAngle, 230), Color.Blue);
            Layer_Game.Add(_Line);

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
                    Layer_Game.Add(r);

                    var l = new Line(_Core, position, position + Create.Vector2fFromAngle(MathHelper.CalculateReflectionAngle(rayAngle, angle), 100), Color.Cyan);
                    Layer_Game.Add(l);

                    _Core.AnimationManager.Run(0, 2000, 5f, v => r.Rotation = v, () => { Layer_Game.Remove(r); Layer_Game.Remove(l); });
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
            Layer_Game.Add(_BlurTest);

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

        private void UiTest()
        {
            var labels = new List<Label>();
            for (int i = 0; i < 10; i++)
            {
                labels.Add(new Label(_Core, "Banana " + i)
                {
                    CharacterSize = 14
                });
            }


            var container = new UIContainer(_Core)
            {
                Input = new UIInput(_Core.Input, true),
                BackgroundColor = new Color(100, 100, 100),
                BackgroundAlpha = 1,
                Init = new UIComponent[]
                {
                    new AnchoredContainer(_Core, Anchor.TopRight)
                    {
                        BackgroundAlpha = 0.4f,
                        Init = labels.Take(1)
                    },
                    
                    new OffsetContainer(_Core)
                    {
                        BackgroundAlpha = 0.4f,
                        Position = new Vector2f(200, 200),
                        Init = labels.Skip(1).Take(3)
                    },
                    
                    new Canvas(_Core, new Vector2f(20, 20))
                    {
                        BackgroundAlpha = 0.4f,
                        Padding=new FloatRect(10,10,10,10),
                        DockX = true,
                        DockY = true,
                        Init = new UIComponent[]
                        {
                            labels.Skip(4).First(),
                            new DistributionContainer(_Core, true)
                            {
                                BackgroundAlpha = 0.4f,
                                Position = new Vector2f(100, 100),
                                Init = labels.Skip(5).Take(4)
                            }
                        }
                    },

                    new TextBox(_Core)
                    {
                        Position = new Vector2f(50,150),
                        InnerPadding = new FloatRect(5,5,5,5)
                    }
                }
            };
            Layer_Game.Add(container);


            OpenInspector();
        }

        private void TextureTests()
        {
            var tex = TextureLoader.Load(Files.Emitter_Smoke_White);
            Log.Debug(tex.Repeated);
            var graphic = new Graphic(_Core, tex)
            {
                Position = new Vector2f(40,100)
            };
            Layer_Game.Add( new Graphic(_Core, tex)
            {
                Position = new Vector2f(240, 100)
            });

            var renderTexture = new PrerenderedContainer(_Core);
            renderTexture.Add(graphic);
            renderTexture.RedrawNow();
            Layer_Game.Add(renderTexture);

        }
    }
}