using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;
using BlackTournament.Net.Data;
using TiledSharp;

namespace BlackTournament.Tmx
{
    class Killzone : ICollidable
    {
        public ICollisionShape CollisionShape { get; set; }
        public float Damage { get; set; }
        public EffectType Effect { get; set; }

        public Killzone(TmxObject obj, CollisionShape collisionShape)
        {
            CollisionShape = collisionShape;
            Damage = float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Damage), float.MaxValue));
            Effect = (EffectType)Enum.Parse(typeof(EffectType), TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Effect), EffectType.Gore), true);
        }
    }
}