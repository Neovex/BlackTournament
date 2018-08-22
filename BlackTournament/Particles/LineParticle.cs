using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.Graphics;
using SFML.System;

namespace BlackTournament.Particles
{
    class LineParticle : BasicPixelParticle
    {
        private Vector2f _Position2;


        public LineParticle(Core core) : base(core)
        {
        }


        public void Initialize(Vector2f position, LineInfo info)
        {
            _Position2 = info.Position2;
            base.Initialize(position, info);
        }

        protected override unsafe void Clear(Vertex* vPtr)
        {
            base.Clear(vPtr);
            vPtr++;
            base.Clear(vPtr);
        }
        protected override unsafe bool UpdateInternal(float deltaT, Vertex* vPtr)
        {
            vPtr++;
            vPtr->Position = _Position + _Position2;
            vPtr->Color = _Color;
            vPtr--;
            return base.UpdateInternal(deltaT, vPtr);
        }
    }
}