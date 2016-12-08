using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;

namespace BlackTournament.Controller
{
    public class MenuController
    {
        private Game _Game;
        private String _Message;
        private MainMenu _State;

        public MenuController(Game game)
        {
            _Game = game;
        }

        public void Activate(String message = null)
        {
            _Message = message;
            _State = new MainMenu(_Game.Core);
            _State.Ready += StateReady;
            _State.OnDestroy += ReleaseState;
            _Game.Core.StateManager.ChangeState(_State);
        }

        private void StateReady()
        {
            if (!String.IsNullOrWhiteSpace(_Message)) _State.DisplayPopupMessage(_Message);
            // TODO: restore UI State
        }

        private void ReleaseState()
        {
            // deregister from state events
            _State.Ready -= StateReady;
            _State.OnDestroy -= ReleaseState;
            _State = null;
        }
    }
}