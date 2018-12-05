using System;
using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.ParticleSystem;
using SFML.Graphics;
using SFML.System;

namespace BlackTournament.Entities
{
    class Remote<TEmitter> : IEntity where TEmitter : EmitterBase, ITriggerEmitter
    {
        public event Action<TEmitter> AboutToBeTriggered = i => { };


        protected TEmitter _Emitter;
        protected float _TriggerFrequency;
        protected float _TriggerTimer;


        public string Name { get; set; }
        public virtual Container Parent { get; set; }
        public virtual bool Visible { get => false; set { } }
        public virtual View View { get => null; set { } }
        public virtual RenderStates RenderState { get => default(RenderStates); set { } }
        public virtual Shader Shader { get => null; set { } }
        public virtual BlendMode BlendMode { get => BlendMode.Alpha; set { } }
        public virtual Color Color { get => Color.White; set { } }
        public virtual float Alpha { get => 1; set { } }
        public virtual RenderTarget RenderTarget { get => null; set { } }
        public virtual Vector2f Origin { get => default(Vector2f); set { } }
        public virtual Vector2f Position { get => _Emitter.Position; set => _Emitter.Position = value; }
        public virtual float Rotation { get => 0; set { } }
        public virtual Vector2f Scale { get; set; }
        public virtual Transform Transform { get; set; }
        public virtual Transform InverseTransform { get; set; }

        public IEntity Source { get; set; }
        public int AdditionalTriggers { get; set; }

        /// <summary>
        /// Gets the position of this <see cref="IEntity"/> independent from scene graph and view.
        /// </summary>
        public Vector2f GlobalPosition => Parent == null ? Position : (Position - Origin).ToGlobal(Parent.GlobalPosition);

        public bool Destroyed { get; private set; }

        public Remote(TEmitter emitter, float triggerFrequency, IEntity source = null)
        {
            _Emitter = emitter ?? throw new ArgumentNullException(nameof(emitter));
            _TriggerFrequency = triggerFrequency;
            Source = source;
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
                _Emitter.Position = Source?.Position ?? Parent?.Position ?? Position;
                for (int i = -1; i < AdditionalTriggers; i++)
                {
                    AboutToBeTriggered(_Emitter);
                    _Emitter.Trigger();
                }
            }
        }

        public void Dispose()
        {
            Destroyed = true;
            _Emitter = null;
            Source = null;
        }
    }
}