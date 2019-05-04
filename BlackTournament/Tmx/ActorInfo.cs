using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using TiledSharp;

namespace BlackTournament.Tmx
{
    abstract class ActorInfo
    {
        public Vector2f Position { get; set; }
        public String Tag { get; }

        public ActorInfo() { }
        public ActorInfo(TmxObject obj)
        {
            Position = new Vector2f((float)obj.X, (float)obj.Y);
            Tag = TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Tag));
        }
    }

    class AssetActorInfo : ActorInfo
    {
        public String Asset { get; }
        public Vector2f Scale { get; set; }

        public AssetActorInfo() { }
        public AssetActorInfo(TmxObject obj):base(obj)
        {
            Asset = TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Asset));
            Scale = new Vector2f(float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "ScaleX", 1), System.Globalization.CultureInfo.CurrentUICulture),
                                 float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "ScaleY", 1), System.Globalization.CultureInfo.CurrentUICulture));
        }
    }

    class RotorInfo : AssetActorInfo
    {
        public Vector2f Origin { get; set; }
        public float Speed { get; set; }

        public RotorInfo() { }
        public RotorInfo(TmxObject obj) : base(obj)
        {
            Origin = new Vector2f(float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "OriginX", 0), System.Globalization.CultureInfo.CurrentUICulture),
                                  float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "OriginY", 0), System.Globalization.CultureInfo.CurrentUICulture));
            Speed = float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Speed), 0), System.Globalization.CultureInfo.CurrentUICulture);
        }
    }
}