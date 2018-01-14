using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.System;
using BlackCoat;
using BlackTournament.Entities;
using SFML.Graphics;

namespace BlackTournament.GameStates
{
    public class TournamentIntro : BaseGamestate
    {
        private GameText _Text;

        public TournamentIntro(Core core) : base(core)
        {
        }

        protected override bool Load()
        {
            // todo : load intro

            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(225, 399);
            _Text.Text = "HOLY SHIT! We got an Intro?";
            Layer_Game.AddChild(_Text);
            _Core.AnimationManager.Wait(2, t => _Core.StateManager.ChangeState(new MainMenu(_Core)));

            return true;
        }

        protected override void Update(float deltaT)
        {
            
        }

        protected override void Destroy()
        {
            // todo : unload intro

            Layer_Game.RemoveChild(_Text);
        }
    }
}