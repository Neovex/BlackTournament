using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using BlackCoat;
using BlackCoat.ParticleSystem;

namespace BlackTournament.Particles
{
    public class SparkEmitter : PixelEmitter
    {
        private static readonly Guid _GUID = typeof(BasicPixelParticle).GUID;
        public override Guid ParticleTypeGuid => _GUID;

        public Int32 ParticlesPerSpawn { get; set; }
        public float Speed { get; set; }
        public Vector2f Gravity { get; set; }


        public SparkEmitter(Core core, int depth = 0) : base(core, depth)
        {
        }


        public void Trigger()
        {
            for (int i = 0; i < ParticlesPerSpawn; i++)
            {
                var particle = RetrieveFromCache() as BasicPixelParticle ?? new BasicPixelParticle(_Core);
                AddParticle(particle);

                var vector = VectorExtensions.VectorFromLookup(_Core.Random.NextFloat(0, 360));
                var velo = vector.Normalize() * Speed;

                particle.Initialize(Position, Color, velo, Gravity);
            }
        }

        protected override void Update(float deltaT)
        {
        }
    }
}
