﻿namespace _3DModeler
{
    partial class MainForm
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
            LabelObjectList = new Label();
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
            ViewWindow.Location = new Point(201, 20);
            ViewWindow.Margin = new Padding(2, 1, 2, 1);
            ViewWindow.Name = "ViewWindow";
            ViewWindow.Size = new Size(538, 388);
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
            MenuStrip.Padding = new Padding(3, 1, 0, 1);
            MenuStrip.Size = new Size(740, 24);
            MenuStrip.TabIndex = 4;
            MenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveASToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 22);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(123, 22);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // saveASToolStripMenuItem
            // 
            saveASToolStripMenuItem.Name = "saveASToolStripMenuItem";
            saveASToolStripMenuItem.Size = new Size(123, 22);
            saveASToolStripMenuItem.Text = "Save As...";
            saveASToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            // 
            // displayToolStripMenuItem
            // 
            displayToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { WireframeToolStripMenuItem, CullingToolStripMenuItem, ShadingToolStripMenuItem, SolidToolStripMenuItem, TextureToolStripMenuItem });
            displayToolStripMenuItem.Name = "displayToolStripMenuItem";
            displayToolStripMenuItem.Size = new Size(57, 22);
            displayToolStripMenuItem.Text = "Display";
            // 
            // WireframeToolStripMenuItem
            // 
            WireframeToolStripMenuItem.CheckOnClick = true;
            WireframeToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            WireframeToolStripMenuItem.Name = "WireframeToolStripMenuItem";
            WireframeToolStripMenuItem.Size = new Size(129, 22);
            WireframeToolStripMenuItem.Text = "Wireframe";
            // 
            // CullingToolStripMenuItem
            // 
            CullingToolStripMenuItem.Checked = true;
            CullingToolStripMenuItem.CheckOnClick = true;
            CullingToolStripMenuItem.CheckState = CheckState.Checked;
            CullingToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            CullingToolStripMenuItem.Name = "CullingToolStripMenuItem";
            CullingToolStripMenuItem.Size = new Size(129, 22);
            CullingToolStripMenuItem.Text = "Culling";
            // 
            // ShadingToolStripMenuItem
            // 
            ShadingToolStripMenuItem.Checked = true;
            ShadingToolStripMenuItem.CheckOnClick = true;
            ShadingToolStripMenuItem.CheckState = CheckState.Checked;
            ShadingToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            ShadingToolStripMenuItem.Name = "ShadingToolStripMenuItem";
            ShadingToolStripMenuItem.Size = new Size(129, 22);
            ShadingToolStripMenuItem.Text = "Shading";
            // 
            // SolidToolStripMenuItem
            // 
            SolidToolStripMenuItem.Checked = true;
            SolidToolStripMenuItem.CheckOnClick = true;
            SolidToolStripMenuItem.CheckState = CheckState.Checked;
            SolidToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            SolidToolStripMenuItem.Name = "SolidToolStripMenuItem";
            SolidToolStripMenuItem.Size = new Size(129, 22);
            SolidToolStripMenuItem.Text = "Solid";
            // 
            // TextureToolStripMenuItem
            // 
            TextureToolStripMenuItem.Checked = true;
            TextureToolStripMenuItem.CheckOnClick = true;
            TextureToolStripMenuItem.CheckState = CheckState.Checked;
            TextureToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            TextureToolStripMenuItem.Name = "TextureToolStripMenuItem";
            TextureToolStripMenuItem.Size = new Size(129, 22);
            TextureToolStripMenuItem.Text = "Texture";
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cubeToolStripMenuItem });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new Size(41, 22);
            addToolStripMenuItem.Text = "Add";
            // 
            // cubeToolStripMenuItem
            // 
            cubeToolStripMenuItem.Name = "cubeToolStripMenuItem";
            cubeToolStripMenuItem.Size = new Size(102, 22);
            cubeToolStripMenuItem.Text = "Cube";
            cubeToolStripMenuItem.Click += CubeToolStripMenuItem_Click;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { CameraToolStripMenuItem, WorldToolStripMenuItem });
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new Size(47, 22);
            resetToolStripMenuItem.Text = "Reset";
            // 
            // CameraToolStripMenuItem
            // 
            CameraToolStripMenuItem.Name = "CameraToolStripMenuItem";
            CameraToolStripMenuItem.Size = new Size(115, 22);
            CameraToolStripMenuItem.Text = "Camera";
            CameraToolStripMenuItem.Click += CameraToolStripMenuItem_Click;
            // 
            // WorldToolStripMenuItem
            // 
            WorldToolStripMenuItem.Name = "WorldToolStripMenuItem";
            WorldToolStripMenuItem.Size = new Size(115, 22);
            WorldToolStripMenuItem.Text = "World";
            WorldToolStripMenuItem.Click += WorldToolStripMenuItem_Click;
            // 
            // CameraSpeedSlider
            // 
            CameraSpeedSlider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CameraSpeedSlider.Location = new Point(0, 366);
            CameraSpeedSlider.Margin = new Padding(2, 1, 2, 1);
            CameraSpeedSlider.Maximum = 100;
            CameraSpeedSlider.Name = "CameraSpeedSlider";
            CameraSpeedSlider.Size = new Size(140, 45);
            CameraSpeedSlider.TabIndex = 5;
            CameraSpeedSlider.TickFrequency = 10;
            CameraSpeedSlider.Value = 8;
            CameraSpeedSlider.ValueChanged += CameraSpeedSlider_ValueChanged;
            // 
            // CamSpeedLabel
            // 
            CamSpeedLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CamSpeedLabel.AutoSize = true;
            CamSpeedLabel.Location = new Point(5, 350);
            CamSpeedLabel.Margin = new Padding(2, 0, 2, 0);
            CamSpeedLabel.Name = "CamSpeedLabel";
            CamSpeedLabel.Size = new Size(86, 15);
            CamSpeedLabel.TabIndex = 6;
            CamSpeedLabel.Text = "Camera Speed:";
            // 
            // CamSpeedUpDown
            // 
            CamSpeedUpDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            CamSpeedUpDown.Location = new Point(143, 366);
            CamSpeedUpDown.Margin = new Padding(2, 1, 2, 1);
            CamSpeedUpDown.Name = "CamSpeedUpDown";
            CamSpeedUpDown.Size = new Size(42, 23);
            CamSpeedUpDown.TabIndex = 7;
            CamSpeedUpDown.Value = new decimal(new int[] { 8, 0, 0, 0 });
            CamSpeedUpDown.ValueChanged += CamSpeedUpDown_ValueChanged;
            CamSpeedUpDown.KeyDown += CamSpeedUpDown_KeyDown;
            // 
            // ObjectList
            // 
            ObjectList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            ObjectList.FormattingEnabled = true;
            ObjectList.ItemHeight = 15;
            ObjectList.Location = new Point(6, 46);
            ObjectList.Margin = new Padding(2, 1, 2, 1);
            ObjectList.Name = "ObjectList";
            ObjectList.Size = new Size(181, 199);
            ObjectList.TabIndex = 8;
            ObjectList.SelectedIndexChanged += ObjectList_SelectedIndexChanged;
            ObjectList.MouseUp += ObjectList_MouseUp;
            // 
            // ContextMenuStrip
            // 
            ContextMenuStrip.ImageScalingSize = new Size(32, 32);
            ContextMenuStrip.Items.AddRange(new ToolStripItem[] { deleteToolStripMenuItem });
            ContextMenuStrip.Name = "contextMenuStrip1";
            ContextMenuStrip.Size = new Size(108, 26);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new Size(107, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
            // 
            // TransformationBox
            // 
            TransformationBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            TransformationBox.FormattingEnabled = true;
            TransformationBox.Items.AddRange(new object[] { "Translation", "Rotation", "Scale" });
            TransformationBox.Location = new Point(77, 258);
            TransformationBox.Margin = new Padding(2, 1, 2, 1);
            TransformationBox.Name = "TransformationBox";
            TransformationBox.Size = new Size(110, 23);
            TransformationBox.TabIndex = 9;
            TransformationBox.Text = "Translation";
            TransformationBox.SelectedIndexChanged += TransformationBox_SelectedIndexChanged;
            // 
            // LabelX
            // 
            LabelX.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            LabelX.AutoSize = true;
            LabelX.Location = new Point(53, 281);
            LabelX.Margin = new Padding(2, 0, 2, 0);
            LabelX.Name = "LabelX";
            LabelX.Size = new Size(17, 15);
            LabelX.TabIndex = 13;
            LabelX.Text = "X:";
            // 
            // LabelY
            // 
            LabelY.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            LabelY.AutoSize = true;
            LabelY.Location = new Point(53, 302);
            LabelY.Margin = new Padding(2, 0, 2, 0);
            LabelY.Name = "LabelY";
            LabelY.Size = new Size(17, 15);
            LabelY.TabIndex = 14;
            LabelY.Text = "Y:";
            // 
            // LabelZ
            // 
            LabelZ.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            LabelZ.AutoSize = true;
            LabelZ.Location = new Point(53, 323);
            LabelZ.Margin = new Padding(2, 0, 2, 0);
            LabelZ.Name = "LabelZ";
            LabelZ.Size = new Size(17, 15);
            LabelZ.TabIndex = 15;
            LabelZ.Text = "Z:";
            // 
            // LabelTransform
            // 
            LabelTransform.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            LabelTransform.AutoSize = true;
            LabelTransform.Location = new Point(3, 260);
            LabelTransform.Margin = new Padding(2, 0, 2, 0);
            LabelTransform.Name = "LabelTransform";
            LabelTransform.Size = new Size(63, 15);
            LabelTransform.TabIndex = 16;
            LabelTransform.Text = "Transform:";
            // 
            // UpDownX
            // 
            UpDownX.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            UpDownX.DecimalPlaces = 4;
            UpDownX.Location = new Point(77, 280);
            UpDownX.Margin = new Padding(2, 1, 2, 1);
            UpDownX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            UpDownX.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            UpDownX.Name = "UpDownX";
            UpDownX.Size = new Size(108, 23);
            UpDownX.TabIndex = 17;
            UpDownX.ValueChanged += UpDownX_ValueChanged;
            UpDownX.KeyDown += UpDownX_KeyDown;
            // 
            // UpDownY
            // 
            UpDownY.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            UpDownY.DecimalPlaces = 4;
            UpDownY.Location = new Point(77, 301);
            UpDownY.Margin = new Padding(2, 1, 2, 1);
            UpDownY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            UpDownY.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            UpDownY.Name = "UpDownY";
            UpDownY.Size = new Size(108, 23);
            UpDownY.TabIndex = 18;
            UpDownY.ValueChanged += UpDownY_ValueChanged;
            UpDownY.KeyDown += UpDownY_KeyDown;
            // 
            // UpDownZ
            // 
            UpDownZ.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            UpDownZ.DecimalPlaces = 4;
            UpDownZ.Location = new Point(77, 322);
            UpDownZ.Margin = new Padding(2, 1, 2, 1);
            UpDownZ.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            UpDownZ.Minimum = new decimal(new int[] { 10000, 0, 0, int.MinValue });
            UpDownZ.Name = "UpDownZ";
            UpDownZ.Size = new Size(108, 23);
            UpDownZ.TabIndex = 19;
            UpDownZ.ValueChanged += UpDownZ_ValueChanged;
            UpDownZ.KeyDown += UpDownZ_KeyDown;
            // 
            // LabelObjectList
            // 
            LabelObjectList.AutoSize = true;
            LabelObjectList.Location = new Point(5, 24);
            LabelObjectList.Margin = new Padding(2, 0, 2, 0);
            LabelObjectList.Name = "LabelObjectList";
            LabelObjectList.Size = new Size(66, 15);
            LabelObjectList.TabIndex = 20;
            LabelObjectList.Text = "Object List:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(255, 245, 245);
            ClientSize = new Size(740, 410);
            Controls.Add(LabelObjectList);
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
            Margin = new Padding(2, 1, 2, 1);
            MinimumSize = new Size(546, 302);
            Name = "MainForm";
            Text = "3DModeler";
            SizeChanged += MainForm_SizeChanged;
            Click += MainForm_Click;
            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
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
        private Label LabelObjectList;
    }
}