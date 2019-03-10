using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using TiledSharp;

namespace BlackTournament.Tmx
{
    class RotorInfo : ActorInfo, IAssetLayer
    {
        public string Asset { get; set; }
        public Vector2f Origin { get; set; }
        public float Speed { get; set; }

        internal static RotorInfo Parse(TmxObject obj)
        {
            return new RotorInfo()
            {
                Position = new Vector2f((float)obj.X, (float)obj.Y),
                Asset = TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Asset)),
                Origin = new Vector2f(float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "OriginX", 0)),
                                      float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "OriginY", 0))),
                Speed = float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "Speed", 0))
            };
        }
    }
}