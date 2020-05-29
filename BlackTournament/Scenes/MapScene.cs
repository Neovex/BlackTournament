using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using SFML.System;
using SFML.Graphics;
using SFML.Audio;

using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;
using BlackCoat.Entities.Lights;
using BlackCoat.ParticleSystem;
using BlackCoat.AssetHandling;
using BlackCoat.Properties;

using BlackTournament.UI;
using BlackTournament.Tmx;
using BlackTournament.Entities;
using BlackTournament.Net.Data;
using BlackTournament.Particles;
using BlackTournament.InputMaps;

namespace BlackTournament.Scenes
{
    class MapScene : Scene
    {
        // Rendering
        private View _View;
        private UIInputMap _UiInput;
        private TmxMapper _MapData;
        private Lightmap _Lightmap;
        private ParticleEmitterHost _ParticleEmitterHost;

        // Sound
        private Music _Music;
        private SfxManager _Sfx;

        // Emitters
        private LineInfo _TracerLinesInfo;
        private LineEmitter _TracerLinesEmitter;

        private LightningInfo _LigtningInfo;
        private LightningEmitter _LigtningEmitter;

        private PixelEmitter _SparkEmitter;
        private SparkInfo _SparkInfo;

        private ImpactInfo _ImpactInfo;
        private EmitterComposition _ImpactEmitter;

        private SmokeInfo _ExplosionInfo;
        private TextureParticleInitializationInfo _WaveInfo;
        private EmitterComposition _ExplosionEmitter;

        private SmokeTrailEmitter _SmokeTrailEmitter;

        private TextureParticleInitializationInfo _LightEmitterInfo;
        private TextureEmitter _LightEmitter;

        // Entities
        private Dictionary<int, IEntity> _EnitityLookup;
        private IEntity _LocalPlayer;
        private Graphic _DamageIndicator;

        // Misc
        private readonly Vector2f _PlayerScale = new Vector2f(0.5f, 0.5f);


        public Vector2f ViewMovement { get; set; }
        public HUD HUD { get; private set; }


        public MapScene(Core core, TmxMapper map, UIInputMap uiInput) : base(core, map.Name, Game.TEXTURE_ROOT, Game.MUSIC_ROOT, Game.FONT_ROOT, Game.SFX_ROOT)
        {
            _MapData = map ?? throw new ArgumentNullException(nameof(map));
            _UiInput = uiInput;
            _Sfx = new SfxManager(SfxLoader, () => Properties.Settings.Default.SfxVolume);
            _EnitityLookup = new Dictionary<int, IEntity>();
        }


