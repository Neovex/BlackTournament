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

        protected override void StateLoadingFailed()
        {
            throw new Exception("FUBAR");
        }

        protected override void StateReady()
        {
            if (!String.IsNullOrWhiteSpace(_Message)) _State.DisplayPopupMessage(_Message);
            _State.Host += State_HostClicked;
            // TODO: restore UI State
        }

        private void State_HostClicked()
        {
            _Game.StartNewGame("thepit");
        }

        protected override void StateReleased()
        {
            _State.Host -= State_HostClicked;
            _Message = null;
            _State = null;
        }
    }
}