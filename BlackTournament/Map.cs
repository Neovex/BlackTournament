using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using SFML.Graphics;
using SFML.System;

using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;

using TiledSharp;


namespace BlackTournament
{
    // temorary MAP Renderer
    public class Map : Container // TODO: implement full tmx featureset
    {
        private const String _MAP_ROOT = "Maps";

        private String _Name;
        private TmxMap _MapData;
        private Dictionary<String, TilesetTexture> _TileTextures;

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
                _TileTextures = _MapData.Tilesets.ToDictionary(ts => ts.Name, ts => new TilesetTexture(ts.Name, ts.Columns.Value, new Texture(ts.Image.Source)));

                foreach (var layer in _MapData.Layers)
                {
                    var tex = _TileTextures[layer.Properties["TilesetName"]];
                    var c = new MapRenderer(_Core, new Vector2i(_MapData.Width, _MapData.Height), tex.Texture, new Vector2i(_MapData.TileWidth, _MapData.TileHeight));
                    c.Position = new Vector2f((float)(layer.OffsetX ?? 0), (float)(layer.OffsetY ?? 0));
                    for (int i = 0; i < layer.Tiles.Count; i++)
                    {
                        var tile = layer.Tiles[i];
                        c.AddTile(i * 4, new Vector2f(tile.X * _MapData.TileWidth, tile.Y * _MapData.TileHeight),
                            new Vector2i(((tile.Gid - 1) % tex.Columns) * _MapData.TileWidth, ((tile.Gid - 1) / tex.Columns) * _MapData.TileHeight));
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
                                var poly = new Polygon(_Core, obj.Points.Select(point=> new Vector2f((float)point.X, (float)point.Y)))
                                {
                                    Position = new Vector2f((float)obj.X, (float)obj.Y),
                                    Color = Color.Blue
                                };
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

    class TilesetTexture
    {
        public String Name { get; private set; }
        public int Columns { get; private set; }
        public Texture Texture { get; private set; }

        public TilesetTexture(string name, int columns, Texture texture)
        {
            Name = name;
            Columns = columns;
            Texture = texture;
        }

        public void Dispose()
        {
            Texture.Dispose();
        }
    }
}