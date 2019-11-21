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


        private Label _ScoreLabel;
        private Label _TimeLabel;
        private Label _RankLabel;
        private Label _TotalLabel;
        private Label _HealthLabel;
        private Label _ShieldLabel;

        public HUD(Core core, TextureLoader texLoader) : base(core, core?.DeviceSize)
        {
            // Build HUD
            // Score and Time
            var snt = new AlignedContainer(_Core, Alignment.CenterTop,
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_TopBg)) { Name = "TOP BG" },
                    new OffsetContainer(_Core, true,
                            _ScoreLabel = new Label(_Core, "0", 40, Game.DefaultFont)
                            {
                                Name = "Score",
                                Position = (0, 16),
                                Padding = (0, 0, -10, 0) // Texts are weird
                            },
                            new UIGraphic(_Core, texLoader.Load(Files.HUD_Skull)) { Name = "Skull" }
                        )
                    {
                        Position = (16, 0),
                        Offset = 10
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
                Name = "TOP"
            };
            Add(snt);

            // Health and Shield
            var hns = new AlignedContainer(_Core, Alignment.CenterBottom,
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
                        Name = "Shield Label",
                        Margin = (0,7,45,0)
                    },
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_ShieldActive))
                    {
                        Name = "Shield Active Icon",
                        Position = (165, 1)
                    }
                )
            )
            {
                Name = "STAT",
                BackgroundColor = Color.Black,
                BackgroundAlpha = 0.35f,
                Margin = (0, 0, 0, 60)
            };
            Add(hns);
            // CH
            
            // Listen to Size Changes
            _Core.DeviceResized += Resize;
        }




        protected override void Destroy(bool disposing)
        {
            _Core.DeviceResized -= Resize;
            base.Destroy(disposing);
        }
    }
}
