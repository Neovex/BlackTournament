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
    class SmokeTrailEmitter : Emitter<TextureParticle, TexturedSpawnInfo>
    {
        public SmokeTrailEmitter(Core core, TexturedSpawnInfo info, int depth = 0) 
            : base(core, info,  PrimitiveType.Quads, BlendMode.Alpha, info.Texture, depth)
        {
        }

        public override void Trigger()
        {
            var amount = ParticleInfo.ParticlesPerSpawn;
            for (int i = 0; i < amount; i++)
            {
                var particle = RetrieveFromCache() as TextureParticle ?? CreateParticle();
                particle.Initialize(Position, ParticleInfo);
                AddParticle(particle, ParticleInfo.TTL);
            }
        }
    }
}