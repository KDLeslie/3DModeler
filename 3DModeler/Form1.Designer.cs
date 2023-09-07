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
            ViewWindow = new PictureBox();
            Clock = new System.Windows.Forms.Timer(components);
            MenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveASToolStripMenuItem = new ToolStripMenuItem();
            displayToolStripMenuItem = new ToolStripMenuItem();
            WireframeToolStripMenuItem = new ToolStripMenuItem();
            CullingToolStripMenuItem = new ToolStripMenuItem();
            ShadingToolStripMenuItem = new ToolStripMenuItem();
            SolidToolStripMenuItem = new ToolStripMenuItem();
            TextureToolStripMenuItem = new ToolStripMenuItem();
            addToolStripMenuItem = new ToolStripMenuItem();
            cubeToolStripMenuItem = new ToolStripMenuItem();
            resetToolStripMenuItem = new ToolStripMenuItem();
            CameraToolStripMenuItem = new ToolStripMenuItem();
            WorldToolStripMenuItem = new ToolStripMenuItem();
            CameraSpeedSlider = new TrackBar();
            CamSpeedLabel = new Label();
            CamSpeedUpDown = new NumericUpDown();
            ObjectList = new ListBox();
            ContextMenuStrip = new ContextMenuStrip(components);
            deleteToolStripMenuItem = new ToolStripMenuItem();
            TransformationBox = new ComboBox();
            LabelX = new Label();
            LabelY = new Label();
            LabelZ = new Label();
            LabelTransform = new Label();
            UpDownX = new NumericUpDown();
            UpDownY = new NumericUpDown();
            UpDownZ = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)ViewWindow).BeginInit();
            MenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CameraSpeedSlider).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CamSpeedUpDown).BeginInit();
            ContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)UpDownX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)UpDownY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)UpDownZ).BeginInit();
            SuspendLayout();
            // 
            // ViewWindow
            // 
            ViewWindow.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ViewWindow.Location = new Point(374, 43);
            ViewWindow.Name = "ViewWindow";
            ViewWindow.Size = new Size(1000, 800);
            ViewWindow.TabIndex = 0;
            ViewWindow.TabStop = false;
            ViewWindow.Click += ViewWindow_Click;
            ViewWindow.Paint += Viewer_Paint;
            ViewWindow.MouseDown += Viewer_MouseDown;
            ViewWindow.MouseMove += Viewer_MouseMove;
            ViewWindow.MouseUp += Viewer_MouseUp;
            // 
            // Clock
            // 
            Clock.Tick += Clock_Tick;
            // 
            // MenuStrip
            // 
            MenuStrip.BackColor = Color.FromArgb(255, 245, 245);
            MenuStrip.ImageScalingSize = new Size(32, 32);
            MenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, displayToolStripMenuItem, addToolStripMenuItem, resetToolStripMenuItem });
            MenuStrip.Location = new Point(0, 0);
            MenuStrip.Name = "MenuStrip";
            MenuStrip.Size = new Size(1374, 40);
            MenuStrip.TabIndex = 4;
            MenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveASToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(71, 36);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(244, 44);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // saveASToolStripMenuItem
            // 
            saveASToolStripMenuItem.Name = "saveASToolStripMenuItem";
            saveASToolStripMenuItem.Size = new Size(244, 44);
            saveASToolStripMenuItem.Text = "Save As...";
            saveASToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            // 
            // displayToolStripMenuItem
            // 
            displayToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { WireframeToolStripMenuItem, CullingToolStripMenuItem, ShadingToolStripMenuItem, SolidToolStripMenuItem, TextureToolStripMenuItem });
            displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            displayToolStripMenuItem.Size = new Size(111, 36);
            displayToolStripMenuItem.Text = "Display";
            // 
            // WireframeToolStripMenuItem
            // 
            WireframeToolStripMenuItem.CheckOnClick = true;
            WireframeToolStripMenuItem.Name = "WireframeToolStripMenuItem";
            WireframeToolStripMenuItem.Size = new Size(258, 44);
            WireframeToolStripMenuItem.Text = "Wireframe";
            // 
            // CullingToolStripMenuItem
            // 
            CullingToolStripMenuItem.Checked = true;
            CullingToolStripMenuItem.CheckOnClick = true;
            CullingToolStripMenuItem.CheckState = CheckState.Checked;
            CullingToolStripMenuItem.Name = "CullingToolStripMenuItem";
            CullingToolStripMenuItem.Size = new Size(258, 44);
            CullingToolStripMenuItem.Text = "Culling";
            // 
            // ShadingToolStripMenuItem
            // 
            ShadingToolStripMenuItem.Checked = true;
            ShadingToolStripMenuItem.CheckOnClick = true;
            ShadingToolStripMenuItem.CheckState = CheckState.Checked;
            ShadingToolStripMenuItem.Name = "ShadingToolStripMenuItem";
            ShadingToolStripMenuItem.Size = new Size(258, 44);
            ShadingToolStripMenuItem.Text = "Shading";
            // 
            // SolidToolStripMenuItem
            // 
            SolidToolStripMenuItem.Checked = true;
            SolidToolStripMenuItem.CheckOnClick = true;
            SolidToolStripMenuItem.CheckState = CheckState.Checked;
            SolidToolStripMenuItem.Name = "SolidToolStripMenuItem";
            SolidToolStripMenuItem.Size = new Size(258, 44);
            SolidToolStripMenuItem.Text = "Solid";
            // 
            // TextureToolStripMenuItem
            // 
            TextureToolStripMenuItem.Checked = true;
            TextureToolStripMenuItem.CheckOnClick = true;
            TextureToolStripMenuItem.CheckState = CheckState.Checked;
            TextureToolStripMenuItem.Name = "TextureToolStripMenuItem";
            TextureToolStripMenuItem.Size = new Size(258, 44);
            TextureToolStripMenuItem.Text = "Texture";
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cubeToolStripMenuItem });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new Size(77, 36);
            addToolStripMenuItem.Text = "Add";
            // 
            // cubeToolStripMenuItem
            // 
            cubeToolStripMenuItem.Name = "cubeToolStripMenuItem";
            cubeToolStripMenuItem.Size = new Size(203, 44);
            cubeToolStripMenuItem.Text = "Cube";
            cubeToolStripMenuItem.Click += CubeToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { CameraToolStripMenuItem, WorldToolStripMenuItem });
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new Size(91, 36);
            resetToolStripMenuItem.Text = "Reset";
            // 
            // CameraToolStripMenuItem
            // 
            CameraToolStripMenuItem.Name = "CameraToolStripMenuItem";
            CameraToolStripMenuItem.Size = new Size(228, 44);
            CameraToolStripMenuItem.Text = "Camera";
            CameraToolStripMenuItem.Click += CameraToolStripMenuItem_Click;
            // 
            // WorldToolStripMenuItem
            // 
            WorldToolStripMenuItem.Name = "WorldToolStripMenuItem";
            WorldToolStripMenuItem.Size = new Size(228, 44);
            WorldToolStripMenuItem.Text = "World";
            WorldToolStripMenuItem.Click += WorldToolStripMenuItem_Click;
            // 
            // CameraSpeedSlider
            // 
            CameraSpeedSlider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CameraSpeedSlider.Location = new Point(0, 753);
            CameraSpeedSlider.Maximum = 100;
            CameraSpeedSlider.Name = "CameraSpeedSlider";
            CameraSpeedSlider.Size = new Size(260, 90);
            CameraSpeedSlider.TabIndex = 5;
            CameraSpeedSlider.TickFrequency = 10;
            CameraSpeedSlider.Value = 8;
            CameraSpeedSlider.ValueChanged += CameraSpeedSlider_ValueChanged;
            // 
            // CamSpeedLabel
            // 
            CamSpeedLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CamSpeedLabel.AutoSize = true;
            CamSpeedLabel.Location = new Point(10, 718);
            CamSpeedLabel.Name = "CamSpeedLabel";
            CamSpeedLabel.Size = new Size(174, 32);
            CamSpeedLabel.TabIndex = 6;
            CamSpeedLabel.Text = "Camera Speed:";
            // 
            // CamSpeedUpDown
            // 
            CamSpeedUpDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CamSpeedUpDown.Location = new Point(266, 753);
            CamSpeedUpDown.Name = "CamSpeedUpDown";
            CamSpeedUpDown.Size = new Size(78, 39);
            CamSpeedUpDown.TabIndex = 7;
            CamSpeedUpDown.Value = new decimal(new int[] { 8, 0, 0, 0 });
            CamSpeedUpDown.ValueChanged += CamSpeedUpDown_ValueChanged;
            CamSpeedUpDown.KeyDown += CamSpeedUpDown_KeyDown;
            // 
            // ObjectList
            // 
            ObjectList.FormattingEnabled = true;
            ObjectList.ItemHeight = 32;
            ObjectList.Location = new Point(12, 43);
            ObjectList.Name = "ObjectList";
            ObjectList.Size = new Size(324, 260);
            ObjectList.TabIndex = 8;
            ObjectList.SelectedIndexChanged += ObjectList_SelectedIndexChanged;
            ObjectList.MouseUp += ObjectList_MouseUp;
            // 
            // ContextMenuStrip
            // 
            ContextMenuStrip.ImageScalingSize = new Size(32, 32);
            ContextMenuStrip.Items.AddRange(new ToolStripItem[] { deleteToolStripMenuItem });
            ContextMenuStrip.Name = "contextMenuStrip1";
            ContextMenuStrip.Size = new Size(159, 42);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(158, 38);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // TransformationBox
            // 
            TransformationBox.FormattingEnabled = true;
            TransformationBox.Items.AddRange(new object[] { "Translation", "Rotation", "Scale" });
            TransformationBox.Location = new Point(143, 517);
            TransformationBox.Name = "TransformationBox";
            TransformationBox.Size = new Size(193, 40);
            TransformationBox.TabIndex = 9;
            TransformationBox.Text = "Translation";
            TransformationBox.SelectedIndexChanged += TransformationBox_SelectedIndexChanged;
            // 
            // LabelX
            // 
            LabelX.AutoSize = true;
            LabelX.Location = new Point(151, 576);
            LabelX.Name = "LabelX";
            LabelX.Size = new Size(33, 32);
            LabelX.TabIndex = 13;
            LabelX.Text = "X:";
            // 
            // LabelY
            // 
            LabelY.AutoSize = true;
            LabelY.Location = new Point(151, 621);
            LabelY.Name = "LabelY";
            LabelY.Size = new Size(32, 32);
            LabelY.TabIndex = 14;
            LabelY.Text = "Y:";
            // 
            // LabelZ
            // 
            LabelZ.AutoSize = true;
            LabelZ.Location = new Point(150, 666);
            LabelZ.Name = "LabelZ";
            LabelZ.Size = new Size(33, 32);
            LabelZ.TabIndex = 15;
            LabelZ.Text = "Z:";
            // 
            // LabelTransform
            // 
            LabelTransform.AutoSize = true;
            LabelTransform.Location = new Point(12, 520);
            LabelTransform.Name = "LabelTransform";
            LabelTransform.Size = new Size(125, 32);
            LabelTransform.TabIndex = 16;
            LabelTransform.Text = "Transform:";
            // 
            // UpDownX
            // 
            UpDownX.Location = new Point(200, 569);
            UpDownX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            UpDownX.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            UpDownX.Name = "UpDownX";
            UpDownX.Size = new Size(136, 39);
            UpDownX.TabIndex = 17;
            UpDownX.ValueChanged += UpDownX_ValueChanged;
            // 
            // UpDownY
            // 
            UpDownY.Location = new Point(200, 614);
            UpDownY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            UpDownY.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            UpDownY.Name = "UpDownY";
            UpDownY.Size = new Size(136, 39);
            UpDownY.TabIndex = 18;
            UpDownY.ValueChanged += UpDownY_ValueChanged;
            // 
            // UpDownZ
            // 
            UpDownZ.Location = new Point(200, 659);
            UpDownZ.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            UpDownZ.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            UpDownZ.Name = "UpDownZ";
            UpDownZ.Size = new Size(136, 39);
            UpDownZ.TabIndex = 19;
            UpDownZ.ValueChanged += UpDownZ_ValueChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 245, 245);
            ClientSize = new Size(1374, 846);
            Controls.Add(UpDownZ);
            Controls.Add(UpDownY);
            Controls.Add(UpDownX);
            Controls.Add(LabelTransform);
            Controls.Add(LabelZ);
            Controls.Add(LabelY);
            Controls.Add(LabelX);
            Controls.Add(TransformationBox);
            Controls.Add(ObjectList);
            Controls.Add(CamSpeedUpDown);
            Controls.Add(CamSpeedLabel);
            Controls.Add(CameraSpeedSlider);
            Controls.Add(ViewWindow);
            Controls.Add(MenuStrip);
            KeyPreview = true;
            MainMenuStrip = MenuStrip;
            MinimumSize = new Size(600, 250);
            Name = "Form1";
            Text = "3DModeler";
            Load += Form1_Load;
            SizeChanged += Form1_SizeChanged;
            Click += Form1_Click;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            ((System.ComponentModel.ISupportInitialize)ViewWindow).EndInit();
            MenuStrip.ResumeLayout(false);
            MenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)CameraSpeedSlider).EndInit();
            ((System.ComponentModel.ISupportInitialize)CamSpeedUpDown).EndInit();
            ContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)UpDownX).EndInit();
            ((System.ComponentModel.ISupportInitialize)UpDownY).EndInit();
            ((System.ComponentModel.ISupportInitialize)UpDownZ).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox ViewWindow;
        private System.Windows.Forms.Timer Clock;
        private MenuStrip MenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveASToolStripMenuItem;
        private ToolStripMenuItem displayToolStripMenuItem;
        private ToolStripMenuItem WireframeToolStripMenuItem;
        private ToolStripMenuItem CullingToolStripMenuItem;
        private ToolStripMenuItem ShadingToolStripMenuItem;
        private ToolStripMenuItem SolidToolStripMenuItem;
        private ToolStripMenuItem addToolStripMenuItem;
        private ToolStripMenuItem cubeToolStripMenuItem;
        private ToolStripMenuItem resetToolStripMenuItem;
        private ToolStripMenuItem CameraToolStripMenuItem;
        private ToolStripMenuItem WorldToolStripMenuItem;
        private ToolStripMenuItem TextureToolStripMenuItem;
        private TrackBar CameraSpeedSlider;
        private Label CamSpeedLabel;
        private NumericUpDown CamSpeedUpDown;
        private ListBox ObjectList;
        private ContextMenuStrip ContextMenuStrip;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ComboBox TransformationBox;
        private Label LabelX;
        private Label LabelY;
        private Label LabelZ;
        private Label LabelTransform;
        private NumericUpDown UpDownX;
        private NumericUpDown UpDownY;
        private NumericUpDown UpDownZ;
    }
}