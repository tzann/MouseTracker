using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using SharpDX.DirectInput;

namespace MouseTracker
{
    public partial class MouseTrackerForm : Form
    {
        private List<Point> points;
        private double lineLength;
        private DirectInput directInput;
        private Mouse mouse;
        Point pos = new Point(-100, -100);

        private System.Threading.Timer pollTimer;
        private System.Threading.Timer refreshTimer;

        private Camera camera;
        private int cameraX = 0;
        private int cameraY = 0;

        private bool isRetractingLine = false;
        private int pollsSinceLastMovement = 0;

        // how many times the mouse input is read per second
        // shouldn't really impact performance too much (I hope)
        private static int pollingRate = 1000;

        // how many frames are drawn per second
        // for performance reasons, this should probably not be higher than whatever framerate you plan to record at
        private static int refreshRate = 240;

        private static double scale = 0.5;

        // how many points are in the line before it starts removing them
        private static int numPoints = 50;
        
        // mouse deceleration for testing purposes
        private static bool useMouseDecel = false;

        // choose whether the camera follows the "center of mass" of the line, or the cursor
        private static bool cameraTargetsCursor = true;

        public MouseTrackerForm()
        {
            InitializeComponent();
            InitializeInput();

            SetupVariables();

            SetupPollingTimer();
            SetupRefreshTimer();
        }

        private void SetupVariables()
        {
            camera = new Camera(graphicsPanel.ClientSize.Width, graphicsPanel.ClientSize.Height);
            points = new List<Point>();
            lineLength = 0;
        }

        private void InitializeInput()
        {
            directInput = new DirectInput();

            Guid guid = Guid.Empty;
            foreach (DeviceInstance device in directInput.GetDevices(DeviceType.Mouse, DeviceEnumerationFlags.AllDevices))
            {
                guid = device.InstanceGuid;
            }
            if (guid == Guid.Empty)
            {
                throw new Exception("No mouse found");
            }

            mouse = new Mouse(directInput);
            mouse.Acquire();
        }

        private void SetupPollingTimer()
        {
            TimerCallback callback = delegate
            {
                PollMouse();
            };
            pollTimer = new System.Threading.Timer(callback, null, 1000, 1000 / pollingRate);
        }

        private void SetupRefreshTimer()
        {
            TimerCallback callback = delegate
            {
                RefreshScreen();
            };
            refreshTimer = new System.Threading.Timer(callback, null, 1000, 1000 / refreshRate);
        }

        private void PollMouse()
        {
            if (mouse == null)
            {
                return;
            }

            MouseState currentState = mouse.GetCurrentState();
            if (currentState.X != 0 || currentState.Y != 0)
            {
                pollsSinceLastMovement = 0;
                isRetractingLine = false;
                
                int dx = currentState.X;
                int dy = currentState.Y;
                // TODO: mouse accel and decel here

                pos = new Point(pos.X + dx, pos.Y + dy);

                AddPoint(pos);
            }
            else
            {
                // Mouse didn't move
                pollsSinceLastMovement++;
                if (pollsSinceLastMovement + points.Count > numPoints)
                {
                    isRetractingLine = true;
                }
                if (isRetractingLine)
                {
                    if (points.Count > 0)
                    {
                        RemovePoint();
                    }
                    else
                    {
                        // Line has fully retracted
                        isRetractingLine = false;
                        ResetCameraPosAndScale();
                    }
                }
            }
        }

        private void RefreshScreen()
        {
            graphicsPanel.Invalidate();
        }

        private void AddPoint(Point p)
        {
            if (points.Count > 0)
            {
                Point last = points[points.Count - 1];
                lineLength += Distance(last, p);
            }

            points.Add(p);
            while (points.Count > numPoints)
            {
                RemovePoint();
            }
        }

        private void RemovePoint()
        {
            if (points.Count > 1)
            {
                Point toRemove = points[0];
                Point next = points[1];
                lineLength -= Distance(toRemove, next);
            }
            points.RemoveAt(0);
        }

        private double GetFurthestDistanceFromCursor()
        {
            double max = 0;
            for (int i = 0; i < points.Count; i++)
            {
                max = Math.Max(max, Distance(pos, points[i]));
            }
            return max;
        }

        private double GetFurthestDistanceFromCamera()
        {
            double max = 0;
            for (int i = 0; i < points.Count; i++)
            {
                max = Math.Max(max, Distance(new Point(cameraX, cameraY), points[i]));
            }
            return max;
        }

        private Point GetCenterOfLine()
        {
            int minX = pos.X;
            int minY = pos.Y;
            int maxX = pos.X;
            int maxY = pos.Y;

            for (int i = 0; i < points.Count; i++)
            {
                minX = Math.Min(points[i].X, minX);
                minY = Math.Min(points[i].Y, minY);
                maxX = Math.Max(points[i].X, maxX);
                maxY = Math.Max(points[i].Y, maxY);
            }

            return new Point((minX + maxX) / 2, (minY + maxY) / 2);
        }

        private double GetTargetScale()
        {
            if (!cameraTargetsCursor)
            {
                return Math.Max(0.5, 2d * GetFurthestDistanceFromCamera() / graphicsPanel.ClientSize.Width);
            }
            return Math.Max(0.5, 2d * GetFurthestDistanceFromCursor() / graphicsPanel.ClientSize.Width);
        }

        private Point GetTargetPos()
        {
            if (!cameraTargetsCursor)
            {
                return GetCenterOfLine();
            }
            return pos;
        }

        private void ResetCameraPosAndScale()
        {
            double targetScale = GetTargetScale();
            Point targetPos = GetTargetPos();

            cameraX = targetPos.X;
            cameraY = targetPos.Y;
            scale = targetScale;
        }

        private void AdjustCameraPosAndScale()
        {
            double targetScale = GetTargetScale();
            Point targetPos = GetTargetPos();

            cameraX = (29 * cameraX + targetPos.X) / 30;
            cameraY = (29 * cameraY + targetPos.Y) / 30;
            if (!isRetractingLine)
            {
                scale = (19 * scale + targetScale) / 20;
            }
        }

        // Function to help fix dynamic zoom thresholds, not currently being used
        private double roundToPow2(double val)
        {
            double low = 0.5;
            double next = 1.0;
            while (next < val)
            {
                low *= 2;
                next *= 2;
            }

            if (val - low < next - val)
            {
                return low;
            }

            return next;
        }

        private void GraphicsPanel_Paint(object sender, PaintEventArgs e)
        {
            AdjustCameraPosAndScale();
            Point[] graphicalPoints = ToCameraCoordinates(points.ToArray());

            Point origin = ToCameraCoordinates(new Point(0, 0));
            Bitmap bmp = camera.render(graphicalPoints, scale, origin.X, origin.Y);

            e.Graphics.DrawImage(bmp, 0, 0, graphicsPanel.ClientSize.Width, graphicsPanel.ClientSize.Height);
        }

        private Point[] ToCameraCoordinates(Point[] ps)
        {
            Point[] result = new Point[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                result[i] = ToCameraCoordinates(ps[i]);
            }
            return result;
        }

        private Point ToCameraCoordinates(Point p)
        {
            return new Point((int) ((p.X - cameraX) / scale) + camera.width / 2, (int) ((p.Y - cameraY) / scale) + camera.height / 2);
        }

        private double Distance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
