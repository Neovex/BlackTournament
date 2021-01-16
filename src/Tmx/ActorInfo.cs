using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackTournament.Net.Data;
using SFML.System;
using TiledSharp;

namespace BlackTournament.Tmx
{
    public enum Parent
    {
        Layer_BG,
        Layer_Game,
        Layer_Overlay,
        Light
    }

    abstract class ActorInfo
    {
        protected readonly CultureInfo _Culture = CultureInfo.CurrentUICulture;

        public String Name { get; }
        public Vector2f Position { get; set; }
        public float Rotation { get; set; }
        public Parent Parent { get; set; }
        public String Tag { get; }

        public ActorInfo() { }
        public ActorInfo(TmxObject obj)
        {
            Name = obj.Name;
            Position = new Vector2f((float)obj.X, (float)obj.Y);
            Rotation = (float)obj.Rotation;
            Parent = (Parent)Enum.Parse(typeof(Parent), TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Parent), Parent.Layer_BG.ToString()), true);
            Tag = TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Tag));
        }
    }

    class PickupInfo : ActorInfo
    {
        public PickupType Item { get; set; }
        public int Amount { get; set; }
        public float RespawnTime { get; set; }

        public PickupInfo(TmxObject obj) : base(obj)
        {
            var pickupString = TmxMapper.ReadTmxObjectProperty(obj.Properties, "Item");
            if (!Enum.TryParse<PickupType>(pickupString, true, out PickupType type))
            {
                Log.Warning("Invalid Pickup entry skipped", pickupString);
                type = PickupType.None;
            }
            Item = type;
            Amount = Int32.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Amount), 0), _Culture);
            RespawnTime = Single.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(RespawnTime), 10), _Culture);
            Position += new Vector2f((float)obj.Width, (float)obj.Height) / 2;
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
            Scale = new Vector2f(float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "ScaleX", 1), _Culture),
                                 float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "ScaleY", 1), _Culture));
        }
    }

    class RotorInfo : AssetActorInfo
    {
        public Vector2f Origin { get; set; }
        public float Speed { get; set; }

        public RotorInfo() { }
        public RotorInfo(TmxObject obj) : base(obj)
        {
            Origin = new Vector2f(float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "OriginX", 0), _Culture),
                                  float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, "OriginY", 0), _Culture));
            Speed = float.Parse(TmxMapper.ReadTmxObjectProperty(obj.Properties, nameof(Speed), 0), _Culture);
        }
    }
}