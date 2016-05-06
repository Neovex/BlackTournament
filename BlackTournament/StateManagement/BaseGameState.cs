using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat;
using BlackCoat.Entities;

namespace BlackTournament.StateManagement
{
    public abstract class BaseGameState
    {
        protected Core _Core;
        protected StateManager _StateManager;

        public BaseGameState(Core core, StateManager stateManager)
        {
            _Core = core;
            _StateManager = stateManager;
        }

        public abstract Boolean Load();
        public abstract void Update(float deltaT);
        public abstract void Destroy();
    }
}