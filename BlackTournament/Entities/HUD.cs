using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using BlackCoat;
using BlackCoat.UI;
using BlackCoat.Entities;
using SFML.Graphics;
using BlackTournament.Net.Data;

namespace BlackTournament.Entities
{
    class HUD : Canvas
    {
        // Game
        public int Kills { get; set; }
        public int Rank { get; set; }
        public int TotalPlayers { get; set; }
        public TimeSpan Time { get; set; }

        // Player
        public int Health { get; set; }
        public int Shield { get; set; }

        // Weapons
        public int DrakeSecondary { get; set; }
        public int TitanPrimary { get; set; }
        public int TitanSecundary { get; set; }
        public int HedgePrimary { get; set; }
        public int HedgeSecundary { get; set; }
        public int ThumperPrimary { get; set; }
        public int ThumperSecundary { get; set; }

        public override View View { get => null; set => base.View = value; } // Disable view inheritance


        private AlignedContainer _PlayerInfo;
        private AlignedContainer _Weapons;
        private Label _ScoreLabel;
        private Label _TimeLabel;
        private Label _RankLabel;
        private Label _TotalLabel;
        private Label _HealthLabel;
        private Label _ShieldLabel;
        private UIGraphic _ShieldActive;
        private UIGraphic _ShieldInactive;
        private WeaponIcon _Drake;
        private WeaponIcon _Hedge;
        private WeaponIcon _Thump;
        private WeaponIcon _Titan;


        public HUD(Core core, TextureLoader texLoader) : base(core, core?.DeviceSize)
        {
            // Build HUD
            // Score and Time
            var snt = new AlignedContainer(_Core, Alignment.CenterTop,
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_TopBg)) { Name = "TOP BG" },
                    new OffsetContainer(_Core, Orientation.Horizontal, 10,
                            _ScoreLabel = new Label(_Core, "0", 40, Game.DefaultFont)
                            {
                                Name = "Score",
                                Position = (0, 16),
                                Padding = (0, 0, -10, 0) // Texts are weird
                            },
                            new UIGraphic(_Core, texLoader.Load(Files.HUD_Skull)) { Name = "Skull" }
                        )
                    {
                        Position = (16, 0)
                    },
                    new AlignedContainer(_Core, Alignment.CenterTop, _TimeLabel = new Label(_Core, "TIME", 20, Game.DefaultFont)
                    {
                        Name = "Time",
                        Position = (0, 25)
                    }),
                    new AlignedContainer(_Core, Alignment.TopRight,
                            _RankLabel = new Label(_Core, "1", 40, Game.DefaultFont)
                            {
                                Name = "Rank",
                                Position = (0, 16)
                            },
                            _TotalLabel = new Label(_Core, "-10", 20, Game.DefaultFont)
                            {
                                Name = "Total",
                                Position = (6, 32),
                                Padding = (0, 0, 10, 0)
                            }
                        )
            )
            {
                Name = "Game"
            };
            Add(snt);

            // Health and Shield
            _PlayerInfo = new AlignedContainer(_Core, Alignment.CenterBottom,
                new Canvas(_Core, (200, 36),
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_Health))
                    {
                        Name = "Health Icon",
                        Position = (3, 2)
                    },
                    _HealthLabel = new Label(_Core, "100", 26, Game.DefaultFont)
                    {
                        Name = "Health",
                        Position = (45, 7)
                    },
                    new AlignedContainer(_Core, Alignment.TopRight,
                        _ShieldLabel = new Label(_Core, "100", 26, Game.DefaultFont)
                        {
                            Name = "Shield"
                        }
                    )
                    {
                        Margin = (0, 7, 45, 0)
                    },
                    _ShieldActive = new UIGraphic(_Core, texLoader.Load(Files.HUD_ShieldActive))
                    {
                        Name = "Shield Active Icon",
                        Position = (165, 1),
                        Visible = false
                    },
                    _ShieldInactive = new UIGraphic(_Core, texLoader.Load(Files.HUD_ShieldInactive))
                    {
                        Name = "Shield Inactive Icon",
                        Position = _ShieldActive.Position,
                        Visible = true
                    }
                )
            )
            {
                Name = "Player",
                BackgroundColor = Color.Black,
                BackgroundAlpha = 0.35f,
                Margin = (0, 0, 0, 60)
            };
            Add(_PlayerInfo);

            _Weapons = new AlignedContainer(_Core, Alignment.CenterBottom,
                new OffsetContainer(_Core, Orientation.Horizontal, 20,
                    _Drake = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Drake_White), texLoader.Load(Files.HUD_Drake_Outline)),
                    _Hedge = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Hedgeshock_White), texLoader.Load(Files.HUD_Hedgeshock_Outline)),
                    _Thump = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Thumper_White), texLoader.Load(Files.HUD_Thumper_Outline)),
                    _Titan = new WeaponIcon(_Core, texLoader.Load(Files.HUD_Titandrill_White), texLoader.Load(Files.HUD_Titandrill_Outline))
                )
            )
            {
                Name = "Weapons",
                Margin = (0, 0, 0, 12)
            };
            Add(_Weapons);

            // Listen to Size Changes
            _Core.DeviceResized += Resize;
        }

        public void SetPlayerInfoVisibility(bool v)
        {
            _PlayerInfo.Visible = v;
            _Weapons.Visible = v;
        }

        protected override void Destroy(bool disposing)
        {
            _Core.DeviceResized -= Resize;
            base.Destroy(disposing);
        }

        internal void SetPlayerWeapon(PickupType weapon)
        {
            _Drake.Active = false;
            _Hedge.Active = false;
            _Thump.Active = false;
            _Titan.Active = false;
            switch (weapon)
            {
                case PickupType.Hedgeshock:
                    _Hedge.Active = true;
                    break;
                case PickupType.Thumper:
                    _Thump.Active = true;
                    break;
                case PickupType.Titandrill:
                    _Titan.Active = true;
                    break;
                default:
                    _Drake.Active = true;
                    break;
            }
        }
    }
}