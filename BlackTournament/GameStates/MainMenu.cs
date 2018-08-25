using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;
using BlackCoat.Entities.Shapes;
using BlackTournament.Entities;

namespace BlackTournament.GameStates
{
    class MainMenu:Gamestate
    {
        private GameText _Text;

        private Shader _Shader;
        private Graphic _BlurTest;
        private float _Blurryness = 0;
        
        private SFML.Audio.Music music;
        private Line _Ray;
        private Circle _Circle;

        public MainMenu(Core core):base(core, nameof(MainMenu), Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
            // TODO: cleanup testing stuff
        }

        protected override bool Load()
        {
            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(300, 100);
            _Text.Text = "MAIN MENU";
            Layer_Game.AddChild(_Text);

            //IntersectionTest();
            //ShaderTest();
            
            music = MusicLoader.Load("Ten_Seconds_to_Rush");
            music.Volume = 15;
            //music.Play();

            return true;
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

            _Core.Input.MouseButtonPressed += Input_MouseButtonPressed;
        }

        private void Input_MouseButtonPressed(SFML.Window.Mouse.Button obj)
        {
            if (obj == SFML.Window.Mouse.Button.Left)
            {
                var intersections = _Core.CollisionSystem.Raycast(_Ray.Start.Position, _Ray.Start.Position.AngleTowards(_Ray.End.Position), _Circle);
                foreach (var intersect in intersections)
                {
                    var r = new Rectangle(_Core)
                    {
                        Size = new Vector2f(10, 10),
                        Origin = new Vector2f(5, 5),
                        Position = intersect,
                        Color = Color.Transparent,
                        OutlineColor = Color.Red,
                        OutlineThickness = 0.5f
                    };
                    Layer_Game.AddChild(r);
                    _Core.AnimationManager.Run(0, 90, 0.5f, v => r.Rotation = v, () => Layer_Game.RemoveChild(r));
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
            Log.Fatal("Shader Available: ", Shader.IsAvailable); // fixme

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
                if (k == SFML.Window.Keyboard.Key.Up)
                {
                    _Blurryness += 0.001f;
                    Log.Debug(_Blurryness);
                }
                else if (k == SFML.Window.Keyboard.Key.Down)
                {
                    _Blurryness -= 0.001f;
                    Log.Debug(_Blurryness);
                }
                _Shader.SetParameter("blur_radius", _Blurryness);
            };
        }

        internal void DisplayPopupMessage(string message)
        {
            Log.Debug(message);
        }

        protected override void Update(float deltaT)
        {
            //_Ray.End.Position = VectorExtensions.VectorFromAngle(_Ray.Start.Position.AngleTowards(_Core.Input.MousePosition), 1000).ToGlobal(_Ray.Start.Position);
        }

        protected override void Destroy()
        {
            music?.Dispose();
            // todo: find better destroy / music logic
            //_Core.AnimationManager.Run(music.Volume, 0, 1, v => music.Volume = v, BlackCoat.Animation.InterpolationType.Linear, a => music = null);
        }
    }
}