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
    class TestState : Gamestate
    {
        private LightningEmitter _LineEmitter;
        private SfxManager _Sfx;
        private ParticleEmitterHost _Host;

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

            Layer_Game.AddChild(_Host = new ParticleEmitterHost(_Core));
            _Host.AddEmitter(_LineEmitter);
            _LineEmitter.Position = _Core.DeviceSize.ToVector2f() / 2;

            // Snd
            _Sfx = new SfxManager(SfxLoader);
            _Sfx.LoadFromDirectory();
            foreach (var sfx in Files.GAME_SFX) _Sfx.AddToLibrary(sfx, 100, true);
            return true;
        }

        private void Input_MouseButtonPressed(SFML.Window.Mouse.Button btn)
        {
            switch (btn)
            {
                case SFML.Window.Mouse.Button.Left:
                    _Sfx.Play(Files.Sfx_Explosion);
                    _LineEmitter.Trigger();
                    break;

                case SFML.Window.Mouse.Button.Right:
                    break;
            }
        }

        protected override void Update(float deltaT)
        {
            _LineEmitter.ParticleInfo.LigtningTarget = _Core.Input.MousePosition;

            //_Emitter.Rotation += deltaT * 1000;
            //_Emitter.Velocity = VectorExtensions.VectorFromAngle(_Emitter.Rotation, 100);

            //_Emitter.Velocity = (_OldPos - _Core.Input.MousePosition)*-10000*deltaT;
            //if (_Emitter.Velocity.X < 1 && _Emitter.Velocity.Y < 1) _Emitter.Velocity = new Vector2f(0, -5);
            //ChangeColor();
        }

        protected override void Destroy()
        {
            _Core.Input.MouseButtonPressed -= Input_MouseButtonPressed;
        }
    }
}