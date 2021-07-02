using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseTracker
{
    class Camera
    {
        public int width;
        public int height;
        private Bitmap bmp;
        private Graphics graphics;
        private Pen linePen;
        private Color backgroundColor = Color.FromArgb(0, 255, 0);
        private Color lineColor = Color.White;

        private int lineWidth = 10;
        private bool drawCursor = true;
        private int cursorSize = 20;

        private bool drawOneCurve = false;
        private bool drawMultipleCurves = false;
        private bool drawLines = true;

        public Camera(int width, int height)
        {
            this.width = width;
            this.height = height;
            bmp = new Bitmap(width, height);
            graphics = Graphics.FromImage(bmp);
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            SetupLinePen();
        }

        private void SetupLinePen()
        {
            linePen = new Pen(lineColor, lineWidth);
            linePen.StartCap = LineCap.Round;
            linePen.EndCap = LineCap.Round;
        }

        public Bitmap render(Point[] points, double sizeMultiplier)
        {

            int cursorSize = (int)(this.cursorSize / Math.Sqrt(sizeMultiplier));
            int lineWidth = (int)(this.lineWidth / Math.Sqrt(sizeMultiplier));
            linePen.Width = lineWidth;

            graphics.Clear(backgroundColor);

            if (points.Length > 1)
            {
                if (drawOneCurve)
                {
                    graphics.DrawCurve(linePen, points);
                }
                else if (drawMultipleCurves)
                {
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
                else if (drawLines)
                {
                    Point prev = points[0];
                    for (int i = 1; i < points.Length; i++)
                    {
                        Point curr = points[i];
                        graphics.DrawLine(linePen, prev, curr);
                        prev = curr;
                    }
                }
            }

            foreach (Point p in points)
            {
                Rectangle rect = new Rectangle(p, new Size(5, 5));
                // graphics.FillRectangle(Brushes.Black, rect);
            }

            if (drawCursor && points.Length > 0)
            {
                Point curr = points[points.Length - 1];
                Point adjusted = new Point(curr.X - cursorSize / 2, curr.Y - cursorSize / 2);
                Rectangle rect = new Rectangle(adjusted, new Size(cursorSize, cursorSize));
                graphics.FillEllipse(Brushes.Red, rect);
            }

            return bmp;
        }

    }
}
