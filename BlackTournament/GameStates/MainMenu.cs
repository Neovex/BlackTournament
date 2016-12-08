﻿using System;
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

        private const String SHADER_SRC = "uniform sampler2D texture;uniform float blur_radius;void main(){	vec2 offx = vec2(blur_radius, 0.0);	vec2 offy = vec2(0.0, blur_radius);	vec4 pixel = texture2D(texture, gl_TexCoord[0].xy)               * 4.0 +				 texture2D(texture, gl_TexCoord[0].xy - offx)        * 2.0 +				 texture2D(texture, gl_TexCoord[0].xy + offx)        * 2.0 +				 texture2D(texture, gl_TexCoord[0].xy - offy)        * 2.0 +				 texture2D(texture, gl_TexCoord[0].xy + offy)        * 2.0 +				 texture2D(texture, gl_TexCoord[0].xy - offx - offy) * 1.0 +				 texture2D(texture, gl_TexCoord[0].xy - offx + offy) * 1.0 +				 texture2D(texture, gl_TexCoord[0].xy + offx - offy) * 1.0 +				 texture2D(texture, gl_TexCoord[0].xy + offx + offy) * 1.0;	gl_FragColor =  gl_Color * (pixel / 16.0);}";

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

            //Test();

            MusicManager.RootFolder = "music";
            music = MusicManager.Load("Ten_Seconds_to_Rush");
            music.Volume = 15;
            //music.Play();

            return true;
        }

        private void Test()
        {
            TextureManager.RootFolder = "Assets";
            var tex = TextureManager.Load("AztekTiles");
            var strm = GenerateStreamFromString(SHADER_SRC);
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

        private MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value));
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