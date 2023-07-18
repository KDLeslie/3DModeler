﻿namespace _3DModeler
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
            ((System.ComponentModel.ISupportInitialize)Viewer).BeginInit();
            SuspendLayout();
            // 
            // Viewer
            // 
            Viewer.Location = new Point(262, 12);
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
            FPS.Location = new Point(30, 72);
            FPS.Name = "FPS";
            FPS.Size = new Size(57, 32);
            FPS.TabIndex = 1;
            FPS.Text = "FPS:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(42, 163);
            label1.Name = "label1";
            label1.Size = new Size(78, 32);
            label1.TabIndex = 2;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(42, 224);
            label2.Name = "label2";
            label2.Size = new Size(78, 32);
            label2.TabIndex = 3;
            label2.Text = "label2";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1274, 929);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(FPS);
            Controls.Add(Viewer);
            Name = "Form1";
            Text = "3DModeler";
            Load += Form1_Load;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            ((System.ComponentModel.ISupportInitialize)Viewer).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox Viewer;
        private System.Windows.Forms.Timer Clock;
        private Label FPS;
        private Label label1;
        private Label label2;
    }
}