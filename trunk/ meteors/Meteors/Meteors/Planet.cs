using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Convenience class to bind together a planet sprite, its bounding circle, and its "oort cloud" (circle on which meteors spawn)
/// </summary>
public class Planet
{
    public Sprite Sprite { get; private set; }
    public Circle Bounds { get; private set; }
    public Circle OortCloud { get; private set; }
    public float Angle
    {
        get
        {
            return Sprite.Rotation;
        }
        set
        {
            Sprite.Rotation = value;
        }
    }

    public Vector2 Center { get { return Bounds.Center; } }
    public float Radius { get { return Bounds.Radius; } }

    public Planet(Sprite spr, Vector2 position, int oortCloudRadius)
    {
        Sprite = spr;
        spr.Position = position;
        Bounds = new Circle(position, Sprite.ScaledWidth / 2);
        OortCloud = new Circle(position, oortCloudRadius);
    }

    public void Draw()
    {
        Sprite.Draw();
    }
}
