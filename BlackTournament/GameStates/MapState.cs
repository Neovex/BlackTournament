using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SFML.System;
using SFML.Graphics;
using SFML.Audio;

using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.ParticleSystem;

using BlackTournament.Tmx;
using BlackTournament.Entities;
using BlackCoat.Entities.Shapes;
using BlackTournament.Net.Data;
using BlackTournament.Particles;

namespace BlackTournament.GameStates
{
    class MapState : Gamestate
    {
        // Rendering
        private TmxMapper _MapData;
        private View _View;

        // Sound
        private SfxManager _Sfx;

        // Emitters
        private LineInfo _LineInfo;
        private LineEmitter _LineEmitter;
        private LightningInfo _LigtningInfo;
        private LightningEmitter _LigtningEmitter;
        private PixelEmitter _SparkEmitter;
        private SparkInfo _SparkInfo;
        private ImpactInfo _ImpactInfo;
        private EmitterComposition _ImpactEmitter;

        private SmokeInfo _ExplosionInfo;
        private TextureParticleInitializationInfo _WaveInfo;
        private EmitterComposition _ExplosionEmitter;

        // Entities
        private Dictionary<int, IEntity> _EnitityLookup;
        private IEntity _LocalPlayer;

        public Vector2f ViewMovement { get; set; }
        

        public MapState(Core core, TmxMapper map) : base(core, map.Name, Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
            _MapData = map ?? throw new ArgumentNullException(nameof(map));
            _Sfx = new SfxManager(SfxLoader);
            _EnitityLookup = new Dictionary<int, IEntity>();
        }


        protected override bool Load()
        {
            // Setup View
            _View = new View(new FloatRect(new Vector2f(), _Core.DeviceSize.ToVector2f()));
            Layer_BG.View = _View;
            Layer_Game.View = _View;
            Layer_Overlay.View = _View;

            // Setup Map
            _Core.ClearColor = _MapData.ClearColor;
            foreach (var layer in _MapData.TileLayers)
            {
                var mapTex = TextureLoader.Load(layer.TextureName);
                var mapLayer = new MapRenderer(_Core, _MapData.Size, mapTex, _MapData.TileSize);
                mapLayer.Position = layer.Offset;

                int i = 0;
                foreach (var tile in layer.Tiles)
                {
                    mapLayer.AddTile(i * 4, tile.Position, tile.TexCoords); // mayhaps find a better solution
                    i++;
                }
                Layer_BG.AddChild(mapLayer);
            }

            // Set camera to the center of the map
            _View.Center = _MapData.Pickups.FirstOrDefault(p => p.Type == PickupType.BigShield)?.Position ?? _View.Center; // TODO: add center pos to mapdata


            // Load Sounds
            _Sfx.LoadFromDirectory();
            // Setup Sounds
            foreach (var sfx in Files.GAME_SFX) _Sfx.AddToLibrary(sfx, 100, true); // doublecheck

            // Setup Special Effects
            SetupEmitters();

            // TESTING ############################################

            // Debug Views
            /*
            var wallColor = new Color(155, 155, 155, 155);
            foreach (var shape in _MapData.WallCollider) // Collision
            {
                if (shape is RectangleCollisionShape)
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
                else if (shape is PolygonCollisionShape)
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
            foreach (var pos in _MapData.SpawnPoints) // Spawns
            {
                Layer_BG.AddChild(new Rectangle(_Core)
                {
                    Position = pos,
                    Size = new Vector2f(10, 10),
                    Origin = new Vector2f(5, 5),
                    OutlineThickness = 1,
                    OutlineColor = Color.Blue,
                    FillColor = Color.White,
                    Alpha = 0.3f
                });
            }
            */
            return true;
        }

