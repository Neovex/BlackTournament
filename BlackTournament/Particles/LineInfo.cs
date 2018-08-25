using SFML.System;
using BlackCoat.ParticleSystem;

namespace BlackTournament.Particles
{
    class LineInfo : PixelParticleInitializationInfo
    {
        public virtual Vector2f TargetOffset { get; set; }
    }
}