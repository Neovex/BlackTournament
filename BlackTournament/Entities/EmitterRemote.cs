using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.ParticleSystem;
using SFML.Graphics;
using SFML.System;

namespace BlackTournament.Entities
{
    class EmitterRemote<Tparticle, TInfo> : IEntity where Tparticle : ParticleBase, IInitializableByInfo<TInfo> where TInfo : PixelParticleInitializationInfo
    {

        public event Action<TInfo> Triggered = i => { };


        private readonly Emitter<Tparticle, TInfo> _Emitter;
        private readonly float _TriggerFrequency;
        private float _TriggerTimer;


        public Container Parent { get; set; }
        public bool Visible { get => true; set { } }
        public View View { get => null; set { } }
        public RenderStates RenderState { get => default(RenderStates); set { } }
        public Shader Shader { get => null; set { } }
        public BlendMode BlendMode { get => BlendMode.Alpha; set { } }
        public Color Color { get => Color.White; set { } }
        public float Alpha { get => 1; set { } }
        public RenderTarget RenderTarget => null;
        public Vector2f Origin { get => default(Vector2f); set { } }
        public Vector2f Position { get => _Emitter.Position; set => _Emitter.Position = value; }
        public float Rotation { get => 0; set { } }
        public Vector2f Scale { get => default(Vector2f); set { } }
        public Transform Transform { get; set; }
        public Transform InverseTransform { get; set; }


        public EmitterRemote(Emitter<Tparticle, TInfo> emitter, float triggerFrequency)
        {
            _Emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
            _TriggerFrequency = triggerFrequency;
        }

        public void Draw()
        {
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
        }

        public void Update(float deltaT)
        {
            _TriggerTimer -= deltaT;
            if (_TriggerTimer < 0)
            {
                _TriggerTimer = _TriggerFrequency;
                Triggered(_Emitter.ParticleInfo);
                _Emitter.Trigger();
            }
        }
    }
}