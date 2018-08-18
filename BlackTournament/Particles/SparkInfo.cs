using System;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.System;

namespace BlackTournament.Particles
{
    class SparkInfo : ParticleAnimationInfo
    {
        private readonly Core _Core;
        private readonly float _Speed;
        private readonly float? _Direction;
        private readonly float _Spread;

        public override Vector2f Velocity
        {
            set => base.Velocity = value;
            get
            {
                var dmin = (_Direction ?? _Spread) - _Spread;
                var dmax = (_Direction ?? 360 - _Spread) + _Spread;
                return VectorExtensions.VectorFromLookup(_Core.Random.NextFloat(dmin, dmax), _Speed + _Core.Random.NextFloat(-80, 60));
            }
        }
        //public override bool UseAlphaAsTTL { get => true; set => base.UseAlphaAsTTL = value; }
        public override float TTL { get => base.TTL + _Core.Random.NextFloat(-0.05f, 0.05f); set => base.TTL = value; }

        public SparkInfo(Core core, float speed, float spread, float? direction = null)
        {
            _Core = core;
            _Speed = speed;
            _Direction = direction;
            _Spread = spread;
        }
    }
}