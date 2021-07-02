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

        private bool resetPoints = false;
        private bool isRetractingLine = false;
        private int pollsSinceLastMovement = 0;

        private static int pollingRate = 1000;
        private static int refreshRate = 240;
        private static double scale = 5;
        private static double targetScale = 5;
        private static int numPoints = 50;
        private static bool useMouseDecel = false;

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
            camera = new Camera(Width, Height);
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
                if (useMouseDecel)
                {
                    if (currentState.X < 0)
                    {
                        dx = (int) -Math.Sqrt(-dx);
                    }
                    else
                    {
                        dx = (int) Math.Sqrt(dx);
                    }
                    if (currentState.Y < 0)
                    {
                        dy = (int) -Math.Sqrt(-dy);
                    }
                    else
                    {
                        dy = (int) Math.Sqrt(dy);
                    }
                }

                pos = new Point(pos.X + dx, pos.Y + dy);

                AddPoint(pos);
            }
            else
            {
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
                        isRetractingLine = false;
                    }
                }
            }
        }

        private void RefreshScreen()
        {
            GraphicsPanel.Invalidate();
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

        private void AdjustCameraPosAndScale()
        {
            if (lineLength > 125)
            {
                targetScale = roundToPow2(2d * lineLength / Width);
            } else
            {
                targetScale = 0.5;
            }

            cameraX = (29 * cameraX + pos.X) / 30;
            cameraY = (29 * cameraY + pos.Y) / 30;
            if (!isRetractingLine)
            {
                scale = (19 * scale + targetScale) / 20;
            }
        }

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

            Bitmap bmp = camera.render(graphicalPoints, scale);

            e.Graphics.DrawImage(bmp, 0, 0, Width, Height);
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
