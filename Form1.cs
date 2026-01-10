using System.Runtime.InteropServices;

namespace reversi_cs
{
    public partial class Form1 : Form
    {
        private readonly Button StartButton = new Button();
        private readonly ComboBox blackPlayerCombo = new ComboBox();
        private readonly ComboBox whitePlayerCombo = new ComboBox();
        private readonly Label blackLabel = new Label();
        private readonly Label whiteLabel = new Label();

        public Form1()
        {
            InitializeComponent();

            this.Text = "Reversi Game";
            this.ClientSize = new Size(320, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            blackLabel.Text = "Black";
            blackLabel.AutoSize = true;
            blackLabel.Location = new Point(40, 140);

            blackPlayerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            blackPlayerCombo.Items.AddRange(new object[] { PlayerType.Human, PlayerType.Random });
            blackPlayerCombo.SelectedItem = PlayerType.Human;
            blackPlayerCombo.Location = new Point(120, 136);
            blackPlayerCombo.Width = 160;

            whiteLabel.Text = "White";
            whiteLabel.AutoSize = true;
            whiteLabel.Location = new Point(40, 190);

            whitePlayerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            whitePlayerCombo.Items.AddRange(new object[] { PlayerType.Human, PlayerType.Random });
            whitePlayerCombo.SelectedItem = PlayerType.Random;
            whitePlayerCombo.Location = new Point(120, 186);
            whitePlayerCombo.Width = 160;

            this.Controls.Add(blackLabel);
            this.Controls.Add(blackPlayerCombo);
            this.Controls.Add(whiteLabel);
            this.Controls.Add(whitePlayerCombo);

            // Start Button
            this.StartButton.Text = "Start Game";
            this.StartButton.Size = new Size(200, 50);
            this.StartButton.Location = new Point((this.ClientSize.Width - this.StartButton.Width) / 2, 260);
            this.StartButton.Click += new EventHandler(this.StartButton_Click);
            this.Controls.Add(this.StartButton);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            var config = new GameConfig
            {
                Black = (PlayerType)(blackPlayerCombo.SelectedItem ?? PlayerType.Human),
                White = (PlayerType)(whitePlayerCombo.SelectedItem ?? PlayerType.Random)
            };

            Form gameForm = new GameForm(config);
            gameForm.Owner = this;
            gameForm.StartPosition = FormStartPosition.CenterScreen;
            gameForm.Show();
            this.Enabled = false;
        }
    }
}
