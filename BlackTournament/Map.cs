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

namespace BlackTournament
{
    public class Map : Container
    {
        private String _Path;
        private TmxMap _MapData;
        private Dictionary<TmxTileset, Texture> _TileTextures;

        public Map(Core core,String path):base(core)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);
            _Path = path;
        }

        public bool Load()
        {
            //try
            {
                _MapData = new TmxMap(_Path);
                _Core.ClearColor = new Color((byte)_MapData.BackgroundColor.R, (byte)_MapData.BackgroundColor.G, (byte)_MapData.BackgroundColor.B);
                _TileTextures = _MapData.Tilesets.ToDictionary(ts=>ts, ts => new Texture(ts.Image.Source));
                
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
                        c.AddChild(g);
                    }
                    AddChild(c);
                }
            }
            //catch (Exception e)
            {
                //_Core.Log(e);
            }
            return true;
        }


    }
}
