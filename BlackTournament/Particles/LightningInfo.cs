using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.ParticleSystem;
using BlackTournament.Net.Data;

namespace BlackTournament.Particles
{
    class LightningInfo : LineInfo
    {
        private Core _Core;
        private Vector2f _Last;


        public Vector2f LigtningTarget { get; set; }


        public LightningInfo(Core core)
        {
            _Core = core;
        }

        internal void UpdateCluster(Vector2f start, int index)
        {
            index++;
            Offset = _Last;

            var dir = LigtningTarget.ToLocal(start);
            TargetOffset = (dir / ParticlesPerSpawn) * index;
            if (index != ParticlesPerSpawn)
            {
                var jitter = (float)dir.Length() / 12; // consider adding ParticlesPerSpawn into the auto jitter calc
                TargetOffset += VectorExtensions.VectorFromAngleLookup(_Core.Random.NextFloat(0, 360), _Core.Random.NextFloat(0, jitter));
            }
            _Last = TargetOffset;
            TargetOffset = TargetOffset.ToLocal(Offset);
        }

        internal void Reset()
        {
            _Last = default(Vector2f);
        }

        public void Update(PickupType type, Vector2f target)
        {
            LigtningTarget = TargetOffset = target;

            switch (type)
            {
                case PickupType.SmallHealth:
                    break;
                case PickupType.BigHealth:
                    break;
                case PickupType.SmallShield:
                    break;
                case PickupType.BigShield:
                    break;
                case PickupType.Drake:
                    Color = Color.White;
                    Alpha = 0.5F;
                    AlphaFade = -2;
                    UseAlphaAsTTL = true;
                    break;
                case PickupType.Hedgeshock:
                    Color = Color.Cyan;
                    Alpha = 0.5F;
                    AlphaFade = -2;
                    UseAlphaAsTTL = true;
                    break;
                case PickupType.Thumper:
                    break;
                case PickupType.Titandrill:
                    break;
                default:
                    break;
            }
        }
    }
}