using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SFML.System;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.Collision;
using BlackCoat.Collision.Shapes;

using TiledSharp;
using BlackTournament.Net.Data;

namespace BlackTournament.Tmx
{
    class TmxMapper
    {
        private const String _MAP_ROOT = "Maps";

        private TmxMap _MapData;
        private Dictionary<String, int> _TextureColumnLookup;
        private List<Layer> _Layers;
        private List<Pickup> _Pickups;
        private List<Vector2f> _SpawnPoints;
        private List<CollisionShape> _WallCollider;
        private List<CollisionShape> _Killzones;

        public String Name { get; private set; }
        public Color ClearColor { get; private set; }
        public Vector2i Size { get; internal set; }
        public Vector2i TileSize { get; internal set; }
        public IEnumerable<Layer> TileLayers { get { return _Layers; } }
        public IEnumerable<Pickup> Pickups { get { return _Pickups; } }
        public IEnumerable<Vector2f> SpawnPoints { get { return _SpawnPoints; } }
        public IEnumerable<CollisionShape> WallCollider { get { return _WallCollider; } }
        public IEnumerable<CollisionShape> Killzones { get { return _Killzones; } }

        private Func<int> IdProvider { get; }


        public TmxMapper(Func<int> idProvider)
        {
            IdProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }


        public bool Load(string name, CollisionSystem cSys)
        {
            try
            {
                // Load map data
                _MapData = new TmxMap(Path.Combine(_MAP_ROOT, name + ".tmx"));
                Name = name;
                ClearColor = new Color((byte)_MapData.BackgroundColor.R, (byte)_MapData.BackgroundColor.G, (byte)_MapData.BackgroundColor.B);
                _TextureColumnLookup = _MapData.Tilesets.ToDictionary(ts => ts.Name, ts => ts.Columns.Value);

                Size = new Vector2i(_MapData.Width, _MapData.Height);
                TileSize = new Vector2i(_MapData.TileWidth, _MapData.TileHeight);

                // Load Tile Layers
                _Layers = new List<Layer>(_MapData.Layers.Count);
                foreach (var layer in _MapData.Layers)
                {
                    var textureName = layer.Properties["TilesetName"];
                    var columns = _TextureColumnLookup[textureName];

                    Layer l = new Layer();
                    l.TextureName = textureName;
                    l.Offset = new Vector2f((float)(layer.OffsetX ?? 0), (float)(layer.OffsetY ?? 0));
                    l.Tiles = layer.Tiles.Select(t => new Tile(new Vector2f(t.X * TileSize.X, t.Y * TileSize.Y),
                                                               new Vector2i(((t.Gid - 1) % columns) * TileSize.X, ((t.Gid - 1) / columns) * TileSize.Y)
                                                )).ToArray();
                    _Layers.Add(l);
                }

                // Load Entities
                _Pickups = new List<Pickup>();
                _SpawnPoints = new List<Vector2f>();
                _WallCollider = new List<CollisionShape>();
                _Killzones = new List<CollisionShape>();
                foreach (var group in _MapData.ObjectGroups)
                {
                    //Log.Debug(group.Name);
                    foreach (var obj in group.Objects)
                    {

                        switch (obj.Type)
                        {
                            case "Pickup":
                                _Pickups.Add(ParsePickup(cSys, obj));
                                break;

                            case "Spawn":
                                _SpawnPoints.Add(new Vector2f((float)obj.X, (float)obj.Y) + new Vector2f((float)obj.Width, (float)obj.Height) / 2);
                                break;

                            case "Collision":
                                _WallCollider.Add(ParseCollisionShape(cSys, obj));
                                break;

                            case "Killzone":
                                _Killzones.Add(ParseCollisionShape(cSys, obj));
                                break;

                            default:
                                Log.Warning("Unknown map object type", obj.Type);
                                break;
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return false;
        }

        private Pickup ParsePickup(CollisionSystem cSys, TmxObject obj)
        {
            if (!Enum.TryParse<PickupType>(ReadProperty(obj.Properties, "Item"), out PickupType type))
            {
                Log.Warning("Invalid Pickup entry skipped");
                return null;
            }
            if (!Int32.TryParse(ReadProperty(obj.Properties, "Amount"), out int amount)) amount = 1;
            if (!Single.TryParse(ReadProperty(obj.Properties, "RespawnTime"), out float respawnTime)) respawnTime = 1;
            var position = new Vector2f((float)obj.X, (float)obj.Y) + new Vector2f((float)obj.Width, (float)obj.Height) / 2;

            return new Pickup(IdProvider(), position, type, amount, respawnTime, cSys);
        }

        private CollisionShape ParseCollisionShape(CollisionSystem cSys, TmxObject obj)
        {
            switch (obj.ObjectType)
            {
                case TmxObjectType.Basic: return new RectangleCollisionShape(cSys, new Vector2f((float)obj.X, (float)obj.Y), new Vector2f((float)obj.Width, (float)obj.Height));
                case TmxObjectType.Tile:  return new RectangleCollisionShape(cSys, new Vector2f((float)obj.X, (float)obj.Y), TileSize.ToVector2f());
                case TmxObjectType.Ellipse:  return new CircleCollisionShape(cSys, new Vector2f((float)obj.X, (float)obj.Y) - new Vector2f((float)obj.Width, (float)obj.Height) / 2, (float)obj.Width / 2);
                case TmxObjectType.Polygon: return new PolygonCollisionShape(cSys, new Vector2f((float)obj.X, (float)obj.Y), obj.Points.Select(point => new Vector2f((float)point.X, (float)point.Y)));
                case TmxObjectType.Polyline:
                    Log.Warning("Primitive", TmxObjectType.Polyline, "is currently not supported");
                    break;
                default:
                    Log.Warning("Unknown primitive", obj.ObjectType);
                    break;
            }
            return null;
        }

        private string ReadProperty(PropertyDict properties, string propertyName, string defaultValue = null)
        {
            return properties.TryGetValue(propertyName, out string value) ? value : defaultValue;
        }
    }
}