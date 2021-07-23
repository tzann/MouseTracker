
namespace MouseTracker
{
    partial class MouseTrackerForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.graphicsPanel = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.graphicsPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // graphicsPanel
            // 
            this.graphicsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphicsPanel.Location = new System.Drawing.Point(0, 0);
            this.graphicsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.graphicsPanel.Name = "graphicsPanel";
            this.graphicsPanel.Size = new System.Drawing.Size(984, 961);
            this.graphicsPanel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.graphicsPanel.TabIndex = 0;
            this.graphicsPanel.TabStop = false;
            this.graphicsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphicsPanel_Paint);
            // 
            // MouseTrackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 961);
            this.Controls.Add(this.graphicsPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MouseTrackerForm";
            this.Text = "Mouse Tracker";
            ((System.ComponentModel.ISupportInitialize)(this.graphicsPanel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox graphicsPanel;
    }
}

