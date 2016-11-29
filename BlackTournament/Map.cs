﻿using System;
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
    public class Map : Container // TODO: implement full tmx featureset
    {
        private const String _MAP_ROOT = "Maps";

        private String _Name;
        private TmxMap _MapData;
        private Dictionary<TmxTileset, Texture> _TileTextures;



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
                        c.AddChild(g);
                    }
                    AddChild(c);
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
