using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// This file contains miscellaneous utility and extension methods that don't really belong anywhere else.
/// </summary>

public class Circle
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }

    public Circle(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}

public class Line
{
    public Vector2 Point1 { get; set; }
    public Vector2 Point2 { get; set; }

    public Line(Vector2 p1, Vector2 p2)
    {
        Point1 = p1;
        Point2 = p2;
    }
}

public static class Util
{
    private static Random rand;
    private static Texture2D dummyTexture;

    static Util()
    {
        rand = new Random(Guid.NewGuid().GetHashCode());
    }

    public static float Random()
    {
        return (float)rand.NextDouble();
    }

    public static float Random(float min, float max)
    {
        return ((max - min) * ((float)rand.NextDouble())) + min;
    }

    public static int Random(int min, int max)
    {
        return rand.Next(min, max);
    }

    public static Color RandomColor()
    {
        return new Color(Random(0, 255), Random(0, 255), Random(0, 255));
    }

    public static Vector2 Random(float minX, float maxX, float minY, float maxY)
    {
        return new Vector2(Random(minX, maxY), Random(minY, maxY));
    }

    public static Vector2 ToVector2(this Point p)
    {
        return new Vector2(p.X, p.Y);
    }

    public static float Distance(float x1, float y1, float x2, float y2)
    {
        double dx = x1 - x2;
        double dy = y1 - y2;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public static float Distance(Vector2 p1, Vector2 p2)
    {
        return Distance(p1.X, p1.Y, p2.X, p2.Y);
    }

    //determines the coordinates of the point on the given circle at the given angle
    public static Vector2 GetPointOnCircle(Circle circle, float angle)
    {
        return new Vector2(circle.Center.X + (circle.Radius * (float)Math.Cos(angle)), circle.Center.Y + (circle.Radius * (float)Math.Sin(angle)));
    }

    public static float GetAngleOnCircle(Circle circle, Vector2 point)
    {
        float a = Distance(circle.Center.X, circle.Center.Y, point.X, point.Y);
        float b = Distance(circle.Center.X, circle.Center.Y, circle.Center.X + 120, circle.Center.Y);
        float c = Distance(point.X, point.Y, circle.Center.X + 120, circle.Center.Y);
        //float theta = point.Y < Video.Screen.Height / 2 ? Math.Acos((a * a + b * b - c * c) / (2 * a * b)) : 2 * Math.PI - Math.Acos((a * a + b * b - c * c) / (2 * a * b));
        return (float)(Math.Acos((a * a + b * b - c * c) / (2 * a * b)));
    }

    public static Vector2 GetRandomPointOnCircle(Circle circle)
    {
        return GetPointOnCircle(circle, Random(0, MathHelper.TwoPi));
    }

    public static void DrawRectangle(SpriteBatch sb, Rectangle rect, Color color)
    {
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(sb.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
        }

        sb.Draw(dummyTexture, rect, color);
    }

    public static void DrawRectangle(RotatedRectangle rect, Color color)
    {
        SpriteBatch sb = ServiceLocator.Get<SpriteBatch>();
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(sb.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
        }

        Rectangle aPositionAdjusted = new Rectangle(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2), rect.Width, rect.Height);
        sb.Draw(dummyTexture, aPositionAdjusted, new Rectangle(0, 0, 2, 6), color, rect.Rotation, new Vector2(1, 3), SpriteEffects.None, 0);
    }

    public static void DrawLine(Line line, Color color)
    {
        SpriteBatch sb = ServiceLocator.Get<SpriteBatch>();
        if (dummyTexture == null)
        {
            dummyTexture = new Texture2D(sb.GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
        }

        float angle = (float)System.Math.Atan2(line.Point2.Y - line.Point1.Y, line.Point2.X - line.Point1.X);
        float length = Vector2.Distance(line.Point1, line.Point2);

        sb.Draw(dummyTexture, line.Point1, null, color, angle, Vector2.Zero, new Vector2(length, 2.0f), SpriteEffects.None, 0);
    }

    /// <summary>
    /// Returns the coordinates of the point situated percentage length down the given line.
    /// </summary>
    /// <param name="line">The given line</param>
    /// <param name="percent">The percentage length (e.g. 0.5 is the midpoint of the line)</param>
    public static Vector2 CalculatePointOnLine(Line line, float percent)
    {
        if (percent < 0 || percent > 1)
            throw new ArgumentException("Percent passed to CalculatePointOnLine is out of range");

        float x = line.Point1.X + ((line.Point2.X - line.Point1.X) * percent);
        float y = line.Point1.Y + ((line.Point2.Y - line.Point1.Y) * percent);
        return new Vector2(x, y);
    }

    public static bool CircleCollision(Circle c1, Circle c2)
    {
        //instead of taking sqrt, compare distance squared to combined radii squared
        float dx = c2.Center.X - c1.Center.X;
        float dy = c2.Center.Y - c1.Center.Y;
        float radii = c1.Radius + c2.Radius;
        return ((dx * dx) + (dy * dy) < (radii * radii));
    }

    public static Vector2 Round(this Vector2 v)
    {
        return new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
    }
}