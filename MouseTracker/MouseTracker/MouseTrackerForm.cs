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
        // how many frames are drawn per second
        // for performance reasons, this should probably not be higher than whatever framerate you plan to record at
        private static int refreshRate = 60;

        // how many milliseconds "long" the line can get before it starts retracting
        // also dictates how long it takes to start retracting the line when idle
        private static int maxLineLength = 1000;
        // maxNumPoints is calculated automatically
        // TODO: calculate this better... it's based on the fact that windows doesn't let you sleep for less than ~15ms
        private static int maxNumPoints = maxLineLength * 60 / 1000;

        // what the maximum zoom level is
        // smaller = more zoomed in
        private static double minScale = 0.5;

        // choose whether the camera follows the "center of mass" of the line, or the cursor
        private static bool cameraTargetsCursor = true;
        // how quickly the camera follows the cursor (tension of rubber band)
        private static int cameraFollowSpeed = 30;
        // how quickly the camera zooms to fit stuff on screen
        private static int cameraZoomSpeed = 30;

        // ----------------------------------------------------------

        private List<Point> points;
        private double lineLength;
        private DirectInput directInput;
        private Mouse mouse;
        Point pos;

        private System.Threading.Timer pollTimer;
        private System.Threading.Timer refreshTimer;

        private Camera camera;
        private int cameraX;
        private int cameraY;
        private double scale;

        private long lastPollTime;
        private long timeSinceLastMovement;

        private State currentState;

        private enum State
        {
            Reset,
            Moving,
            Idle,
            Retracting,
        }

        public MouseTrackerForm()
        {
            InitializeComponent();
            InitializeMouseInput();

            SetupVariables();

            SetupPollingTimer();
            SetupRefreshTimer();
        }

        private void SetupVariables()
        {
            pos = new Point(0, 0);
            camera = new Camera(graphicsPanel.ClientSize.Width, graphicsPanel.ClientSize.Height);
            cameraX = 0;
            cameraY = 0;
            points = new List<Point>();
            lineLength = 0;
            scale = minScale;
            currentState = State.Reset;
        }

        private void InitializeMouseInput()
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
            // 1ms interval is the lowest we can set
            // in reality windows doesn't allow you to trigger stuff faster than ~60hz
            pollTimer = new System.Threading.Timer(callback, null, 1000, 1);
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

            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long dt = currentTime - lastPollTime;
            lastPollTime = currentTime;

            MouseState mouseState = mouse.GetCurrentState();
            int dx = mouseState.X;
            int dy = mouseState.Y;

            DoStuff(dx, dy, dt);

            AdjustCameraPosAndScale();
        }

        private void DoStuff(int dx, int dy, long dt)
        {
            if (dx != 0 || dy != 0)
            {
                timeSinceLastMovement = 0;
                currentState = State.Moving;

                // TODO: mouse accel / decel
                pos = new Point(pos.X + dx, pos.Y + dy);

                AddPoint(pos);
            }
            else
            {
                // Mouse didn't move
                timeSinceLastMovement += dt;
                if (currentState == State.Moving)
                {
                    currentState = State.Idle;
                }
                else if (currentState == State.Idle)
                {
                    int timeThreshold = maxLineLength * (maxNumPoints - points.Count) / maxNumPoints;
                    if (timeSinceLastMovement > timeThreshold)
                    {
                        currentState = State.Retracting;
                    }
                }
                else if (currentState == State.Retracting)
                {
                    if (points.Count > 0)
                    {
                        RemovePoint();
                    }
                    else
                    {
                        // Line has fully retracted
                        currentState = State.Reset;
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
            while (points.Count > maxNumPoints)
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
            double targetWidth;
            if (!cameraTargetsCursor)
            {
                targetWidth = GetFurthestDistanceFromCamera();
            } else
            {
                targetWidth = GetFurthestDistanceFromCursor();
            }

            double targetScale = 2d * targetWidth / graphicsPanel.ClientSize.Width;
            return Math.Max(minScale, targetScale);
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

            // Pan camera to target position
            cameraX = ((cameraFollowSpeed - 1) * cameraX + targetPos.X) / cameraFollowSpeed;
            cameraY = ((cameraFollowSpeed - 1) * cameraY + targetPos.Y) / cameraFollowSpeed;

            // Only zoom if not retracting
            if (currentState != State.Retracting)
            {
                scale = ((cameraZoomSpeed - 1) * scale + targetScale) / cameraZoomSpeed;
            }
        }

        private void GraphicsPanel_Paint(object sender, PaintEventArgs e)
        {
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
