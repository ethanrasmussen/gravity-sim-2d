using System.Collections.Generic;
using System.Drawing;

public class CelestialBody
{
    public PointF Position;
    public PointF Velocity;
    public float Mass;
    public float Radius;
    public Color Color;

    // store recent positions
    public Queue<PointF> Trail = new Queue<PointF>();

    public CelestialBody(PointF pos, PointF vel, float mass, float radius, Color color)
    {
        Position = pos;
        Velocity = vel;
        Mass = mass;
        Radius = radius;
        Color = color;
    }
}
