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
using BlackCoat.Entities;
using BlackTournament.Systems;
using BlackCoat.Tools;

namespace BlackTournament.GameStates
{
    class TestState : BaseGamestate
    {
        private ParticleEmitterHost _Host;

        private SfxManager _Sfx;
        private CompositeEmitter _Composite;

        public TestState(Core core) : base(core, "TEST", Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
        }

        protected override bool Load()
        {
            _Core.Input.MouseButtonPressed += Input_MouseButtonPressed;


            Layer_Game.AddChild(new Rectangle(_Core)
            {
                Size= new Vector2f(_Core.DeviceSize.X/2, _Core.DeviceSize.Y),
                Color=Color.White,Visible =false
            });

            _Composite = new CompositeEmitter(_Core);
            _Composite.Add(new BasicPixelEmitter(_Core, 0, new SparkInfo(_Core, 400, 10, 180)
            {
                TTL = 0.2f,
                Color = new Color(255, 255, 120, 255),
                Acceleration = new Vector2f(0, 950)
                //AlphaFadeOut = -4
            }));

            var smokeTex = TextureLoader.Load(Files.Emitter_Smoke_Grey);
            _Composite.Add(new BasicTextureEmitter(_Core, smokeTex, 25, new SmokeInfo(_Core, 25)
            {
                TTL = 25,
                Color = new Color(255, 100, 20, 255),
                Alpha = 1f,
                Scale = new Vector2f(0.5f, 0.5f),
                Origin = smokeTex.Size.ToVector2f() / 2,
                AlphaFade = -1,
                UseAlphaAsTTL = true
            }, blendMode: BlendMode.Add));
            var shockTex = TextureLoader.Load(Files.Emitter_Shockwave);
            _Composite.Add(new BasicTextureEmitter(_Core, shockTex, 1, new TextureParticleAnimationInfo()
            {
                TTL = 25,
                Alpha = 0.75f,
                Origin = shockTex.Size.ToVector2f() / 2,
                Scale = new Vector2f(0.05f, 0.05f),
                ScaleVelocity = new Vector2f(4f, 4f),
                AlphaFade = -2.5f,
                UseAlphaAsTTL = true
            }));

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
                    _Sfx.Play(Files.Sfx_Explosion1);
                    _Composite.Trigger();
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
            _Core.Input.MouseButtonPressed -= Input_MouseButtonPressed;
        }
    }
}