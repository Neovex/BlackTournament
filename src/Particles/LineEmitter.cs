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
    class LineEmitter : Emitter<LineParticle, LineInfo>
    {
        public LineEmitter(Core core, LineInfo info, int depth = 0) : base(core, info, PrimitiveType.Lines, BlendMode.Alpha, null, depth)
        {
        }
    }
}