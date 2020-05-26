using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackTournament.Properties;

namespace BlackTournament
{
    public partial class SettingsTab : UserControl
    {
        public Color SelectedColor { get; set; }

        public SettingsTab()
        {
            Text = "Game";
            InitializeComponent();
            _PlayerNameTextBox.Text = Settings.Default.PlayerName;
            _ColorPanel.BackColor = Settings.Default.PlayerColor;
        }

        private void ColorPanelClicked(object sender, EventArgs e)
        {
            if (_ColorDialog.ShowDialog() == DialogResult.OK)
            {
                _ColorPanel.BackColor = _ColorDialog.Color;
                Settings.Default.PlayerColor = _ColorDialog.Color;
            }
        }

        private void PlayerNameTextBoxTextChanged(object sender, EventArgs e)
        {
            Settings.Default.PlayerName = _PlayerNameTextBox.Text;
        }
    }
}