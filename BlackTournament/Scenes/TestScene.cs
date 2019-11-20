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
using BlackTournament.Tmx;
using BlackCoat.Collision.Shapes;
using BlackCoat.Collision;
using BlackCoat.Entities.Lights;
using BlackTournament.UI;

namespace BlackTournament.Scenes
{
    class TestScene : Scene
    {
        private Game _Game;
        private Shader _Shader;
        private Graphic _BlurTest;
        private float _Blurryness = 0;
        private Line _Ray;
        private Circle _Circle;
        private Line _Line;
        private float _LineAngle = 320;
        private Rectangle _Rect;
        private IEntity _Test;

        public TestScene(Game game) : base(game.Core, "TEST", Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
            _Game = game;
        }

        protected override bool Load()
        {
            //IntersectionTest();
            //ShaderTest();
            UiTests();
            //TextureTests();
            //CollisionTest();
            //Particles();
            //PreRenderVSParticles();
            Log.Info("Nobody here but us chickens");
            return true;
        }

        protected override void Update(float deltaT)
        {

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

        private void UiTests()
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
                //DialogContainer = Layer_Overlay,
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

                    new OffsetContainer(_Core, true)
                    {
                        BackgroundAlpha = 0.4f,
                        Position = new Vector2f(200, 200),
                        Init = labels.Skip(1).Take(3)
                    },

                    new Canvas(_Core, new Vector2f(20, 20))
                    {
                        BackgroundAlpha = 0.4f,
                        Margin=new FloatRect(10,10,10,10),
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
                        Padding = new FloatRect(5,5,5,5),
                        MinSize = new Vector2f(150,20),
                        Font = Game.DefaultFont,
                        InitInEditChanged = TextboxEditChanged
                    }
                }
            };
            Layer_Game.Add(container);

            OpenInspector();
        }

        private void TextboxEditChanged(TextBox sender)
        {
            if (!sender.InEdit) return;

            var d = new ComboBoxDialog(_Core, sender, new[] { "Banana", "Apple", "Dog", "Whatever" });
            /*
            var c = new OffsetContainer(_Core, false)
            {
                Position = sender.GlobalPosition + new Vector2f(0, sender.OuterSize.Y),
                Offset = 10,
                Init = new UIComponent[]
                {
                    new Button(_Core, new Vector2f(100, 20))
                    {
                        BackgroundColor = Color.Yellow,
                        InitReleased = ButtonClick,
                        Tag = sender,
                        Init=new UIComponent[]
                        {
                            new Label(_Core, "Banana")
                        }
                    },
                    new Button(_Core, new Vector2f(100, 20))
                    {
                        BackgroundColor = Color.Red,
                        InitReleased = ButtonClick,
                        Tag = sender,
                        Init=new UIComponent[]
                        {
                            new Label(_Core, "Apple")
                        }
                    },
                    new Button(_Core, new Vector2f(100, 20))
                    {
                        BackgroundColor = Color.Green,
                        InitReleased = ButtonClick,
                        Tag = sender,
                        Init=new UIComponent[]
                        {
                            new Label(_Core, "Onion")
                        }
                    }
                }
            };*/
            
            sender.ShowDialog(Layer_Overlay, d);
        }

        private void ButtonClick(Button b)
        {
            (b.Tag as TextBox).Text = b.GetFirst<Label>().Text;
            b.CloseDialog();
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


        private void CollisionTest()
        {
            /*
            var mapper = new TmxMapper(() => 0);
            mapper.Load("turbine", _Core.CollisionSystem);

            foreach (var kz in mapper.Killzones.Skip(2).Take(2))
            {
                switch (kz.CollisionShape.CollisionGeometry)
                {
                    case BlackCoat.Collision.Geometry.Circle:
                        var c = kz.CollisionShape as CircleCollisionShape;
                        Layer_Overlay.Add(new Circle(_Core)
                        {
                            Radius = c.Radius
                        });
                        break;
                    case BlackCoat.Collision.Geometry.Rectangle:
                        var r = kz.CollisionShape as RectangleCollisionShape;
                        Layer_Overlay.Add(new Rectangle(_Core)
                        {
                            Size = r.Size
                        });
                        break;
                    case BlackCoat.Collision.Geometry.Polygon:
                        var p = kz.CollisionShape as PolygonCollisionShape;
                        Layer_Overlay.Add(new Polygon(_Core, p.Points)
                        {
                            Position = _Core.Random.NextVector(200,500)
                        });
                        _Test2 = p;
                        break;
                }
            }*/

            var points = new Vector2f[] { new Vector2f(), new Vector2f(100, -150), new Vector2f(200, 0) };
            Layer_Overlay.Add(new Polygon(_Core, points)
            {
                Position = new Vector2f(100, 100)
            });
            Layer_Overlay.Add(new Polygon(_Core, points.Reverse())
            {
                Position = new Vector2f(450, 100)
            });

            Layer_Overlay.Add( _Test = new Circle(_Core)
            {
                Radius = 50
            });

            Input.DEFAULT.MouseButtonPressed += DEFAULT_MouseButtonPressed;
        }

        private void DEFAULT_MouseButtonPressed(Mouse.Button obj)
        {
            Update(0);
        }

        private void Particles()
        {
            var host = new ParticleEmitterHost(_Core);
            Layer_Game.Add(host);
            var info = new PixelInfo(_Core);
            var emitter = new PixelEmitter(_Core, info);
            host.AddEmitter(emitter);
            emitter.Trigger();
        }

        private void PreRenderVSParticles()
        {
            var view = new View(new FloatRect(new Vector2f(), _Core.DeviceSize));

            var rContainer = new PrerenderedContainer(_Core, new Vector2f(3840, 3584));
            Layer_Game.Add(rContainer);
            rContainer.RenderEachFrame = true;

            //var lightTex = TextureLoader.Load(nameof(Resources.Pointlight), Resources.Pointlight);
            var lightTex = TextureLoader.Load("checker");
            var info = new TextureParticleInitializationInfo(lightTex)
            {
                TTL = 5,
                //Origin = lightTex.Size.ToVector2f() / 2,
                //Scale = new Vector2f(0.2f, 0.2f)
            };
            var emitter = new TextureEmitter(_Core, info);
            // Add to scene
            var host = new ParticleEmitterHost(_Core);
            host.AddEmitter(emitter);
            rContainer.Add(host);
            rContainer.Add(new Graphic(_Core, TextureLoader.Load("checker")) { Position = Create.Vector2f(100) });
            //Layer_Game.Add(host);

            Input.DEFAULT.MouseButtonPressed += b =>
            {
                //view.Center += Create.Vector2f(5);
                emitter.Position = Input.DEFAULT.MousePosition;
                emitter.Trigger();
                if(b== Mouse.Button.Middle)
                {
                    Layer_Game.View = view;
                }
            };
        }
    }
    class PixelInfo : PixelParticleInitializationInfo
    {
        private Core _Core;

        public override Vector2f Velocity { get => _Core.Random.NextVector(-10, +10, -100, -10); set => base.Acceleration = value; }
        public override Vector2f Offset { get => new Vector2f(_Core.Random.NextFloat(0, _Core.DeviceSize.X), _Core.DeviceSize.Y); set => base.Offset = value; }
        public override float AlphaFade { get => _Core.Random.NextFloat(-1,-0.1f); set => base.AlphaFade = value; }

        public PixelInfo(Core core)
        {
            _Core = core;
            TTL = 100;
            Loop = true;
            ParticlesPerSpawn = 10;
            SpawnRate = 0.01f;
            UseAlphaAsTTL = true;
            Color = Color.White;
        }
    }
}