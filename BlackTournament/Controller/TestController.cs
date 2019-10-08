using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackTournament.Scenes;

namespace BlackTournament.Controller
{
    public class TestController : ControllerBase
    {
        public TestController(Game game) : base(game)
        {
        }

        internal void Activate()
        {
            Activate(new TestScene(_Game));
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