        protected override bool Load()
        {
            // Music
            _Music = MusicLoader.Load(Files.GAME_MUSIC.Skip(_Core.Random.Next(Files.GAME_MUSIC.Count)).First());
            _Music.Volume = Properties.Settings.Default.MusikVolume;
            _Music.Loop = true;
#if !DEBUG
            _Music.Play();
#endif

            // Setup View
            _View = new View(new FloatRect(new Vector2f(), _Core.DeviceSize));
            Layer_BG.View = _View;
            Layer_Game.View = _View;
            Layer_Overlay.View = _View;
            _Core.DeviceResized += UpdateViewOnDeviceResize;

            // Setup Particle Host
            _ParticleEmitterHost = new ParticleEmitterHost(_Core);

            // Setup Lighting
            _Lightmap = new Lightmap(_Core, new Vector2f(_MapData.TileSize.X * _MapData.Size.X, _MapData.TileSize.Y * _MapData.Size.Y));
            _Lightmap.RenderEachFrame = true;
            var lightFile = $"{Game.MAP_ROOT}{_MapData.Name}.bcl";
            if (File.Exists(lightFile)) _Lightmap.Load(TextureLoader, lightFile);
            else _Lightmap.Ambient = Color.White;
            Layer_Overlay.Add(_Lightmap);

            // Setup Map
            _Core.ClearColor = _MapData.ClearColor;
            foreach (var layer in _MapData.Layers)
            {
                switch (layer)
                {
                    case TileLayer tileLayer: // MAP TILES
                        var mapTex = TextureLoader.Load(tileLayer.Asset);
                        var mapLayer = new MapRenderer(_Core, _MapData.Size, mapTex, _MapData.TileSize)
                        {
                            Position = tileLayer.Offset
                        };

                        for (int i = 0; i < tileLayer.Tiles.Length; i++)
                        {
                            mapLayer.AddTile(i * 4, tileLayer.Tiles[i].Position, tileLayer.Tiles[i].TexCoords); // mayhaps find a better solution
                        }
                        Layer_BG.Add(mapLayer);
                        break;

                    case ObjectLayer objectLayer: // MAP ENTITIES
                        foreach (var obj in objectLayer.Objects)
                        {
                            IEntity entity = null;
                            switch (obj)
                            {
                                case RotorInfo rotor:
                                    entity = new RotatingGraphic(_Core, TextureLoader.Load(rotor.Asset), rotor.Speed)
                                    {
                                        Name = obj.Name,
                                        Position = rotor.Position,
                                        Rotation = rotor.Rotation,
                                        Origin = rotor.Origin,
                                        Scale = rotor.Scale
                                    };
                                    break;

                                case AssetActorInfo actor:
                                    var texture = TextureLoader.Load(actor.Asset);
                                    entity = new Graphic(_Core, texture)
                                    {
                                        Name = obj.Name,
                                        Position = actor.Position,
                                        Rotation = actor.Rotation,
                                        Origin = texture.Size.ToVector2f() / 2,
                                        Scale = actor.Scale,
                                    };
                                    break;
                            }
                            switch (obj.Parent)
                            {
                                case Parent.Layer_BG:
                                    Layer_BG.Add(entity);
                                    break;
                                case Parent.Layer_Game:
                                    Layer_Game.Add(entity);
                                    break;
                                case Parent.Layer_Overlay:
                                    Layer_Overlay.Add(entity);
                                    break;
                                case Parent.Layer_Debug:
                                    Layer_Debug.Add(entity);
                                    break;
                                case Parent.Light:
                                    entity.BlendMode = BlendMode.Add;
                                    _Lightmap.Add(entity);
                                    break;
                            }
                            if (obj.Tag != null)
                            {
                                var tagData = obj.Tag?.Split(';');
                                if (tagData.Length != 0)
                                {
                                    switch (tagData[0])
                                    {
                                        case "TorchEmitter":
                                            var emitter = new TextureEmitter(_Core, new FireInfo(_Core, TextureLoader.Load(Files.Emitter_Smoke_Grey), 25), BlendMode.Add);
                                            emitter.Position = entity.Position;
                                            emitter.Trigger();
                                            _ParticleEmitterHost.AddEmitter(emitter);
                                            break;
                                        case "Tint":
                                            entity.Color = new Color(byte.Parse(tagData[1]), byte.Parse(tagData[2]), byte.Parse(tagData[3]));
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            // Set camera to a nice spot of the map
            _View.Center = _MapData.Pickups.FirstOrDefault(p => p.Item == PickupType.BigShield)?.Position ?? _View.Center; // maybe add camera start pos to mapdata?

            // Load all Sounds
            _Sfx.LoadFromDirectory(parallelSounds: 5);
            // Manually setup some specific sounds due to sound stacking
            _Sfx.AddToLibrary(Files.Sfx_Hit, 1);
            _Sfx.AddToLibrary(Files.Sfx_Simpleshot, 20);
            _Sfx.AddToLibrary(Files.Sfx_Pew, 20);
            _Sfx.AddToLibrary(Files.Sfx_Spark, 20);

            // Setup Special Effects
            SetupEmitters();
            Layer_Overlay.Add(_ParticleEmitterHost);
            var indicatorTexture = TextureLoader.Load(nameof(Resources.Pointlight), Resources.Pointlight);
            _DamageIndicator = new Graphic(_Core, indicatorTexture)
            {
                Name = "DamageIndicator",
                Origin = indicatorTexture.Size.ToVector2f() / 2,
                Color = Color.Red,
                Alpha = 0
            };
            Layer_Overlay.Add(_DamageIndicator);

            // HUD
            Layer_Overlay.Add(HUD = new HUD(_Core, TextureLoader, _Sfx, _UiInput));

            return true;
        }

        private void UpdateViewOnDeviceResize(Vector2f size)
        {
            _View.Size = size;
        }

        private void SetupEmitters()
        {
            // Tracer and Laser Lines
            _TracerLinesInfo = new LineInfo()
            {
                TTL = 25,
                UseAlphaAsTTL = true
            };
            _TracerLinesEmitter = new LineEmitter(_Core, _TracerLinesInfo);
            _ParticleEmitterHost.AddEmitter(_TracerLinesEmitter);

            // Electro Sparks Effect
            _LigtningInfo = new LightningInfo(_Core)
            {
                TTL = 25,
                ParticlesPerSpawn = 10,
                Color = Color.Cyan,
                AlphaFade = -3,
                UseAlphaAsTTL = true
            };
            _LigtningEmitter = new LightningEmitter(_Core, _LigtningInfo);
            _ParticleEmitterHost.AddEmitter(_LigtningEmitter);

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
            _ParticleEmitterHost.AddEmitter(_ImpactEmitter);

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
            _ParticleEmitterHost.AddEmitter(_ExplosionEmitter);

            // Grenade Smoke Trail
            var smokeTrailInfo = new SmokeInfo(_Core, TextureLoader.Load(Files.Emitter_Smoke_Grey), 50)
            {
                TTL = 25,
                Alpha = 0.8f,
                ParticlesPerSpawn = 1,
                SpawnRate = 0.01f,
                Color = new Color(100, 100, 100),
                Scale = new Vector2f(0.05f, 0.05f),
                Origin = smokeTex.Size.ToVector2f() / 2,
                AlphaFade = -1,
                UseAlphaAsTTL = true
            };
            _SmokeTrailEmitter = new SmokeTrailEmitter(_Core, smokeTrailInfo, -1);
            _ParticleEmitterHost.AddEmitter(_SmokeTrailEmitter);

            // LIGHTING
            var lightTex = TextureLoader.Load(nameof(Resources.Pointlight), Resources.Pointlight);
            _LightEmitterInfo = new TextureParticleInitializationInfo(lightTex)
            {
                TTL = 25,
                Origin = lightTex.Size.ToVector2f() / 2,
                AlphaFade = -1,
                UseAlphaAsTTL = true
            };
            _LightEmitter = new TextureEmitter(_Core, _LightEmitterInfo, BlendMode.Add);
            // Add to scene
            var lighthost = new ParticleEmitterHost(_Core);
            lighthost.AddEmitter(_LightEmitter);
            _Lightmap.Add(lighthost);
        }

        protected override void Update(float deltaT)
        {
            // spectator movement
            _View.Center += ViewMovement * 2000 * deltaT;
            // Update listener position for spatial sounds
            _Sfx.ListenerPosition = _View.Center;
            // Update damage indicator
            _DamageIndicator.Position = _View.Center;
            _DamageIndicator.Alpha -= 5 * deltaT;
        }

        protected override void Destroy()
        {
            _Core.DeviceResized -= UpdateViewOnDeviceResize;
        }

        public void CreatePlayer(int id, bool isLocalPlayer = false)
        {
            var playerTexture = TextureLoader.Load(Files.Tex_CharacterBase);
            var player = new Graphic(_Core, playerTexture)
            {
                Name = (isLocalPlayer ? "Local " : "") + "Player",
                Origin = playerTexture.Size.ToVector2f() / 2,
                Scale = _PlayerScale
            };
            Layer_Game.Add(player);
            _EnitityLookup.Add(id, player);

            // add player light
            _Lightmap.AddCustomLight(new PlayerLight(_Core, player, TextureLoader));

            if (isLocalPlayer) _LocalPlayer = player;
        }

        public void CreatePickup(int id, PickupType type, Vector2f position, bool visible)
        {
            var tex = TextureLoader.Load(type.ToString());
            var entity = new Graphic(_Core)
            {
                Name = type.ToString(),
                Texture = tex,
                Scale = new Vector2f(0.48f, 0.48f),
                Position = position,
                Origin = tex.Size.ToVector2f() / 2,
                Visible = visible
            };
            _EnitityLookup.Add(id, entity);
            Layer_Game.Add(entity);

            var emTex = TextureLoader.Load(nameof(Resources.Pointlight), Resources.Pointlight);
            var emitter = new TextureEmitter(_Core, new SpawnInfo(_Core, emTex, type));
            _ParticleEmitterHost.AddEmitter(emitter);
            emitter.Position = position;
            emitter.Trigger();
        }

        public void CreateProjectile(int id, PickupType type, Vector2f position, float rotation, bool primary)
        {
            switch (type)
            {
                case PickupType.Drake:
                case PickupType.Thumper:
                    // create grenade
                    var grenate = new Container(_Core);
                    var size = new Vector2f(8, 4);
                    var grenateGfx = new Rectangle(_Core, size, Color.Black)
                    {
                        Origin = size / 2
                    };
                    if (primary)
                    {
                        grenateGfx.OutlineColor = Color.White;
                        grenateGfx.OutlineThickness = 0.5f;
                    }
                    // Add Smoke Trail
                    grenate.Add(grenateGfx);
                    grenate.Add(new Remote<SmokeTrailEmitter>(_SmokeTrailEmitter, _SmokeTrailEmitter.ParticleInfo.SpawnRate));
                    _EnitityLookup.Add(id, grenate);
                    Layer_Game.Add(grenate);
                    break;

                case PickupType.Hedgeshock:
                    // create shock orb
                    var shockCoreTex = TextureLoader.Load(Files.Emitter_Shockwave);
                    var shockOrb = new Container(_Core)
                    {
                        Texture = shockCoreTex,
                        Origin = shockCoreTex.Size.ToVector2f() / 2,
                        Scale = new Vector2f(0.15f, 0.15f),
                        Color = Color.Cyan
                    };
                    // Add lightning decoration
                    var orb = new Remote<LightningEmitter>(_LigtningEmitter, WeaponData.HedgeshockPrimary.FireRate / 2);
                    orb.AdditionalTriggers = 1;
                    orb.AboutToBeTriggered += e => e.ParticleInfo.LigtningTarget = orb.Position + Create.Vector2fFromAngleLookup(_Core.Random.NextFloat(0, 360), WeaponData.HedgeshockSecundary.Length);
                    shockOrb.Add(orb);
                    // Add light
                    var light = new OrbLight(_Core, shockOrb, TextureLoader);
                    _Lightmap.Add(light);
                    // Add to scene
                    _EnitityLookup.Add(id, shockOrb);
                    Layer_Game.Add(shockOrb);
                    break;
            }
        }

        public void CreateEffect(EffectType effect, Vector2f position, float rotation, PickupType source, bool primary, float size = 0)
        {
            switch (effect)
            {
                case EffectType.Environment:
                    // Not needed (yet)
                break;

                case EffectType.PlayerDrop:
                    _Sfx.Play(Files.Sfx_Drop, position);
                    // drop animation
                    var tex = TextureLoader.Load(Files.Tex_CharacterBase);
                    var dropper = new Graphic(_Core, tex)
                    {
                        Position = position,
                        Origin = tex.Size.ToVector2f() / 2,
                        Rotation = rotation,
                        Scale = _PlayerScale,
                    };
                    Layer_Game.Add(dropper);

                    _Core.AnimationManager.Run(1, 0, 0.5f,
                    v =>
                    {
                        dropper.Alpha = v;
                        dropper.Scale = _PlayerScale * v;
                    },
                    () => dropper.Dispose());
                break;

                case EffectType.Gore:
                    _Sfx.Play(Files.Sfx_Hit, position);
                    CreateEffect(EffectType.PlayerImpact, position, rotation, source, primary, size);
                break;

                case EffectType.Explosion:
                    _Sfx.Play(Files.Sfx_Explosion, position);
                    _ExplosionInfo.Scale = new Vector2f(size, size) / 120;
                    _ExplosionEmitter.Position = position;
                    _ExplosionEmitter.Trigger();
                    // Impact Light
                    _LightEmitterInfo.Scale = new Vector2f(0.9f, 0.9f);
                    _LightEmitter.Position = position;
                    _LightEmitter.Trigger();
                break;

                case EffectType.WallImpact:
                    _SparkInfo.Update(source);
                    _ImpactInfo.Update(source);
                    _ImpactEmitter.Position = position;
                    _ImpactEmitter.Trigger();
                    if (source == PickupType.Hedgeshock && !primary) _Sfx.Play(Files.Sfx_Pew, position);
                    // Impact Light
                    _LightEmitterInfo.Scale = new Vector2f(0.1f, 0.1f);
                    _LightEmitter.Position = position;
                    _LightEmitter.Trigger();
                break;

                case EffectType.PlayerImpact:
                    // hacky but works decent
                    _SparkEmitter.Position = position;
                    _SparkInfo.Update(PickupType.Drake);
                    _SparkInfo.Color = Color.Red;
                    _SparkInfo.ParticlesPerSpawn = (!primary && source == PickupType.Hedgeshock) ? 2u : 15u;
                    _SparkInfo.Direction = rotation;
                    _SparkInfo.Offset = new Vector2f(-10, 10);
                    _SparkEmitter.Trigger();
                    _SparkInfo.Offset = new Vector2f();
                    _SparkInfo.Direction = null;
                    // even more hacky :(
                    if (_LocalPlayer.Position.DistanceBetweenSquared(position) < 250)
                    {
                        _DamageIndicator.Alpha = 0.75f;
                        _Sfx.Play(Files.Sfx_Hit, position);
                    }
                break;

                case EffectType.Pickup:
                    switch (source)
                    {
                        case PickupType.SmallHealth:
                        case PickupType.SmallShield:
                            _Sfx.Play(Files.Sfx_Pickup3, position);
                        break;
                        case PickupType.BigHealth:
                        case PickupType.BigShield:
                            _Sfx.Play(Files.Sfx_Pickup4, position);
                        break;
                        case PickupType.Drake:
                        case PickupType.Hedgeshock:
                            _Sfx.Play(Files.Sfx_Pickup1, position);
                        break;
                        case PickupType.Thumper:
                        case PickupType.Titandrill:
                            _Sfx.Play(Files.Sfx_Pickup2, position);
                        break;
                    }
                    break;

                case EffectType.Gunfire:
                    if (primary)
                    {
                        switch (source)
                        {
                            case PickupType.Drake:
                                _Sfx.Play(Files.Sfx_Simpleshot, position);
                                _TracerLinesInfo.TargetOffset = Create.Vector2fFromAngleLookup(rotation, size);
                                _TracerLinesInfo.Color = Color.White;
                                _TracerLinesInfo.Alpha = 0.5f;
                                _TracerLinesInfo.AlphaFade = -3;
                                _TracerLinesEmitter.Position = position;
                                _TracerLinesEmitter.Trigger();
                            break;

                            case PickupType.Hedgeshock:
                                _Sfx.Play(Files.Sfx_Spark, position);
                                _LigtningInfo.LigtningTarget = position + Create.Vector2fFromAngleLookup(rotation, size);
                                _LigtningEmitter.Position = position;
                                _LigtningEmitter.Trigger();
                            break;

                            case PickupType.Thumper:
                                _Sfx.Play(Files.Sfx_Grenatelauncher, position);
                            break;

                            case PickupType.Titandrill:
                                _Sfx.Play(Files.Sfx_LaserBlastSmall, position);
                                _TracerLinesInfo.TargetOffset = Create.Vector2fFromAngleLookup(rotation, size);
                                _TracerLinesInfo.Color = Color.Yellow;
                                _TracerLinesInfo.Alpha = 0.8f;
                                _TracerLinesInfo.AlphaFade = -2;
                                _TracerLinesEmitter.Position = position;
                                _TracerLinesEmitter.Trigger();
                            break;
                        }
                    }
                    else // secondary
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
                                var target = Create.Vector2fFromAngleLookup(rotation, size);
                                _TracerLinesInfo.TargetOffset = target;
                                _TracerLinesInfo.Color = new Color(255, 100,100);
                                _TracerLinesInfo.Alpha = 1f;
                                _TracerLinesInfo.AlphaFade = -2;
                                _TracerLinesEmitter.Position = position;
                                _TracerLinesEmitter.Trigger();
                            break;
                        }
                    }
                 break;
            }
        }

        public void DestroyEntity(int id)
        {
            var entity = _EnitityLookup[id];
            entity.Parent.Remove(entity);
            _EnitityLookup.Remove(id);
            entity.Dispose();
        }

        public void UpdateEntity(int id, Vector2f pos, float rotation, bool visible) // add pickup type? currently not necessary
        {
            var entity = _EnitityLookup[id];
            entity.Position = pos;
            entity.Rotation = rotation;
            entity.Visible = visible;
        }

        public void UpdateColor(int id, uint color)
        {
            _EnitityLookup[id].Color = new Color(color);
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