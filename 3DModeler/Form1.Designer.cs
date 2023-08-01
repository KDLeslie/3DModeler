namespace _3DModeler
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Viewer = new PictureBox();
            Clock = new System.Windows.Forms.Timer(components);
            FPS = new Label();
            label1 = new Label();
            label2 = new Label();
            MenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveASToolStripMenuItem = new ToolStripMenuItem();
            AddCube = new Button();
            ((System.ComponentModel.ISupportInitialize)Viewer).BeginInit();
            MenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // Viewer
            // 
            Viewer.Location = new Point(360, 60);
            Viewer.Name = "Viewer";
            Viewer.Size = new Size(1000, 800);
            Viewer.TabIndex = 0;
            Viewer.TabStop = false;
            Viewer.Paint += Viewer_Paint;
            // 
            // Clock
            // 
            Clock.Tick += Clock_Tick;
            // 
            // FPS
            // 
            FPS.AutoSize = true;
            FPS.Location = new Point(12, 68);
            FPS.Name = "FPS";
            FPS.Size = new Size(57, 32);
            FPS.TabIndex = 1;
            FPS.Text = "FPS:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 100);
            label1.Name = "label1";
            label1.Size = new Size(78, 32);
            label1.TabIndex = 2;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 132);
            label2.Name = "label2";
            label2.Size = new Size(78, 32);
            label2.TabIndex = 3;
            label2.Text = "label2";
            // 
            // MenuStrip
            // 
            MenuStrip.ImageScalingSize = new Size(32, 32);
            MenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            MenuStrip.Location = new Point(0, 0);
            MenuStrip.Name = "MenuStrip";
            MenuStrip.Size = new Size(1374, 40);
            MenuStrip.TabIndex = 4;
            MenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem, saveASToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(71, 36);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(229, 44);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(229, 44);
            saveToolStripMenuItem.Text = "Save";
            // 
            // saveASToolStripMenuItem
            // 
            saveASToolStripMenuItem.Name = "saveASToolStripMenuItem";
            saveASToolStripMenuItem.Size = new Size(229, 44);
            saveASToolStripMenuItem.Text = "Save As";
            saveASToolStripMenuItem.Click += saveASToolStripMenuItem_Click;
            // 
            // AddCube
            // 
            AddCube.Location = new Point(12, 167);
            AddCube.Name = "AddCube";
            AddCube.Size = new Size(150, 46);
            AddCube.TabIndex = 5;
            AddCube.Text = "Cube";
            AddCube.UseVisualStyleBackColor = true;
            AddCube.Click += AddCube_Click;
            AddCube.PreviewKeyDown += AddCube_PreviewKeyDown;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1374, 879);
            Controls.Add(AddCube);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(FPS);
            Controls.Add(Viewer);
            Controls.Add(MenuStrip);
            KeyPreview = true;
            MainMenuStrip = MenuStrip;
            Name = "Form1";
            Text = "3DModeler";
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            ((System.ComponentModel.ISupportInitialize)Viewer).EndInit();
            MenuStrip.ResumeLayout(false);
            MenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox Viewer;
        private System.Windows.Forms.Timer Clock;
        private Label FPS;
        private Label label1;
        private Label label2;
        private MenuStrip MenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private Button AddCube;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveASToolStripMenuItem;
    }
}