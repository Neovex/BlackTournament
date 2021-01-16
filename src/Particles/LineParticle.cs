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
    class LineParticle : PixelParticle, IInitializableByInfo<LineInfo>, IInitializableByInfo<LightningInfo>
    {
        private Vector2f _TargetOffset;


        public LineParticle(Core core) : base(core)
        {
        }


        public void Initialize(Vector2f position, LineInfo info)
        {
            _TargetOffset = info.TargetOffset;
            base.Initialize(position, info);
        }

        public void Initialize(Vector2f position, LightningInfo info)
        {
            Initialize(position, info as LineInfo);
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
            vPtr->Position = _Position + _TargetOffset;
            vPtr->Color = _Color;
            vPtr--;
            return base.UpdateInternal(deltaT, vPtr);
        }
    }
}