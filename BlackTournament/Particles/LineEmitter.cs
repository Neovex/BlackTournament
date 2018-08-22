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
    class LineEmitter : BaseEmitter, ITriggerEmitter
    {
        private static readonly Guid _GUID = typeof(LineParticle).GUID;
        public override Guid ParticleTypeGuid => _GUID;
        private Single _SpawnTimer;


        /// <summary>
        /// Gets a value indicating whether this instance is currently triggered.
        /// </summary>
        public Boolean IsTriggered { get; private set; }
        /// <summary>
        /// Particle animation information for particle initialization.
        /// </summary>
        public LineInfo ParticleInfo { get; private set; }


        public LineEmitter(Core core, LineInfo info) : base(core, PrimitiveType.Lines)
        {
            ParticleInfo = info;
        }

        public LineEmitter(Core core, LineInfo info, int depth, BlendMode blendMode, Texture texture = null) : base(core, depth, PrimitiveType.Lines, blendMode, texture)
        {
            ParticleInfo = info;
        }


        public void Trigger()
        {
            IsTriggered = true;
        }

        protected override void Update(float deltaT)
        {
            if (IsTriggered)
            {
                _SpawnTimer -= deltaT;
                if (_SpawnTimer < 0)
                {
                    _SpawnTimer = ParticleInfo.SpawnRate;
                    IsTriggered = ParticleInfo.Loop;

                    var amount = ParticleInfo.ParticlesPerSpawn;
                    for (int i = 0; i < amount; i++)
                    {
                        var particle = RetrieveFromCache() as LineParticle ?? new LineParticle(_Core);
                        ParticleInfo.MakeFlash(Position, i);
                        particle.Initialize(Position, ParticleInfo);
                        AddParticle(particle, ParticleInfo.TTL);
                    }
                    ParticleInfo.Last = ParticleInfo.Offset = new SFML.System.Vector2f();
                }
            }
        }
    }
}