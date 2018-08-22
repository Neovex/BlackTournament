using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.System;

namespace BlackTournament.Particles
{
    class LineInfo : ParticleAnimationInfo
    {
        private Core _Core;

        //public virtual Vector2f Velocity2 { get; set; }
        //public virtual Vector2f Acceleration2 { get; set; }

        public LineInfo(Core core)
        {
            _Core = core;
        }

        public void MakeFlash(Vector2f start, int index)
        {
            index++;
            var dir = Target.ToLocal(start);
            Position2 = dir;
            Position2 = (dir / ParticlesPerSpawn) * index;
            Jitter = (float)dir.Length() / 12;
            if(index!=ParticlesPerSpawn)
                Position2 += _Core.Random.NextVector(-Jitter, Jitter);
            Offset = Last;
            Last = Position2;
            Position2 -= Offset;
        }

        public Vector2f Position2 { get; set; }
        public Vector2f Target { get; set; }
        public Vector2f Last { get; set; }
        public float Jitter { get; set; }
    }
}