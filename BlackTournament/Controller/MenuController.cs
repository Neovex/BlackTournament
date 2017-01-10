using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;

namespace BlackTournament.Controller
{
    public class MenuController : ControllerBase
    {
        private String _Message;
        private MainMenu _State;

        public MenuController(Game game) : base(game)
        {
        }

        public void Activate(String message = null)
        {
            _Message = message;
            Activate(_State = new MainMenu(_Game.Core));
        }

        protected override void StateReady()
        {
            if (!String.IsNullOrWhiteSpace(_Message)) _State.DisplayPopupMessage(_Message);
            // TODO: restore UI State
        }

        protected override void StateReleased()
        {
            _Message = null;
            _State = null;
        }
    }
}