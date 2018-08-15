using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;
using BlackTournament.Particles;
using BlackCoat.Entities.Shapes;

namespace BlackTournament.GameStates
{
    class TestState : BaseGamestate
    {
        private ParticleEmitterHost _Host;
        private SparkEmitter _Emitter;

        private SfxManager _Sfx;
        private TriggerComposite _Composite;

        public TestState(Core core) : base(core, "TEST", Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            _Core.Input.MouseButtonPressed += Input_MouseButtonPressed;

            Layer_Game.AddChild(new Rectangle(_Core)
            {
                Size= new Vector2f(_Core.DeviceSize.X/2, _Core.DeviceSize.Y),
                Color=Color.White
            });

            _Composite = new TriggerComposite(_Core);
            _Composite.Add(new SparkEmitter(_Core)
            {
                DefaultTTL = 2,
                ParticlesPerSpawn = 0, // WARN!!!! ###############
                Color = Color.Green,
                Speed = 100,
                Gravity = new Vector2f(0,50)
            });
            var smokeTex = TextureLoader.Load(Files.Emitter_Smoke_Grey);
            _Composite.Add(new SmokeEmitter(_Core, smokeTex, BlendMode.Add)
            {
                ParticlesPerSpawn = 25,
                Color = new Color(255,100,20,255),
                Speed = 25,
                Alpha = 1f,
                Scale = new Vector2f(0.5f,0.5f),
                Origin = smokeTex.Size.ToVector2f() / 2

                //Loop = true,
                //SpawnRate = 0.05f
            });
            _Composite.Add(new WaveEmitter(_Core, TextureLoader.Load(Files.Emitter_Shockwave))
            {
                Alpha = 0.75f,
                StartScale = 0.05f,
                EndScale = 1.2f,
                Speed = 2.5f
            });

            _Host = new ParticleEmitterHost(_Core);
            _Host.AddEmitter(_Composite);
            Layer_Game.AddChild(_Host);

            // Snd
            _Sfx = new SfxManager(SfxLoader);
            _Sfx.LoadFromDirectory();
            foreach (var sfx in Files.GAME_SFX) _Sfx.AddToLibrary(sfx, 100, true);
            return true;
        }

        private void ChangeColor()
        {
            //_Emitter.Color = new Color((byte)(255*_Core.Random.NextFloat(0, 3)/3), (byte)(255 * _Core.Random.NextFloat(0, 3) / 3), (byte)(255 * _Core.Random.NextFloat(0, 3) / 3));
            //_Core.AnimationManager.Wait(1.5f, ChangeColor);
        }

        private void Input_MouseButtonPressed(SFML.Window.Mouse.Button btn)
        {
            switch (btn)
            {
                case SFML.Window.Mouse.Button.Left:
                    _Composite.Trigger();
                    _Sfx.Play(Files.Sfx_Explosion1);
                    break;

                case SFML.Window.Mouse.Button.Right:
                    break;
            }
        }

        protected override void Update(float deltaT)
        {
            //_Emitter.Rotation += deltaT * 1000;
            //_Emitter.Velocity = VectorExtensions.VectorFromAngle(_Emitter.Rotation, 100);

            //_Emitter.Velocity = (_OldPos - _Core.Input.MousePosition)*-10000*deltaT;
            //if (_Emitter.Velocity.X < 1 && _Emitter.Velocity.Y < 1) _Emitter.Velocity = new Vector2f(0, -5);
            //ChangeColor();

            _Composite.Position = _Core.Input.MousePosition;
        }

        protected override void Destroy()
        {
        }
    }
}
