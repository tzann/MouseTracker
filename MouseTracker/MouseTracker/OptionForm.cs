using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseTracker
{
    public partial class OptionForm : Form
    {
        private MouseTrackerForm mouseTracker;

        public OptionForm()
        {
            InitializeComponent();
            
            lineTypeComboBox.DataSource = Enum.GetValues(typeof(Camera.LineType));
            lineTypeComboBox.SelectedItem = Camera.LineType.Simple;

            followTypeComboBox.DataSource = Enum.GetValues(typeof(MouseTrackerForm.CameraFollowType));
            followTypeComboBox.SelectedItem = MouseTrackerForm.CameraFollowType.Cursor;

            mouseTracker = new MouseTrackerForm();
            mouseTracker.Show();
        }

        private void gridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = gridCheckBox.Checked;
            gridOptionBox.Enabled = enabled;

            mouseTracker.GetCamera().setGridEnabled(enabled);
        }

        private void outlineCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = outlineCheckBox.Checked;
            outlineOptionBox.Enabled = enabled;

            mouseTracker.GetCamera().setOutlineEnabled(enabled);
        }

        private void cursorCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = cursorCheckBox.Checked;
            cursorOptionBox.Enabled = enabled;

            mouseTracker.GetCamera().setCursorEnabled(enabled);
        }

        private void backgroundColor_Changed(object sender, EventArgs e)
        {
            int red = (int)backgroundColorRed.Value;
            int green = (int)backgroundColorGreen.Value;
            int blue = (int)backgroundColorBlue.Value;

            Color color = Color.FromArgb(red, green, blue);

            mouseTracker.GetCamera().setBackgroundColor(color);
        }

        private void lineColor_Changed(object sender, EventArgs e)
        {
            int red = (int) lineColorRed.Value;
            int green = (int) lineColorGreen.Value;
            int blue = (int) lineColorBlue.Value;

            Color color = Color.FromArgb(red, green, blue);

            mouseTracker.GetCamera().setLineColor(color);
        }

        private void gridColor_Changed(object sender, EventArgs e)
        {
            int red = (int)gridColorRed.Value;
            int green = (int)gridColorGreen.Value;
            int blue = (int)gridColorBlue.Value;

            Color color = Color.FromArgb(red, green, blue);

            mouseTracker.GetCamera().setGridColor(color);
        }

        private void cursorColor_Changed(object sender, EventArgs e)
        {
            int red = (int)cursorColorRed.Value;
            int green = (int)cursorColorGreen.Value;
            int blue = (int)cursorColorBlue.Value;

            Color color = Color.FromArgb(red, green, blue);

            mouseTracker.GetCamera().setCursorColor(color);
        }

        private void outlineColor_Changed(object sender, EventArgs e)
        {
            int red = (int)outlineColorRed.Value;
            int green = (int)outlineColorGreen.Value;
            int blue = (int)outlineColorBlue.Value;

            Color color = Color.FromArgb(red, green, blue);

            mouseTracker.GetCamera().setOutlineColor(color);
        }

        private void drawPointsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = drawPointsCheckBox.Checked;

            mouseTracker.GetCamera().setPointsEnabled(enabled);
        }

        private void gridLineWidth_ValueChanged(object sender, EventArgs e)
        {
            int width = (int) gridLineWidth.Value;
            mouseTracker.GetCamera().setGridLineWidth(width);
        }

        private void lineTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mouseTracker != null)
            {
                Camera.LineType type = (Camera.LineType)lineTypeComboBox.SelectedItem;
                mouseTracker.GetCamera().setLineType(type);
            }
        }

        private void gridDistance_ValueChanged(object sender, EventArgs e)
        {
            int size = (int)gridDistance.Value;

            mouseTracker.GetCamera().setGridSize(size);
        }

        private void cursorSize_ValueChanged(object sender, EventArgs e)
        {
            int size = (int)cursorSize.Value;

            mouseTracker.GetCamera().setCursorSize(size);
        }

        private void gridMinSize_ValueChanged(object sender, EventArgs e)
        {
            int size = (int)gridMinSize.Value;

            mouseTracker.GetCamera().setMinGridSize(size);
        }

        private void gridThickLineWidth_ValueChanged(object sender, EventArgs e)
        {
            int width = (int)gridThickLineWidth.Value;

            mouseTracker.GetCamera().setGridThickLineWidth(width);
        }

        private void gridFactor_ValueChanged(object sender, EventArgs e)
        {
            int factor = (int)gridFactor.Value;

            mouseTracker.GetCamera().setGridFactor(factor);
        }

        private void outlineWidth_ValueChanged(object sender, EventArgs e)
        {
            int width = (int)outlineWidth.Value;

            mouseTracker.GetCamera().setOutlineWidth(width);
        }

        private void uncappedFpsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = uncappedFpsCheckBox.Checked;

            maxFpsLabel.Enabled = !enabled;
            maxFps.Enabled = !enabled;

            if (enabled)
            {
                mouseTracker.setRefreshRate(240);
            } else
            {
                int fps = (int) maxFps.Value;
                mouseTracker.setRefreshRate(fps);
            }
        }

        private void maxFps_ValueChanged(object sender, EventArgs e)
        {
            int fps = (int)maxFps.Value;
            mouseTracker.setRefreshRate(fps);
        }

        private void maxLineLength_ValueChanged(object sender, EventArgs e)
        {
            int length = (int)maxLineLength.Value;
            mouseTracker.setMaxLineLength(length);
        }

        private void maxZoomLevel_ValueChanged(object sender, EventArgs e)
        {
            double zoomLevel = (double)maxZoomLevel.Value;
            double scale = 1d / zoomLevel;

            mouseTracker.setMinScale(scale);
        }

        private void followTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (mouseTracker != null)
            {
                MouseTrackerForm.CameraFollowType type = (MouseTrackerForm.CameraFollowType)followTypeComboBox.SelectedItem;
                mouseTracker.setCameraFollowType(type);
            }
        }

        private void cameraFollowSpeed_ValueChanged(object sender, EventArgs e)
        {
            int speed = (int) cameraFollowSpeed.Value;
            mouseTracker.setCameraFollowSpeed(speed);
        }

        private void cameraZoomSpeed_ValueChanged(object sender, EventArgs e)
        {
            int speed = (int)cameraZoomSpeed.Value;
            mouseTracker.setCameraZoomSpeed(speed);
        }

        private void lineWidth_ValueChanged(object sender, EventArgs e)
        {
            int width = (int)lineWidth.Value;
            mouseTracker.GetCamera().setLineWidth(width);
        }
    }
}
