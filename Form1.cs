using System.Runtime.InteropServices;

namespace reversi_cs
{
    public partial class Form1 : Form
    {
        private readonly Button StartButton = new Button();

        public Form1()
        {
            InitializeComponent();

            this.Text = "Reversi Game";
            this.ClientSize = new Size(320, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Start Button
            this.StartButton.Text = "Start Game";
            this.StartButton.Size = new Size(200, 50);
            this.StartButton.Location = new Point((this.ClientSize.Width - this.StartButton.Width) / 2, (this.ClientSize.Height - this.StartButton.Height) / 2);
            this.StartButton.Click += new EventHandler(this.StartButton_Click);
            this.Controls.Add(this.StartButton);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            Form gameForm = new GameForm();
            gameForm.Owner = this;
            gameForm.StartPosition = FormStartPosition.CenterScreen;
            gameForm.Show();
            this.Enabled = false;
        }
    }
}
