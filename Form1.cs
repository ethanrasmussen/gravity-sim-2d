namespace gravity_simple;

using Timer = System.Windows.Forms.Timer;

public partial class Form1 : Form
{

    List<CelestialBody> bodies = new List<CelestialBody>();
    const float G = 6.674e-1f;   // gravitational force
    const int TrailMax = 100;     // max trail length
    Timer timer = new Timer { Interval = 16 }; // ~60 FPS

    private bool isDragging = false;
    private PointF dragStart = PointF.Empty;
    private PointF currentMouse = PointF.Empty;

    public Form1()
    {
        InitializeComponent();

        // enable double buffering:
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
        UpdateStyles();

        // physics update loop in winform
        InitBodies();
        timer.Tick += (s, e) => { UpdatePhysics(); Invalidate(); };

    }

    // initialize celestial bodies
    void InitBodies()
    {
        var center = new PointF(ClientSize.Width / 2, ClientSize.Height / 2);

        // large central body -- 'sun'
        bodies.Add(new CelestialBody(
            pos: center,
            vel: PointF.Empty,
            mass: 2000,
            radius: 20,
            color: Color.OrangeRed
        ));

        // smaller orbiting body -- 'planet'
        float orbitR = 150;
        float speed = (float)Math.Sqrt(G * bodies[0].Mass / orbitR);
        bodies.Add(new CelestialBody(
            pos: new PointF(center.X + orbitR, center.Y),
            vel: new PointF(0, speed),
            mass: 10,
            radius: 8,
            color: Color.LightBlue
        ));
    }

    // update physics of all celestial objects
    void UpdatePhysics()
    {
        float dt = timer.Interval / 1000f;
        int n = bodies.Count;
        // PointF[] forces = new PointF[n];

        // compute forces between objects
        PointF[] forces = new PointF[n];
        // var forces = new PointF[n];
        for (int i = 0; i < n; i++)
        {
            PointF netForce = PointF.Empty;
            var bi = bodies[i];

            for (int j = 0; j < n; j++)
            {
                if (i == j) continue; // don't compute force on the same object
                var bj = bodies[j];
                float dx = bj.Position.X - bi.Position.X;
                float dy = bj.Position.Y - bi.Position.Y;
                float dist2 = (dx * dx) + (dy * dy);
                float dist = (float)Math.Sqrt(dist2);
                if (dist < 1) continue; // avoid singularity
                // F = G * m1*m2 / r^2
                float F = (
                    G * (bi.Mass * bj.Mass)
                ) / dist2;
                // net force calulations
                netForce.X += (F * dx) / dist;
                netForce.Y += (F * dy) / dist;
            }
            forces[i] = netForce;
        }

        // integrate motion
        for (int i = 0; i < n; i++)
        {
            var b = bodies[i];
            // a = F/m
            var ax = forces[i].X / b.Mass;
            var ay = forces[i].Y / b.Mass;
            b.Velocity = new PointF(
                b.Velocity.X + ax * dt,
                b.Velocity.Y + ay * dt
            );
            b.Position = new PointF(
                b.Position.X + b.Velocity.X * dt,
                b.Position.Y + b.Velocity.Y * dt
            );

            // record trail
            b.Trail.Enqueue(b.Position);
            if (b.Trail.Count > TrailMax)
            {
                b.Trail.Dequeue();
            }
        }
    }

    // rendering for bodies & trails via onPaint override
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.Black);

        foreach (var b in bodies)
        {
            // draw trail
            var pts = b.Trail.ToArray();
            if (pts.Length > 1)
            {
                using (var pen = new Pen(b.Color, 1))
                {
                    g.DrawLines(pen, pts);
                }
            }
            // if (b.Trail.Count > 1)
            // {
            //     var pts = b.Trail.ToArray();
            //     using (var pen = new Pen(b.Color, 1))
            //     {
            //         G.DrawLines(pen, pts);
            //     }
            // }

            // draw body
            float r = b.Radius;
            var rect = new RectangleF(
                b.Position.X - r,
                b.Position.Y - r,
                2 * r, 2 * r
            );
            using (var brush = new SolidBrush(b.Color))
            {
                g.FillEllipse(brush, rect);
            }
        }

        // if dragging, draw rubberband line
        if (isDragging)
        {
            using (var pen = new Pen(Color.White))
            {
                g.DrawLine(pen, dragStart, currentMouse);
            }
        }
    }

    // USER INPUT (click+drag to spawn celestial bodies)
    protected override void OnMouseDown(MouseEventsArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            isDragging = true;
            dragStart = e.Location;
        }
    }
    protected override void OnMouseMove(MouseEventsArgs e)
    {
        if (isDragging)
        {
            currentMouse = e.Location;
            Invalidate();
        }
    }
    protected override void OnMouseUp(MouseEventsArgs e)
    {
        if (isDragging && e.Button == MouseButtons.Left)
        {
            isDragging = false;
            var dragVector = new PointF(
                e.X - dragStart.X,
                e.Y - dragStart.Y
            );
            float speedFactor = 2f;
            var initVel = new PointF(
                dragVector.X * speedFactor,
                dragVector.Y * speedFactor
            );

            // spawn new body
            // TODO: Scroll-wheel for setting size/mass
            // CURRENT: mass & radius set by drag length
            float mass = MathF.Min(500, dragVector.Length() * 2);
            float radius = MathF.Max(4, mass / 50);
            bodies.Add(new CelestialBody(
                pos: dragStart,
                vel: initVel,
                mass: mass,
                radius: radius,
                color: Color.YellowGreen
            ));
            Invalidate();
        }
    }
}