        private void SetupEmitters()
        {
            // Tracer and Laser Lines
            _LineInfo = new LineInfo()
            {
                TTL = 25,
                UseAlphaAsTTL = true
            };
            _LineEmitter = new LineEmitter(_Core, _LineInfo);

            // Electro Sparks Effect
            _LigtningInfo = new LightningInfo(_Core)
            {
                TTL =25,
                ParticlesPerSpawn = 10,
                Color = Color.Cyan,
                AlphaFade =-3,
                UseAlphaAsTTL =true
            };
            _LigtningEmitter = new LightningEmitter(_Core, _LigtningInfo);

            // Pixel Sparks for ricochets and wall impacts
            _SparkInfo = new SparkInfo(_Core, 200, 10, null)
            {
                TTL = 0.2f,
                ParticlesPerSpawn = 4,
                DefaultColor = new Color(255, 255, 120, 255),
                Acceleration = new Vector2f(0, 950)
            };
            _SparkEmitter = new PixelEmitter(_Core, _SparkInfo);

            // Wall impact glow
            _ImpactInfo = new ImpactInfo(_Core)
            {
                TTL = 25,
                AlphaFade = -0.7f,
                Color = Color.Yellow,
                UseAlphaAsTTL = true
            };
            var impactEmitter = new PixelEmitter(_Core, _ImpactInfo);

            // Combine sparks and impact
            _ImpactEmitter = new EmitterComposition(_Core);
            _ImpactEmitter.Add(_SparkEmitter);
            _ImpactEmitter.Add(impactEmitter);

            // Orange/White Explosion
            var smokeTex = TextureLoader.Load(Files.Emitter_Smoke_Grey);
            _ExplosionInfo = new SmokeInfo(_Core, smokeTex, 50)
            {
                TTL = 25,
                ParticlesPerSpawn = 25,
                Color = new Color(255, 100, 20, 255),
                Scale = new Vector2f(0.5f, 0.5f),
                Origin = smokeTex.Size.ToVector2f() / 2,
                AlphaFade = -1,
                UseAlphaAsTTL = true
            };
            var explosionSmokeEmitter = new TextureEmitter(_Core, _ExplosionInfo, BlendMode.Add);

            // Explosion shock wave
            var shockTex = TextureLoader.Load(Files.Emitter_Shockwave);
            _WaveInfo = new TextureParticleInitializationInfo(shockTex)
            {
                TTL = 25,
                Alpha = 0.75f,
                Origin = shockTex.Size.ToVector2f() / 2,
                Scale = new Vector2f(0.05f, 0.05f),
                ScaleVelocity = new Vector2f(4f, 4f),
                AlphaFade = -2.5f,
                UseAlphaAsTTL = true
            };
            var explosionWaveEmitter = new TextureEmitter(_Core, _WaveInfo);

            // Combine Explosion core and shock wave
            _ExplosionEmitter = new EmitterComposition(_Core);
            _ExplosionEmitter.Add(explosionSmokeEmitter);
            _ExplosionEmitter.Add(explosionWaveEmitter);

            // Add to scene via host
            var host = new ParticleEmitterHost(_Core);
            host.AddEmitter(_LineEmitter);
            host.AddEmitter(_LigtningEmitter);
            host.AddEmitter(_ImpactEmitter);
            host.AddEmitter(_ExplosionEmitter);
            Layer_Overlay.AddChild(host);
        }

        protected override void Update(float deltaT)
        {
            // spectator movement
            _View.Center += ViewMovement * 2000 * deltaT;
            // Update listener position for spatial sounds
            Listener.Position = _View.Center.ToVector3f();
        }

        protected override void Destroy()
        {
        }

        public void CreatePlayer(int id, bool isLocalPlayer = false)
        {
            var player = new Graphic(_Core)
            {
                Texture = TextureLoader.Load("CharacterBase"),
                Color = Color.Red,
                Scale = new Vector2f(0.5f, 0.5f) // FIXME!
            };
            player.Origin = player.Texture.Size.ToVector2f() / 2;
            Layer_Game.AddChild(player);
            _EnitityLookup.Add(id, player);

            if (isLocalPlayer) _LocalPlayer = player;
        }

        public void CreatePickup(int id, PickupType type, Vector2f position, bool visible)
        {
            var tex = TextureLoader.Load(type.ToString());
            var entity = new Graphic(_Core)
            {
                Texture = tex,
                Scale = new Vector2f(0.4f, 0.4f), // FIXME!
                Position = position,
                Origin = tex.Size.ToVector2f() / 2,
                Visible = visible
            };
            _EnitityLookup.Add(id, entity);
            Layer_Game.AddChild(entity);
        }

        public void CreateProjectile(int id, PickupType type, Vector2f position, float rotation, bool primary)
        {
            switch (type) // todo : replace debug fx with proper shell/emitter fx
            {
                case PickupType.Drake:
                case PickupType.Thumper:
                    // create grenade
                    var size = new Vector2f(8, 4);
                    var grenate = new Rectangle(_Core)
                    {
                        Position = position,
                        Size = size,
                        Origin = size / 2,
                        Rotation = rotation,
                        Color = Color.Black
                    };
                    if (primary)
                    {
                        grenate.OutlineColor = Color.White;
                        grenate.OutlineThickness = 0.5f;
                    }
                    _EnitityLookup.Add(id, grenate);
                    Layer_Game.AddChild(grenate);
                    break;

                case PickupType.Hedgeshock:
                    // create shock orb
                    var orb = new Circle(_Core)
                    {
                        Position = position,
                        Radius = 20,
                        Color = Color.Cyan,
                        Alpha = 0.8f
                    };
                    _EnitityLookup.Add(id, orb);
                    Layer_Game.AddChild(orb);
                    break;
            }
        }

