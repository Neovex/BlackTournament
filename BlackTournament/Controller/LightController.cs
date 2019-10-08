using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Scenes;

namespace BlackTournament.Controller
{
    internal class LightController : ControllerBase
    {
        public LightController(Game game) : base(game)
        {
        }

        internal void Activate(string map)
        {
            Activate(new LightEditor(_Game, map));
        }

        protected override void SceneLoadingFailed()
        {
            Log.Error("");
        }

        protected override void SceneReady()
        {
            Log.Debug("");
        }

        protected override void SceneReleased()
        {
        }
    }
}