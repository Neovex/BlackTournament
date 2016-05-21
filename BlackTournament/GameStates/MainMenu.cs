using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using BlackTournament.Entities;
using SFML.System;

namespace BlackTournament.GameStates
{
    class MainMenu:BaseGameState
    {
        private GameText _Text;
        private SFML.Audio.Music music;
        public MainMenu(Core core):base(core)
        {

        }

        public override bool Load()
        {
            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(300, 100);
            _Text.Text = "MAIN MENU";
            Layer_Game.AddChild(_Text);

            MusicManager.RootFolder = "music";
            music = MusicManager.Load("Ten_Seconds_to_Rush");
            music.Volume = 15;
            music.Play();

            return true;
        }

        public override void Update(float deltaT)
        {

        }

        public override void Destroy()
        {
            music.Dispose();
            // todo: find better destroy / music logic
            //_Core.AnimationManager.Run(music.Volume, 0, 1, v => music.Volume = v, BlackCoat.Animation.InterpolationType.Linear, a => music = null);
        }
    }
}
