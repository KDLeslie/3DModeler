using System.Diagnostics;

namespace _3DModeler
{
    public partial class Form1 : Form
    {
        readonly Stopwatch sw = new Stopwatch();
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new System.Drawing.SolidBrush(Color.Black), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            // e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen greenPen = new Pen(Color.FromArgb(255, 0, 255, 0), 10);
            e.Graphics.FillRectangle(new System.Drawing.SolidBrush(Color.Red), new Rectangle(0, Cursor.Position.Y, pictureBox1.Width, pictureBox1.Height));
            //pictureBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            sw.Start();
            timer1.Interval = 20;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }


    }
}