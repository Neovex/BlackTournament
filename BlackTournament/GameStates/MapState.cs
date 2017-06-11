using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.System;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Collision.Shapes;

using BlackTournament.Entities;
using BlackTournament.Tmx;
using BlackCoat.Entities.Shapes;

namespace BlackTournament.GameStates
{
    class MapState : BaseGamestate
    {
        private TmxMapper _MapData;

        private View _View;
        private Graphic _Player;

        public MapState(Core core, TmxMapper map) : base(core, map.Name)
        {
            _MapData = map ?? throw new ArgumentNullException(nameof(map));
        }

        protected override bool Load()
        {
            TextureManager.RootFolder = "Assets";

            // Setup View
            _View = new View(new FloatRect(0, 0, _Core.DeviceSize.X, _Core.DeviceSize.Y));
            Layer_BG.View = _View;

            // Setup Map
            _Core.ClearColor = _MapData.ClearColor;
            foreach (var layer in _MapData.TileLayers)
            {
                var mapLayer = new MapRenderer(_Core, _MapData.Size, TextureManager.Load(layer.TextureName), _MapData.TileSize);
                mapLayer.Position = layer.Offset;

                int i = 0;
                foreach (var tile in layer.Tiles)
                {
                    mapLayer.AddTile(i * 4, tile.Position, tile.TexCoords); // mayhaps find a better solution
                    i++;
                }
                Layer_BG.AddChild(mapLayer);
            }

            // TESTING ############################################

            // Debug Views
            var wallColor = new Color(155, 155, 155, 155); // TODO : maybe add remaining debug views
            foreach (var shape in _MapData.WallCollider)
            {
                if(shape is RectangleCollisionShape)
                {
                    var s = (RectangleCollisionShape)shape;
                    Layer_BG.AddChild(new Rectangle(_Core)
                    {
                        Position = s.Position,
                        Size = s.Size,
                        FillColor = wallColor,
                        OutlineColor = Color.Black,
                        OutlineThickness = 1
                    });
                }
                else if(shape is PolygonCollisionShape)
                {
                    var s = (PolygonCollisionShape)shape;
                    Layer_BG.AddChild(new Polygon(_Core, s.Points)
                    {
                        Position = s.Position,
                        FillColor = wallColor,
                        OutlineColor = Color.Black,
                        OutlineThickness = 1
                    });
                }
            }

            // Player
            _Player = new Graphic(_Core)
            {
                Texture = TextureManager.Load("CharacterBase"),
                Color = Color.Red,
                View = _View,
                Scale = new Vector2f(0.5f, 0.5f) // FIXME
            };
            _Player.Origin = _Player.Texture.Size.ToVector2f() / 2;
            Layer_Game.AddChild(_Player);

            return true;
        }

        protected override void Update(float deltaT)
        {
            // atm nix
        }

        protected override void Destroy()
        {
            // atm nix
        }

        public void Spawn(IEntity entity)
        {
            Layer_Game.AddChild(entity);
        }

        public void UpdatePosition(int id, float x, float y)
        {
            _Player.Position = new Vector2f(x, y);
            _View.Center = _Player.Position;
        }

        public void Rotate(float angle)
        {
            _Player.Rotation = angle;
            //_View.Rotation = -_Player.Rotation; // cool effect - see later if this can be used (with game-pads maybe)
        }

        public override string ToString()
        {
            return $"Map: \"{Name}\"";
        }
    }
}