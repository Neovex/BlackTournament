using System;
using System.Collections.Generic;
using System.Linq;

using SFML.System;
using SFML.Graphics;

using BlackCoat;
using BlackCoat.UI;
using BlackTournament.Net.Data;

namespace BlackTournament.UI
{
    class ScoreBoard : AlignedContainer
    {
        private static readonly Object _TAG = new Object();

        private readonly Label[] _Template;
        private readonly OffsetContainer _Entries;

        public ScoreBoard(Core core) : base(core, Alignment.Center)
        {
            Visible = false;
            BackgroundColor = Color.Black;
            BackgroundAlpha = 0.6f;
            _Background.OutlineThickness = 1;
            _Background.OutlineColor = Color.Red;

            _Template = new Label[4];

            Add(new Canvas(_Core, new Vector2f(500, 300),
                _Entries = new OffsetContainer(_Core, Orientation.Vertical, 8,
                                new DistributionContainer(_Core, Orientation.Horizontal, null,
                                    _Template[0] = new Label(_Core, "Rank", 16, Game.DefaultFont) { TextColor = Color.Cyan },
                                    _Template[1] = new Label(_Core, "Player", 16, Game.DefaultFont) { TextColor = Color.Cyan },
                                    _Template[2] = new Label(_Core, "Score", 16, Game.DefaultFont) { TextColor = Color.Cyan },
                                    _Template[3] = new Label(_Core, "Ping", 16, Game.DefaultFont) { TextColor = Color.Cyan }
                                )
                                {
                                    Margin = new FloatRect(15, 15, 15, 0)
                                }
                    )
                    {
                        DockX = true
                    }
                )
            );
        }

        public void Update(IEnumerable<ClientPlayer> players)
        {
            if (!Visible) return;

            var playerArray = players.ToArray();
            var requiredEntries = playerArray.Length - _Entries.GetAll<Canvas>().Count(c => c.Tag == _TAG);

            // Create / Remove Entries
            if (requiredEntries > 0)
            {
                for (int i = 0; i < requiredEntries; i++)
                {
                    var entry = new Canvas(_Core)
                    {
                        DockX = true,
                        Margin = new FloatRect(15, 0, 0, 0),
                        Tag = _TAG
                    };
                    for (int j = 0; j < 4; j++)
                    {
                        entry.Add(new Label(_Core, String.Empty, 16, Game.DefaultFont)
                        {
                            Position = new Vector2f(_Template[j].Position.X + _Template[j].InnerSize.X, 0),
                            Alignment = TextAlignment.Right
                        });
                    }
                    _Entries.Add(entry);
                }
            }
            else if(requiredEntries < 0)
            {
                foreach (var container in _Entries.GetAll<Canvas>().Where(c => c.Tag == _TAG).Take(-requiredEntries))
                {
                    _Entries.Remove(container);
                }
            }

            // Update Content
            var containerArray = _Entries.GetAll<Canvas>().Where(c => c.Tag == _TAG).ToArray();
            for (int i = 0; i < playerArray.Length; i++)
            {
                var labels = containerArray[i].GetAll<Label>().ToArray();
                labels[0].Text = (i + 1).ToString();
                labels[1].Text = playerArray[i].Alias;
                labels[2].Text = playerArray[i].Score.ToString();
                labels[3].Text = playerArray[i].Ping.ToString();
            }
        }
    }
}