        public void CreateEffect(EffectType effect, Vector2f position, float rotation, PickupType source, bool primary, float size = 0)
        {
            switch (effect)
            {
                case EffectType.Environment:
                    // None yet
                    break;

                case EffectType.Explosion:
                    _Sfx.Play(Files.Sfx_Explosion, position);
                    _ExplosionInfo.Scale = VectorExtensions.VectorFromValue(size / 120);
                    _ExplosionEmitter.Position = position;
                    _ExplosionEmitter.Trigger();
                    break;

                case EffectType.WallImpact:
                    _SparkInfo.Update(source); 
                    _ImpactInfo.Update(source);
                    _ImpactEmitter.Position = position;
                    _ImpactEmitter.Trigger();
                    break;

                case EffectType.PlayerImpact:
                    _SparkInfo.Update(PickupType.Drake);
                    _SparkInfo.Color = Color.Red;
                    _SparkInfo.Direction = rotation;
                    _SparkEmitter.Position = position;
                    _SparkEmitter.Trigger();
                    _SparkInfo.Direction = null;
                    break;

                case EffectType.Gunfire:
                    if (primary)
                    {
                        switch (source)
                        {
                            case PickupType.Drake:
                                _Sfx.Play(Files.Sfx_Simpleshot, position);
                                _LineInfo.TargetOffset = VectorExtensions.VectorFromAngleLookup(rotation, size);
                                _LineInfo.Color = Color.White;
                                _LineInfo.Alpha = 0.5f;
                                _LineInfo.AlphaFade = -3;
                                _LineEmitter.Position = position;
                                _LineEmitter.Trigger();
                                break;

                            case PickupType.Hedgeshock:
                                _Sfx.Play(Files.Sfx_Spark, position);
                                _LigtningInfo.LigtningTarget = position + VectorExtensions.VectorFromAngleLookup(rotation, size);
                                _LigtningEmitter.Position = position;
                                _LigtningEmitter.Trigger();
                                break;

                            case PickupType.Thumper:
                                _Sfx.Play(Files.Sfx_Grenatelauncher, position);
                                break;

                            case PickupType.Titandrill:
                                _Sfx.Play(Files.Sfx_LaserBlastSmall, position);
                                _LineInfo.TargetOffset = VectorExtensions.VectorFromAngleLookup(rotation, size);
                                _LineInfo.Color = Color.Yellow;
                                _LineInfo.Alpha = 0.8f;
                                _LineInfo.AlphaFade = -2;
                                _LineEmitter.Position = position;
                                _LineEmitter.Trigger();
                                break;
                        }
                    }
                    else
                    {
                        switch (source)
                        {
                            case PickupType.Drake:
                            case PickupType.Thumper:
                                _Sfx.Play(Files.Sfx_Grenatelauncher, position);
                                break;
                            case PickupType.Hedgeshock:
                                _Sfx.Play(Files.Sfx_Pew, position);
                                break;
                            case PickupType.Titandrill:
                                _Sfx.Play(Files.Sfx_LaserBlastBig, position);
                                var target = VectorExtensions.VectorFromAngleLookup(rotation, size);
                                _LineInfo.TargetOffset = target;
                                _LineInfo.Color = new Color(255, 100,100);
                                _LineInfo.Alpha = 1f;
                                _LineInfo.AlphaFade = -2;
                                _LineEmitter.Position = position;
                                _LineEmitter.Trigger();
                                break;
                        }
                    }
                    break;
            }
        }

        public void DestroyEntity(int id)
        {
            var entity = _EnitityLookup[id];
            entity.Parent.RemoveChild(entity);
            _EnitityLookup.Remove(id);
        }

        public void UpdateEntity(int id, Vector2f pos, float rotation, bool visible) // add pickup type? currently not necessary
        {
            var entity = _EnitityLookup[id];
            entity.Position = pos;
            entity.Rotation = rotation;
            entity.Visible = visible;
        }

        public void FocusPlayer()
        {
            _View.Center = _LocalPlayer.Position;
        }

        public void RotatePlayer(float angle)
        {
            _LocalPlayer.Rotation = angle;
            //_View.Rotation = -_Player.Rotation; // cool effect - see later if this can be used (with game-pads maybe)
        }

        public override string ToString()
        {
            return $"Map: \"{Name}\"";
        }
    }
}