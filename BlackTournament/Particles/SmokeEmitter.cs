using System;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;

namespace BlackTournament.Particles
{
    public class SmokeEmitter : TextureEmitter
    {
        private static readonly Guid _GUID = typeof(AlphaParticle).GUID;

        private Single _SpawnTimer;

        // Emitter / Particle Infos
        public override Guid ParticleTypeGuid => _GUID;
        public bool Triggered { get; private set; }
        public bool Loop { get; set; }

        public Vector2f? Velocity { get; set; }
        public float Speed { get; set; }
        public Vector2f Acceleration { get; set; }
        // Spawn Infos
        public Single SpawnRate { get; set; }
        public Int32 ParticlesPerSpawn { get; set; }

        public SmokeEmitter(Core core, Texture texture, BlendMode blendMode, int depth = 0) : base(core, texture, blendMode, depth)
        {
            DefaultTTL = 25;
        }

        public void Trigger()
        {
            Triggered = true;
        }

        protected override void Update(float deltaT)
        {
            if (Triggered)
            {
                Triggered = Loop;

                _SpawnTimer -= deltaT;
                if (_SpawnTimer < 0)
                {
                    _SpawnTimer = SpawnRate;

                    for (int i = 0; i < ParticlesPerSpawn; i++)
                    {
                        var roatationVelocity = _Core.Random.NextFloat(-25, 25);
                        var alphaFadeOut = _Core.Random.NextFloat(-2, -1.1f);
                        var velocity = Velocity ?? VectorExtensions.VectorFromLookup(_Core.Random.NextFloat(0, 360), Speed);

                        var particle = RetrieveFromCache() as AlphaParticle ?? new AlphaParticle(_Core);
                        particle.Initialize(Texture,
                                            Position + _Core.Random.NextVector(-5, 5),
                                            Origin,
                                            Scale + _Core.Random.NextVector(-0.3f, 0),
                                            _Core.Random.NextFloat(0, 360),
                                            Color,
                                            Alpha,
                                            velocity,
                                            Acceleration,
                                            roatationVelocity,
                                            alphaFadeOut
                                            );
                        AddParticle(particle);
                    }
                }
            }
        }
    }
}