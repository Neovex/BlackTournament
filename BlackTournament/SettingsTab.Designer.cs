namespace BlackTournament
{
    partial class SettingsTab
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._ColorPanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this._PlayerNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._ColorDialog = new System.Windows.Forms.ColorDialog();
            this.SuspendLayout();
            // 
            // _ColorPanel
            // 
            this._ColorPanel.Location = new System.Drawing.Point(85, 37);
            this._ColorPanel.Name = "_ColorPanel";
            this._ColorPanel.Size = new System.Drawing.Size(134, 20);
            this._ColorPanel.TabIndex = 7;
            this._ColorPanel.Click += new System.EventHandler(this.ColorPanelClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Player Color";
            // 
            // _PlayerNameTextBox
            // 
            this._PlayerNameTextBox.Location = new System.Drawing.Point(85, 11);
            this._PlayerNameTextBox.Name = "_PlayerNameTextBox";
            this._PlayerNameTextBox.Size = new System.Drawing.Size(134, 20);
            this._PlayerNameTextBox.TabIndex = 5;
            this._PlayerNameTextBox.TextChanged += new System.EventHandler(this.PlayerNameTextBoxTextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Player Name";
            // 
            // _ColorDialog
            // 
            this._ColorDialog.AnyColor = true;
            this._ColorDialog.FullOpen = true;
            // 
            // SettingsTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._ColorPanel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._PlayerNameTextBox);
            this.Controls.Add(this.label1);
            this.Name = "SettingsTab";
            this.Size = new System.Drawing.Size(245, 73);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _ColorPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _PlayerNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColorDialog _ColorDialog;
    }
}
