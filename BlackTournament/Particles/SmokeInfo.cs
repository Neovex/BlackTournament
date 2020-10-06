using System;
using BlackCoat;
using BlackCoat.ParticleSystem;
using SFML.Graphics;
using SFML.System;

namespace BlackTournament.Particles
{
    class SmokeInfo:TexturedSpawnInfo
    {
        private readonly Core _Core;
        private readonly float _Speed;

        public override Vector2f Offset { get => _Core.Random.NextVector(-10, 10); set => base.Offset = value; }
        public override Vector2f Scale { get => base.Scale * _Core.Random.NextFloat(0.8f, 1); set => base.Scale = value; }
        public override float Rotation { get => _Core.Random.NextFloat(0, 360); set => base.Rotation = value; }
        public override Vector2f Velocity { get => base.Velocity + Create.Vector2fFromAngleLookup(_Core.Random.NextFloat(0, 360), _Speed); set => base.Velocity = value; }
        public override float RotationVelocity { get => base.RotationVelocity + _Core.Random.NextFloat(-_Speed, _Speed); set => base.RotationVelocity = value; }
        public override float AlphaFade { get => base.AlphaFade + _Core.Random.NextFloat(-1.5f, -0.5f); set => base.AlphaFade = value; }

        public SmokeInfo(Core core, Texture texture, float speed) : base(texture)
        {
            _Core = core ?? throw new ArgumentNullException(nameof(core));
            _Speed = speed;
        }
    }
}