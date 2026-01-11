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
        private readonly NumericUpDown alphaBetaDepthUpDown = new NumericUpDown();
        private readonly Label alphaBetaDepthLabel = new Label();

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
            blackPlayerCombo.Items.AddRange(new object[] { PlayerType.Human, PlayerType.Random, PlayerType.AlphaBetaNN });
            blackPlayerCombo.SelectedItem = PlayerType.Human;
            blackPlayerCombo.Location = new Point(120, 136);
            blackPlayerCombo.Width = 160;

            whiteLabel.Text = "White";
            whiteLabel.AutoSize = true;
            whiteLabel.Location = new Point(40, 190);

            whitePlayerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            whitePlayerCombo.Items.AddRange(new object[] { PlayerType.Human, PlayerType.Random, PlayerType.AlphaBetaNN });
            whitePlayerCombo.SelectedItem = PlayerType.Random;
            whitePlayerCombo.Location = new Point(120, 186);
            whitePlayerCombo.Width = 160;

            // AlphaBeta depth
            alphaBetaDepthLabel.Text = "AlphaBeta Depth";
            alphaBetaDepthLabel.AutoSize = true;
            alphaBetaDepthLabel.Location = new Point(40, 240);

            alphaBetaDepthUpDown.Minimum = 1;
            alphaBetaDepthUpDown.Maximum = 10;
            alphaBetaDepthUpDown.Value = 4;
            alphaBetaDepthUpDown.Location = new Point(160, 236);
            alphaBetaDepthUpDown.Width = 120;

            this.Controls.Add(blackLabel);
            this.Controls.Add(blackPlayerCombo);
            this.Controls.Add(whiteLabel);
            this.Controls.Add(whitePlayerCombo);
            this.Controls.Add(alphaBetaDepthLabel);
            this.Controls.Add(alphaBetaDepthUpDown);

            // Start Button
            this.StartButton.Text = "Start Game";
            this.StartButton.Size = new Size(200, 50);
            this.StartButton.Location = new Point((this.ClientSize.Width - this.StartButton.Width) / 2, 300);
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
                White = (PlayerType)(whitePlayerCombo.SelectedItem ?? PlayerType.Random),
                AlphaBetaDepth = (int)alphaBetaDepthUpDown.Value
            };

            Form gameForm = new GameForm(config);
            gameForm.Owner = this;
            gameForm.StartPosition = FormStartPosition.CenterScreen;
            gameForm.Show();
            this.Enabled = false;
        }
    }
}
