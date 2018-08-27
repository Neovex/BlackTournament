using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.System;
using SFML.Audio;
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
        
        private Music music;

        public MainMenu(Core core):base(core, nameof(MainMenu), Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(300, 100);
            _Text.Text = "MAIN MENU";
            Layer_Game.AddChild(_Text);
            
            music = MusicLoader.Load("Ten_Seconds_to_Rush");
            music.Volume = 15;
            //music.Play();

            return true;
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
            // todo: find better destroy / music logic - I think this is already done by the music asset manager but better double check
            //_Core.AnimationManager.Run(music.Volume, 0, 1, v => music.Volume = v, BlackCoat.Animation.InterpolationType.Linear, a => music = null);
        }
    }
}