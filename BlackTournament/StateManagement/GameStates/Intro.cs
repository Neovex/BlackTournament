using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.System;
using BlackCoat;
using BlackTournament.Entities;
using SFML.Graphics;

namespace BlackTournament.StateManagement.GameStates
{
    public class Intro : BaseGameState
    {
        private GameText _Text;

        public Intro(Core core, StateManager stateManager) : base(core, stateManager)
        {
        }

        public override bool Load()
        {
            _Core.Log("Loading intro");
            // todo : load intro

            _Text = new GameText(_Core);
            _Text.Position = new Vector2f(299, 399);
            _Text.Text = "HOLY SHIT! We got an Intro?";
            _Core.Layer_Game.AddChild(_Text);
            _Core.AnimationManager.Wait(5, t => _StateManager.ChangeState(State.MainMenu));

            return true;
        }

        public override void Update(float deltaT)
        {
            
        }

        public override void Destroy()
        {
            _Core.Log("Unloading intro");
            // todo : unload intro

            _Core.Layer_Game.RemoveChild(_Text);
        }
    }
}