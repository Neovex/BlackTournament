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
        private Label _PosLabel;
        private Label _TotalLabel;



        public HUD(Core core, TextureLoader texLoader) : base(core, core?.DeviceSize)
        {
            // Build HUD
            var top = new AlignedContainer(_Core, Alignment.CenterTop,
                    new UIGraphic(_Core, texLoader.Load(Files.HUD_TopBg)) { Name = "TOP BG" },
                    new OffsetContainer(_Core, true,
                            _ScoreLabel = new Label(_Core, "0", 40, Game.DefaultFont) { Position = new Vector2f(0, 16), Name = "Score" },
                            new UIGraphic(_Core, texLoader.Load(Files.HUD_Skull)) { Position = new Vector2f(0, 0), Name = "Skull" }
                            )
                    {
                        Position = new Vector2f(16, 0),
                        Offset = 10
                    },
                    new AlignedContainer(_Core, Alignment.CenterTop, _TimeLabel = new Label(_Core, "TIME", 20, Game.DefaultFont) { Name = "Time", Position=new Vector2f(0,25) } ),
                    new AlignedContainer(_Core, Alignment.TopRight,
                            _PosLabel = new Label(_Core, "1", 40, Game.DefaultFont) { Position = new Vector2f(0, 16), Name = "Pos" },
                            _TotalLabel = new Label(_Core, "/10", 20, Game.DefaultFont) { Position = new Vector2f(0, 30), Name = "Total" }
                            )
            );
            // CSH
            Add(top);


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
