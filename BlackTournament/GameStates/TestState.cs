using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;

namespace BlackTournament.GameStates
{
    class TestState : BaseGamestate
    {
        private ParticleEmitterHost _Host;
        private BasicPixelEmitter _Emitter;

        private SfxManager _Sfx;
        private int _SfxIndex;

        public TestState(Core core) : base(core, "TEST", Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            _Host = new ParticleEmitterHost(_Core);
            //Layer_Game.AddChild(_Host);

            _Host.AddEmitter(_Emitter = new BasicPixelEmitter(_Core)
            {
                DefaultTTL = 4,
                ParticlesPerSpawn = 1,
                Position = _Core.DeviceSize.ToVector2f() / 2,
                SpawnRate = 0.0001f,
                Loop = true,
                Velocity = new Vector2f(100, 150),
                Acceleration = new Vector2f(0, 100),
                Color = Color.White
            });
            _Core.Input.MouseButtonPressed += Input_MouseButtonPressed;
            ChangeColor();


            // Snd
            _Sfx = new SfxManager(SfxLoader);
            _Sfx.LoadFromDirectory();
            foreach (var sfx in Files.GAME_SFX) _Sfx.AddToLibrary(sfx, 100, true);
            return true;
        }

        private void ChangeColor()
        {
            _Emitter.Color = new Color((byte)(255*_Core.Random.NextFloat(0, 3)/3), (byte)(255 * _Core.Random.NextFloat(0, 3) / 3), (byte)(255 * _Core.Random.NextFloat(0, 3) / 3));
            //_Core.AnimationManager.Wait(1.5f, ChangeColor);
        }

        private void Input_MouseButtonPressed(SFML.Window.Mouse.Button btn)
        {
            switch (btn)
            {
                case SFML.Window.Mouse.Button.Left:
                    //_Emitter.Position = _Core.Input.MousePosition;
                    //SFML.Audio.Listener.Direction = new Vector3f(0, 1, 0);
                    SFML.Audio.Listener.Position = _Core.DeviceSize.ToVector2f().ToVector3f()/2;
                    //SFML.Audio.Listener.Position += new Vector3f(0, 10, 0);
                    var position = _Core.Input.MousePosition;
                    _Sfx.Play(Files.GAME_SFX.ElementAt(_SfxIndex % Files.GAME_SFX.Count), position);
                    Log.Debug(SFML.Audio.Listener.Position, position);
                    break;

                case SFML.Window.Mouse.Button.Right:
                    //_Emitter.Trigger();
                    _SfxIndex++;
                    SFML.Audio.Listener.Position = _Core.Input.MousePosition.ToVector3f();
                    Log.Debug(SFML.Audio.Listener.Position);
                    break;
            }
        }

        protected override void Update(float deltaT)
        {
            //_Emitter.Rotation += deltaT * 1000;
            //_Emitter.Velocity = VectorExtensions.VectorFromAngle(_Emitter.Rotation, 100);

            _Emitter.Velocity = VectorExtensions.VectorFromAngle(_Core.Random.NextFloat(0,360), 100);
            ChangeColor();

            if (_Core.Input.IsLMButtonDown()) _Emitter.Position = _Core.Input.MousePosition;
        }

        protected override void Destroy()
        {
        }
    }
}
