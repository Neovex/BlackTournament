using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using BlackTournament.Entities;
using SFML.System;
using BlackCoat.Entities;
using SFML.Graphics;
using System.IO;

namespace BlackTournament.GameStates
{
    class MainMenu:BaseGamestate
    {
        private GameText _Text;

        private Shader _Shader;
        private Graphic _BlurTest;
        private float _Blurryness = 0;
        
        private SFML.Audio.Music music;

        public MainMenu(Core core):base(core)
        {
            // TODO: cleanup shader stuff
        }

        protected override bool Load()
        {
            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(300, 100);
            _Text.Text = "MAIN MENU";
            Layer_Game.AddChild(_Text);

            //ShaderTest();

            MusicManager.RootFolder = "music";
            music = MusicManager.Load("Ten_Seconds_to_Rush");
            music.Volume = 15;
            //music.Play();

            return true;
        }

        private void ShaderTest()
        {
            TextureManager.RootFolder = "Assets";
            var tex = TextureManager.Load("AztekTiles");
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

            Input.KeyPressed += (k) =>
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
        }

        protected override void Destroy()
        {
            music?.Dispose();
            // todo: find better destroy / music logic
            //_Core.AnimationManager.Run(music.Volume, 0, 1, v => music.Volume = v, BlackCoat.Animation.InterpolationType.Linear, a => music = null);
        }
    }
}