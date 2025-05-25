namespace gravity_simple;


public partial class Form1 : Form
{

    List<CelestialBody> bodies = new List<CelestialBody>();
    const float G = 6.674e-1f;   // gravitational force
    const int TrailMax = 100;     // max trail length
    Timer timer = new Timer { Interval = 16 }; // ~60 FPS
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

    void UpdatePhysics()
    {
        float dt = timer.Interval / 1000f;
        int n = bodies.Count;

        // compute forces between objects
        var forces = new PointF[n];
        for (int i = 0; i < n; i++)
        {
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
                // net
                net.X += (F * dx) / dist;
                net.Y += (F * dy) / dist;
            }
            forces[i] = net;
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
}
