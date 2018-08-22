using System;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.System;

namespace BlackTournament.Particles
{
    class SmokeInfo:TextureParticleAnimationInfo
    {
        private readonly Core _Core;
        private readonly float _Speed;

        public override Vector2f Offset { get => _Core.Random.NextVector(-10, 10); set => base.Offset = value; }
        public override Vector2f Scale { get => base.Scale + _Core.Random.NextVector(-0.3f, 0); set => base.Scale = value; }
        public override float Rotation { get => _Core.Random.NextFloat(0, 360); set => base.Rotation = value; }
        public override Vector2f Velocity { get => VectorExtensions.VectorFromAngleLookup(_Core.Random.NextFloat(0, 360), _Speed) + base.Velocity; set => base.Velocity = value; }
        public override float RotationVelocity { get => _Core.Random.NextFloat(-_Speed, _Speed)+base.RotationVelocity; set => base.RotationVelocity = value; }
        public override float AlphaFade { get => _Core.Random.NextFloat(-2, -1.1f); set => base.AlphaFade = value; }

        public SmokeInfo(Core core, float speed)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            _Speed = speed;
        }
    }
}