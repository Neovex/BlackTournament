using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackCoat.Entities;
using TiledSharp;
using BlackCoat;
using System.IO;
using SFML.Graphics;
using SFML.System;
using BlackCoat.Entities.Shapes;

namespace BlackTournament
{
    public class Map : Container // TODO: implement full tmx featureset
    {
        private const String _MAP_ROOT = "Maps";

        private String _Name;
        private TmxMap _MapData;
        private Dictionary<TmxTileset, Texture> _TileTextures;

        // HACK
        public List<IEntity> Polys = new List<IEntity>();



        public Map(Core core, String name):base(core)
        {
            _Name = name;
        }

        public bool Load()
        {
            try
            {
                _MapData = new TmxMap(Path.Combine(_MAP_ROOT, _Name + ".tmx"));
                _Core.ClearColor = new Color((byte)_MapData.BackgroundColor.R, (byte)_MapData.BackgroundColor.G, (byte)_MapData.BackgroundColor.B);
                _TileTextures = _MapData.Tilesets.ToDictionary(ts => ts, ts => new Texture(ts.Image.Source));
                
                foreach (var layer in _MapData.Layers)
                {
                    var c = new Container(_Core);
                    c.Position = new Vector2f((float)(layer.OffsetX ?? 0), (float)(layer.OffsetY ?? 0));
                    foreach (var tile in layer.Tiles)
                    {
                        if (tile.Gid == 0) continue;
                        var set = _TileTextures.Where(kvp=>kvp.Key.FirstGid<=tile.Gid).Max();
                        var g = new Graphic(_Core);
                        g.Texture = set.Value;
                        g.Position = new Vector2f(tile.X * _MapData.TileWidth, tile.Y * _MapData.TileHeight);
                        g.TextureRect = new IntRect(((tile.Gid - 1) % set.Key.Columns.Value) * _MapData.TileWidth, ((tile.Gid-1) / set.Key.Columns.Value) * _MapData.TileHeight, _MapData.TileWidth, _MapData.TileHeight);
                        c.AddChild(g); // FIXME: reduce render calls
                    }
                    AddChild(c);
                }
                foreach (var group in _MapData.ObjectGroups) // TOOD : UNHACK THIS!
                {
                    Log.Debug(group.Name);
                    foreach (var obj in group.Objects)
                    {
                        switch (obj.ObjectType)
                        {
                            case TmxObjectType.Basic:
                                var r = new Rectangle(_Core)
                                {
                                    Position = new Vector2f((float)obj.X, (float)obj.Y),
                                    Size = new Vector2f((float)obj.Width, (float)obj.Height),
                                    Color = Color.Cyan
                                };
                                Polys.Add(r);
                                break;
                            case TmxObjectType.Tile:
                                break;
                            case TmxObjectType.Ellipse:
                                break;
                            case TmxObjectType.Polygon:
                                var poly = new Polygon(_Core)
                                {
                                    Position = new Vector2f((float)obj.X, (float)obj.Y),
                                    Color = Color.Blue
                                };
                                foreach (var point in obj.Points)
                                {
                                    poly[(int)poly.GetPointCount()] = new Vector2f((float)point.X, (float)point.Y);
                                }
                                Polys.Add(poly);
                                break;
                            case TmxObjectType.Polyline:
                                break;
                            default:
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

        public void Destroy()
        {
            foreach (var texture in _TileTextures.Values)
            {
                texture.Dispose();
            }
            _TileTextures = null;
            _MapData = null;
        }
    }
}
