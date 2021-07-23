using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTracker
{
    public class Camera
    {
        public int width;
        public int height;

        private Bitmap bmp;
        private Graphics graphics;
        private bool isResizing;

        private Color backgroundColor = Color.FromArgb(0, 255, 0);

        // Pen used to draw grid lines
        private Pen gridPen;
        private Color gridColor = Color.Blue;
        private int gridWidth = 3;

        private Pen thickGridPen;
        private int thickGridWidth = 9;

        // Pen used to draw the line
        private Pen linePen;
        private Color lineColor = Color.White;
        private int lineWidth = 15;

        // Pen used to draw outline
        private bool drawOutline = false;
        private Pen outlinePen;
        private int outlineWidth = 10;
        private Color outlineColor = Color.FromArgb(255, 0, 0);

        // Cursor stuff
        private bool drawCursor = true;
        private Brush cursorBrush;
        private int cursorSize = 30;
        private Color cursorColor = Color.FromArgb(255, 0, 0);

        // Speed color interpolation test
        private bool speedColorInterpolation = false;
        private Color slowColor = Color.Blue;
        private Color fastColor = Color.Red;
        // Speeds below minSpeed will be slowColor
        // Speeds above maxSpeed will be fastColor
        // Everything in between will be interpolated linearly
        // TODO: implement some kind of non-linear interpolation
        // More specifically, the difference between 10 and 20 should be the same apparent as the difference between 100 and 200
        private int minSpeed = 0;
        private int maxSpeed = 200;

        // More Drawing options
        
        // Grid stuff
        private bool drawGridLines = false;
        // Distance between grid lines in pixels (at smallest scale)
        private int gridSize = 100;
        // The factor by which the grid size is reduced when zooming out
        // Also, every nth grid line will be drawn thicker (n = gridFactor)
        private int gridFactor = 3;
        // If the grid squares become smaller than this value, the grid will increase in size (by gridFactor)
        private int minGridSize = 100;

        // Choose how the line is drawn
        private LineType lineType = LineType.Simple;
        public enum LineType
        {
            // Draw a straight line between each pair of consecutive points
            Simple,
            // Draw one single curve through all points (causes visual artifacts)
            Smooth,
            // Draw a curve through each triplet of points (causes visual artifacts)
            Smooth2
        }

        // Draw black rectangles on the actual point positions
        private bool drawIndividualPoints = false;

        public Camera(int width, int height)
        {
            isResizing = false;
            this.width = width;
            this.height = height;
            SetupGraphics();
        }

        private void resize(int width, int height)
        {
            isResizing = true;
            DisposeGraphics();
            this.width = width;
            this.height = height;
            SetupGraphics();
            isResizing = false;
        }

        private void SetupGraphics()
        {
            bmp = new Bitmap(width, height);
            graphics = Graphics.FromImage(bmp);
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            SetupPens();
        }

        private void DisposeGraphics()
        {
            DisposePens();
            graphics.Dispose();
            bmp.Dispose();
        }

        private void SetupPens()
        {
            gridPen = new Pen(gridColor, gridWidth);
            thickGridPen = new Pen(gridColor, thickGridWidth);

            linePen = new Pen(lineColor, lineWidth);
            linePen.StartCap = LineCap.Round;
            linePen.EndCap = LineCap.Round;

            outlinePen = new Pen(outlineColor, outlineWidth);

            cursorBrush = new SolidBrush(cursorColor);
        }

        private void DisposePens()
        {
            gridPen.Dispose();
            thickGridPen.Dispose();
            linePen.Dispose();
            outlinePen.Dispose();
            cursorBrush.Dispose();
        }

        public Bitmap render(Point[] points, double sizeMultiplier, int originX, int originY)
        {
            if (isResizing)
            {
                return new Bitmap(0, 0);
            }

            double gridSize = this.gridSize / sizeMultiplier;

            int cursorSize = (int)(this.cursorSize / Math.Sqrt(sizeMultiplier));
            int lineWidth = (int)(this.lineWidth / Math.Sqrt(sizeMultiplier));
            linePen.Width = lineWidth;

            // Clear canvas, draw background
            graphics.Clear(backgroundColor);

            // Draw grid lines
            if (drawGridLines)
            {
                renderGridLines(graphics, originX, originY, sizeMultiplier, gridSize);
            }

            // Only draw line if there are at least two points
            if (points.Length > 1)
            {
                renderLine(graphics, points, linePen, sizeMultiplier);
            }

            // Draw individual points
            if (drawIndividualPoints)
            {
                renderPoints(graphics, points);
            }

            // Only render cursor if there is at least one point
            if (drawCursor && points.Length > 0)
            {
                renderCursor(graphics, points[points.Length - 1], cursorSize);
            }

            // Draw outline over everything else
            if (drawOutline)
            {
                graphics.DrawRectangle(outlinePen, 0, 0, width - 1, height - 1);
            }

            return bmp;
        }

        /*
         * It would be nice if we could draw curves between points that are further apart
         * Especially with lower polling rates, individual line segments don't look great.
         * However, when points are too close together, DrawCurve actually has visual
         * artifacts, which are really annoying. It may be possible to cull some points
         * that are close together but don't influence the overall shape of the line, but
         * I haven't thought about it too much yet.
         */
        private void renderLine(Graphics graphics, Point[] points, Pen linePen, double sizeMultiplier)
        {
            if (lineType == LineType.Smooth)
            {
                // This has visual artifacts when points get too close together...
                graphics.DrawCurve(linePen, points);
            }
            else if (lineType == LineType.Smooth2)
            {
                // This has visual artifacts when points get too close together...
                Point prevprev = points[0];
                Point prev = points[1];
                for (int i = 2; i < points.Length; i++)
                {
                    Point curr = points[i];
                    Point[] ps = new Point[] { prevprev, prev, curr };
                    graphics.DrawCurve(linePen, ps);
                    prevprev = prev;
                    prev = curr;
                }
            }
            else if (lineType == LineType.Simple)
            {
                renderSegmentedLine(graphics, points, linePen, sizeMultiplier);
            }
        }

        private void renderSegmentedLine(Graphics graphics, Point[] points, Pen linePen, double sizeMultiplier)
        {
            Point prev = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                Point curr = points[i];

                if (speedColorInterpolation)
                {
                    int dx = curr.X - prev.X;
                    int dy = curr.Y - prev.Y;
                    double lineLength = Math.Sqrt(dx * dx + dy * dy) * sizeMultiplier;

                    linePen.Color = getInterpolatedSpeedColor(lineLength);
                }

                graphics.DrawLine(linePen, prev, curr);
                prev = curr;
            }

            // reset line color
            linePen.Color = lineColor;
        }

        private Color getInterpolatedSpeedColor(double lineLength)
        {
            if (lineLength <= minSpeed)
            {
                return slowColor;
            } else if (lineLength >= maxSpeed)
            {
                return fastColor;
            }

            lineLength -= minSpeed;
            double interpolationRange = maxSpeed - minSpeed;
            double interpolationRatio = lineLength / interpolationRange;

            int dR = fastColor.R - slowColor.R;
            int dG = fastColor.G - slowColor.G;
            int dB = fastColor.B - slowColor.B;

            int r = slowColor.R + (int)(interpolationRatio * dR);
            int g = slowColor.G + (int)(interpolationRatio * dG);
            int b = slowColor.B + (int)(interpolationRatio * dB);

            return Color.FromArgb(r, g, b);
        }

        private void renderGridLines(Graphics graphics, int originX, int originY, double sizeMultiplier, double gridSize)
        {
            while (gridSize < minGridSize)
            {
                gridSize *= gridFactor;
            }

            double gridXOffset = originX % gridSize;
            int numXGridLinesOff = (int) ((originX - gridXOffset) / gridSize);

            double gridYOffset = originY % gridSize;
            int numYGridLinesOff = (int) ((originY - gridYOffset) / gridSize);

            for (double x = gridXOffset; x <= width; x += gridSize)
            {
                if (numXGridLinesOff % gridFactor == 0)
                {
                    graphics.DrawLine(thickGridPen, (int)x, 0, (int)x, height);
                } else
                {
                    graphics.DrawLine(gridPen, (int)x, 0, (int)x, height);
                }
                numXGridLinesOff--;
            }

            for (double y = gridYOffset; y <= height; y += gridSize)
            {
                if (numYGridLinesOff % gridFactor == 0)
                {
                    graphics.DrawLine(thickGridPen, 0, (int)y, width, (int)y);
                } else
                {
                    graphics.DrawLine(gridPen, 0, (int)y, width, (int)y);
                }
                numYGridLinesOff--;
            }
        }

        private void renderPoints(Graphics graphics, Point[] points)
        {
            foreach (Point p in points)
            {
                Point adjusted = new Point(p.X - 2, p.Y - 2);
                Rectangle rect = new Rectangle(adjusted, new Size(5, 5));
                graphics.FillRectangle(Brushes.Black, rect);
            }
        }

        private void renderCursor(Graphics graphics, Point cursor, int cursorSize)
        {
            Point adjusted = new Point(cursor.X - cursorSize / 2, cursor.Y - cursorSize / 2);
            Rectangle rect = new Rectangle(adjusted, new Size(cursorSize, cursorSize));
            graphics.FillEllipse(cursorBrush, rect);
        }

        // -------------------
        // Setters for options
        // -------------------

        public void setSize(int width, int height)
        {
            resize(width, height);
        }

        public void setBackgroundColor(Color color)
        {
            backgroundColor = color;
        }

        public void setLineColor(Color color)
        {
            lineColor = color;
            linePen.Color = lineColor;
        }

        public void setLineWidth(int width)
        {
            lineWidth = width;
            linePen.Width = lineWidth;
        }

        public void setLineType(LineType type)
        {
            lineType = type;
        }

        public void setGridEnabled(bool enabled)
        {
            drawGridLines = enabled;
        }

        public void setGridColor(Color color)
        {
            gridColor = color;
            gridPen.Color = gridColor;
            thickGridPen.Color = gridColor;
        }

        public void setGridLineWidth(int width)
        {
            gridWidth = width;
            gridPen.Width = width;
        }

        public void setGridThickLineWidth(int width)
        {
            thickGridWidth = width;
            thickGridPen.Width = thickGridWidth;
        }

        public void setGridFactor(int factor)
        {
            gridFactor = factor;
        }

        public void setGridSize(int size)
        {
            gridSize = size;
        }

        public void setMinGridSize(int size)
        {
            minGridSize = size;
        }

        public void setCursorEnabled(bool enabled)
        {
            drawCursor = enabled;
        }

        public void setCursorColor(Color color)
        {
            cursorColor = color;
            cursorBrush.Dispose();
            cursorBrush = new SolidBrush(cursorColor);
        }

        public void setCursorSize(int size)
        {
            cursorSize = size;
        }

        public void setOutlineEnabled(bool enabled)
        {
            drawOutline = enabled;
        }

        public void setOutlineColor(Color color)
        {
            outlineColor = color;
            outlinePen.Color = outlineColor;
        }

        public void setOutlineWidth(int width)
        {
            outlineWidth = width * 2;
            outlinePen.Width = outlineWidth;
        }

        public void setPointsEnabled(bool enabled)
        {
            drawIndividualPoints = enabled;
        }

        public void setSpeedColorInterpolationEnabled(bool enabled)
        {
            speedColorInterpolation = enabled;
        }

        public void setSlowColor(Color color)
        {
            slowColor = color;
        }

        public void setFastColor(Color color)
        {
            fastColor = color;
        }

        public void setMinSpeed(int speed)
        {
            minSpeed = speed;
        }

        public void setMaxSpeed(int speed)
        {
            maxSpeed = speed;
        }
    }
}
