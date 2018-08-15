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
    class AlphaParticle : BasicTextureParticle
    {
        public AlphaParticle(Core core) : base(core)
        {
        }

        protected override unsafe bool UpdateInternal(float deltaT, Vertex* vPtr)
        {
            return base.UpdateInternal(deltaT, vPtr) || _Alpha <= 0;
        }
    }
}
