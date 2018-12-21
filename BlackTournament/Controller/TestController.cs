using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackTournament.GameStates;

namespace BlackTournament.Controller
{
    public class TestController : ControllerBase
    {
        public TestController(Game game) : base(game)
        {
        }

        internal void Activate()
        {
            Activate(new TestState(_Game));
        }

        protected override void StateLoadingFailed()
        {
            Log.Error("");
        }

        protected override void StateReady()
        {
            Log.Debug("");
        }

        protected override void StateReleased()
        {
        }
    }
}
