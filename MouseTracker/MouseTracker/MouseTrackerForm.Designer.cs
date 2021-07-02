
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
            this.GraphicsPanel = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.GraphicsPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // GraphicsPanel
            // 
            this.GraphicsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphicsPanel.Location = new System.Drawing.Point(0, 0);
            this.GraphicsPanel.Name = "GraphicsPanel";
            this.GraphicsPanel.Size = new System.Drawing.Size(984, 961);
            this.GraphicsPanel.TabIndex = 0;
            this.GraphicsPanel.TabStop = false;
            this.GraphicsPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.GraphicsPanel_Paint);
            // 
            // MouseTrackerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 961);
            this.Controls.Add(this.GraphicsPanel);
            this.Name = "MouseTrackerForm";
            this.Text = "Mouse Tracker";
            ((System.ComponentModel.ISupportInitialize)(this.GraphicsPanel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox GraphicsPanel;
    }
}

