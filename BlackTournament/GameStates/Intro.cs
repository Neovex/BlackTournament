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
    public class Intro : BaseGameState
    {
        private GameText _Text;

        public Intro(Core core) : base(core)
        {
        }

        public override bool Load()
        {
            Log.Debug("Loading intro");
            // todo : load intro

            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(299, 399);
            _Text.Text = "HOLY SHIT! We got an Intro?";
            Layer_Game.AddChild(_Text);
            _Core.AnimationManager.Wait(5, t => _Core.StateManager.ChangeState(new MainMenue(_Core)));

            return true;
        }

        public override void Update(float deltaT)
        {
            
        }

        public override void Destroy()
        {
            Log.Debug("Unloading intro");
            // todo : unload intro

            Layer_Game.RemoveChild(_Text);
        }
    }
}