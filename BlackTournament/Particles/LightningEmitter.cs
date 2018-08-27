using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.Graphics;

namespace BlackTournament.Particles
{
    class LightningEmitter : Emitter<LineParticle, LightningInfo>
    {
        public LightningEmitter(Core core, LightningInfo info, int depth = 0) : base(core, info, PrimitiveType.Lines, BlendMode.Alpha, null, depth)
        {
        }

        public override void Trigger()
        {
            var amount = ParticleInfo.ParticlesPerSpawn;
            for (int i = 0; i < amount; i++)
            {
                var particle = RetrieveFromCache() as LineParticle ?? new LineParticle(_Core);
                ParticleInfo.UpdateCluster(Position, i);
                particle.Initialize(Position, ParticleInfo);
                AddParticle(particle, ParticleInfo.TTL);
            }
            ParticleInfo.Reset();
        }
    }
}