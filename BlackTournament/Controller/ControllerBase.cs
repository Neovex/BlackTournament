using System;
using BlackCoat;

namespace BlackTournament.Controller
{
    public abstract class ControllerBase
    {
        protected Game _Game;
        private Scene _Scene;

        public ControllerBase(Game game)
        {
            _Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        protected virtual void Activate(Scene scene)
        {
            _Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            AttachEvents();
            _Game.Core.SceneManager.ChangeScene(_Scene);
        }

        private void SceneLoadingFailedInternal()
        {
            DetachEvents();
            SceneLoadingFailed();
            _Scene = null;
        }

        private void ReleaseSceneInternal()
        {
            DetachEvents();
            SceneReleased();
            _Scene = null;
        }


        protected abstract void SceneReady();
        protected abstract void SceneLoadingFailed();
        protected abstract void SceneReleased();


        private void AttachEvents()
        {
            _Scene.Loaded += SceneReady;
            _Scene.LoadingFailed += SceneLoadingFailedInternal;
            _Scene.OnDestroy += ReleaseSceneInternal;
        }

        private void DetachEvents()
        {
            _Scene.Loaded -= SceneReady;
            _Scene.LoadingFailed -= SceneLoadingFailedInternal;
            _Scene.OnDestroy -= ReleaseSceneInternal;
        }
    }
}