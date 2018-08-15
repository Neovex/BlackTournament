using System;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;

namespace BlackTournament.Particles
{
    public class WaveEmitter : TextureEmitter
    {
        private static readonly Guid _GUID = typeof(WaveParticle).GUID;

        // Emitter / Particle Infos
        public override Guid ParticleTypeGuid => _GUID;

        public float StartScale { get; set; }
        public float EndScale { get; set; }
        public float Speed { get; set; }

        public WaveEmitter(Core core, Texture texture, int depth = 0) : base(core, texture, BlendMode.Alpha, depth)
        {
            DefaultTTL = 25;
        }

        public void Trigger()
        {
            var particle = RetrieveFromCache() as WaveParticle ?? new WaveParticle(_Core);
            particle.Initialize(Texture,
                                Position,
                                Texture.Size.ToVector2f() / 2,
                                StartScale,
                                EndScale,
                                Color,
                                Alpha,
                                Speed);
            AddParticle(particle);
        }

        protected override void Update(float deltaT)
        {
        }
    }
}