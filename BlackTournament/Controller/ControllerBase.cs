using System;
using BlackCoat;

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
            _State = state ?? throw new ArgumentNullException(nameof(state));
            AttachEvents();
            _Game.Core.StateManager.ChangeState(_State);
        }

        private void StateLoadingFailedInternal()
        {
            DetachEvents();
            StateLoadingFailed();
            _State = null;
        }

        private void ReleaseStateInternal()
        {
            DetachEvents();
            StateReleased();
            _State = null;
        }


        protected abstract void StateReady();
        protected abstract void StateLoadingFailed();
        protected abstract void StateReleased();


        private void AttachEvents()
        {
            _State.Loaded += StateReady;
            _State.LoadingFailed += StateLoadingFailedInternal;
            _State.OnDestroy += ReleaseStateInternal;
        }

        private void DetachEvents()
        {
            _State.Loaded -= StateReady;
            _State.LoadingFailed -= StateLoadingFailedInternal;
            _State.OnDestroy -= ReleaseStateInternal;
        }
    }
}