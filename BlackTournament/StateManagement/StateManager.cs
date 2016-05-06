using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using BlackTournament.StateManagement.GameStates;

namespace BlackTournament.StateManagement
{
    public class StateManager
    {
        private Core _Core;
        private BaseGameState _CurrentState;

        private State _RequestedState;
        private Object _StateData;
        
        public StateManager(Core core)
        {
            _Core = core;
            _Core.OnUpdate += Update;
        }

        public void ChangeState(State state, object stateData = null) // TODO
        {
            _RequestedState = state;
            _StateData = stateData;
        }

        public void Update(float deltaT)
        {
            if (_RequestedState == State.None) return;
            
            // Unload old State
            if (_CurrentState != null)
            {
                _CurrentState.Destroy();
                _CurrentState = null;
            }
            
            // Create new State
            switch (_RequestedState)
            {
                case State.None:
                    break;
                case State.Intro:
                    _CurrentState = new Intro(_Core, this);
                    break;
                case State.MainMenu:
                    _Core.Log("here schould be menue");
                    // todo
                    break;
                case State.Map:
                    _CurrentState = new MapState(_Core, this, _StateData.ToString());
                    break;
            }

            if (_CurrentState != null)
            {
                // Load state
                if (!_CurrentState.Load())
                {
                    _Core.Log("Failed to load state", _RequestedState);
                }
            }

            _RequestedState = State.None;
        }
    }
}