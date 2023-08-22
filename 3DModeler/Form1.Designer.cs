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
            displayToolStripMenuItem = new ToolStripMenuItem();
            WireframeToolStripMenuItem = new ToolStripMenuItem();
            CullingToolStripMenuItem = new ToolStripMenuItem();
            ShadingToolStripMenuItem = new ToolStripMenuItem();
            solidToolStripMenuItem = new ToolStripMenuItem();
            addToolStripMenuItem = new ToolStripMenuItem();
            cubeToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            CameraToolStripMenuItem = new ToolStripMenuItem();
            WorldToolStripMenuItem = new ToolStripMenuItem();
            TextureToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)Viewer).BeginInit();
            MenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // Viewer
            // 
            Viewer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Viewer.Location = new Point(362, 68);
            Viewer.Name = "Viewer";
            Viewer.Size = new Size(1000, 850);
            Viewer.TabIndex = 0;
            Viewer.TabStop = false;
            Viewer.Paint += Viewer_Paint;
            Viewer.MouseDown += Viewer_MouseDown;
            Viewer.MouseMove += Viewer_MouseMove;
            Viewer.MouseUp += Viewer_MouseUp;
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
            MenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, displayToolStripMenuItem, addToolStripMenuItem, resetToolStripMenuItem });
            MenuStrip.Location = new Point(0, 0);
            MenuStrip.Name = "MenuStrip";
            MenuStrip.Size = new Size(1374, 42);
            MenuStrip.TabIndex = 4;
            MenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem, saveASToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(71, 38);
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
            // displayToolStripMenuItem
            // 
            displayToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { WireframeToolStripMenuItem, CullingToolStripMenuItem, ShadingToolStripMenuItem, solidToolStripMenuItem, TextureToolStripMenuItem });
            displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            displayToolStripMenuItem.Size = new Size(111, 38);
            displayToolStripMenuItem.Text = "Display";
            // 
            // WireframeToolStripMenuItem
            // 
            WireframeToolStripMenuItem.CheckOnClick = true;
            WireframeToolStripMenuItem.Name = "WireframeToolStripMenuItem";
            WireframeToolStripMenuItem.Size = new Size(359, 44);
            WireframeToolStripMenuItem.Text = "Wireframe";
            // 
            // CullingToolStripMenuItem
            // 
            CullingToolStripMenuItem.Checked = true;
            CullingToolStripMenuItem.CheckOnClick = true;
            CullingToolStripMenuItem.CheckState = CheckState.Checked;
            CullingToolStripMenuItem.Name = "CullingToolStripMenuItem";
            CullingToolStripMenuItem.Size = new Size(359, 44);
            CullingToolStripMenuItem.Text = "Culling";
            // 
            // ShadingToolStripMenuItem
            // 
            ShadingToolStripMenuItem.Checked = true;
            ShadingToolStripMenuItem.CheckOnClick = true;
            ShadingToolStripMenuItem.CheckState = CheckState.Checked;
            ShadingToolStripMenuItem.Name = "ShadingToolStripMenuItem";
            ShadingToolStripMenuItem.Size = new Size(359, 44);
            ShadingToolStripMenuItem.Text = "Shading";
            // 
            // solidToolStripMenuItem
            // 
            solidToolStripMenuItem.Checked = true;
            solidToolStripMenuItem.CheckOnClick = true;
            solidToolStripMenuItem.CheckState = CheckState.Checked;
            solidToolStripMenuItem.Name = "solidToolStripMenuItem";
            solidToolStripMenuItem.Size = new Size(359, 44);
            solidToolStripMenuItem.Text = "Solid";
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cubeToolStripMenuItem });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new Size(92, 38);
            addToolStripMenuItem.Text = "Add...";
            // 
            // cubeToolStripMenuItem
            // 
            cubeToolStripMenuItem.Name = "cubeToolStripMenuItem";
            cubeToolStripMenuItem.Size = new Size(203, 44);
            cubeToolStripMenuItem.Text = "Cube";
            cubeToolStripMenuItem.Click += cubeToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { CameraToolStripMenuItem, WorldToolStripMenuItem });
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new Size(106, 38);
            resetToolStripMenuItem.Text = "Reset...";
            // 
            // CameraToolStripMenuItem
            // 
            CameraToolStripMenuItem.Name = "CameraToolStripMenuItem";
            CameraToolStripMenuItem.Size = new Size(228, 44);
            CameraToolStripMenuItem.Text = "Camera";
            CameraToolStripMenuItem.Click += cameraToolStripMenuItem_Click;
            // 
            // WorldToolStripMenuItem
            // 
            WorldToolStripMenuItem.Name = "WorldToolStripMenuItem";
            WorldToolStripMenuItem.Size = new Size(228, 44);
            WorldToolStripMenuItem.Text = "World";
            WorldToolStripMenuItem.Click += WorldToolStripMenuItem_Click;
            // 
            // TextureToolStripMenuItem
            // 
            TextureToolStripMenuItem.Checked = true;
            TextureToolStripMenuItem.CheckOnClick = true;
            TextureToolStripMenuItem.CheckState = CheckState.Checked;
            TextureToolStripMenuItem.Name = "TextureToolStripMenuItem";
            TextureToolStripMenuItem.Size = new Size(359, 44);
            TextureToolStripMenuItem.Text = "Texture";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1374, 929);
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
            SizeChanged += Form1_SizeChanged;
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
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem saveASToolStripMenuItem;
        private ToolStripMenuItem displayToolStripMenuItem;
        private ToolStripMenuItem WireframeToolStripMenuItem;
        private ToolStripMenuItem CullingToolStripMenuItem;
        private ToolStripMenuItem ShadingToolStripMenuItem;
        private ToolStripMenuItem solidToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem cubeToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem CameraToolStripMenuItem;
        private ToolStripMenuItem WorldToolStripMenuItem;
        private ToolStripMenuItem TextureToolStripMenuItem;
    }
}