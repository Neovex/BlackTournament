using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackTournament.GameStates;

namespace BlackTournament.Controller
{
    public abstract class ControllerBase
    {
        protected Game _Game;
        private BaseGamestate _State;

        public ControllerBase(Game game)
        {
            _Game = game;
        }

        protected virtual void Activate(BaseGamestate state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _State = state;
            _State.Ready += StateReady;
            _State.OnDestroy += ReleaseStateInternal;
        }

        private void ReleaseStateInternal()
        {
            _State.Ready -= StateReady;
            _State.OnDestroy -= ReleaseStateInternal;
            StateReleased();
            _State = null;
        }

        protected abstract void StateReady();
        protected abstract void StateReleased();
    }
}