using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Represents a 2D image that can draw itself to the screen.
/// Maintains its own position, rotation, scale, etc.
/// </summary>
public class Sprite
{
    //TODO: add support for colored transparent overlays?
    public Texture2D Texture { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Rotation { get; set; }
    public float Scale { get; set; }
    public Color Color { get; set; }
    public float Alpha { get; set; }
    public int Width { get { return Texture.Width; } }
    public int ScaledWidth { get { return (int)(Width * Scale); } }
    public int Height { get { return Texture.Height; } }
    public int ScaledHeight { get { return (int)(Height * Scale); } }
    public Vector2 Position
    {
        get { return new Vector2(X, Y); }
        set { X = value.X; Y = value.Y; }
    }
    public Vector2 Center 
    { 
        get { return new Vector2(Width / 2, Height / 2); }
        set { Position = new Vector2(value.X - Width / 2, value.Y - Height / 2); }
    }
    public Rectangle Rectangle
    {
        get { return new Rectangle((int)(X - ScaledWidth/2), (int)(Y - ScaledHeight/2), ScaledWidth, ScaledHeight); }
    }
    public RotatedRectangle RotatedRectangle
    {
        get { return new RotatedRectangle(Rectangle, Rotation); }
    }

    public Sprite(string assetName, Color color, Vector2 pos, float rotation, float scale)
    {
        Texture = ServiceLocator.Get<ContentManager>().Load<Texture2D>(assetName);
        Color = color;
        Position = pos;
        Rotation = rotation;
        Scale = scale;
        Alpha = 1.0f;
    }

    public Sprite(string assetName) : this(assetName, Color.White, Vector2.Zero, 0, 1.0f) { }
    public Sprite(string assetName, Color color) : this(assetName, color, Vector2.Zero, 0, 1.0f) { }
    public Sprite(string assetName, Vector2 pos) : this(assetName, Color.White, pos, 0, 1.0f) { }
    public Sprite(string assetName, float scale) : this(assetName, Color.White, Vector2.Zero, 0, scale) { }
    public Sprite(string assetName, Vector2 pos, float scale) : this(assetName, Color.White, pos, 0, scale) { }

    public void Draw()
    {
        // note: sprite is *centered* on Position, not upper-left corner
        // use Vector2.Zero instead of Center as origin for default behavior
        // TODO: have a Draw and DrawCentered?

        SpriteBatch sb = ServiceLocator.Get<SpriteBatch>();
        sb.Draw(Texture, Position, null, Color.Lerp(Color.Transparent, Color, MathHelper.Clamp(Alpha, 0, 1)), Rotation, Center, Scale, SpriteEffects.None, 0);
    }

    public Color[] GetPixels()
    {
        Color[] c = new Color[ScaledWidth * Height];
        Texture.GetData(c);
        return c;
    }

    /// <summary>
    /// Determines if there is any overlap of non-transparent pixels between the given Sprites.
    /// </summary>
    public static bool PixelCollision(Sprite sp1, Sprite sp2)
    {
        //determine the intersection rect
        int top = Math.Max(sp1.Rectangle.Top, sp2.Rectangle.Top);
        int bottom = Math.Min(sp1.Rectangle.Bottom, sp2.Rectangle.Bottom);
        int left = Math.Max(sp1.Rectangle.Left, sp2.Rectangle.Left);
        int right = Math.Min(sp1.Rectangle.Right, sp2.Rectangle.Right);

        //get the pixel data for each sprite
        Color[] p1 = sp1.GetPixels();
        Color[] p2 = sp2.GetPixels();

        //check every point within the intersection rect
        for (int y = top; y < bottom; y++)
        {
            for (int x = left; x < right; x++)
            {
                Color c1 = p1[(x - sp1.Rectangle.Left) + (y - sp1.Rectangle.Top) * sp1.Rectangle.Width];
                Color c2 = p2[(x - sp2.Rectangle.Left) + (y - sp2.Rectangle.Top) * sp2.Rectangle.Width];
                
                //collision if both rectangles are non-transparent
                if (c1.A <= 0 && c2.A != 0) return true;
            }
        }

        //no collision detected
        return false;
    }
}
