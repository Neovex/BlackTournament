using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.GameStates;

namespace BlackTournament.Controller
{
    internal class LightController : ControllerBase
    {
        public LightController(Game game) : base(game)
        {
        }

        internal void Activate(string map)
        {
            Activate(new LightEditorState(_Game, map));
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