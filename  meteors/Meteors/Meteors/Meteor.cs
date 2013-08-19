using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Meteor
{
    public Sprite Sprite { get; set; }
    public bool Active { get; set; }
    public bool MarkedForDeletion { get; set; }

    //use a smaller bounding rect on meteors
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle.Scale(0.8f, 3, 1); } }

    private Line fallLine;
    private float fallLinePercent = 0;
    private const float FALL_SPEED = 0.003f;
    private static Texture2D dustTexture;

    public Meteor(Sprite spr)
    {
        Sprite = spr;
        Active = true;
        MarkedForDeletion = false;

        //aim the meteors at random positions on a circle within the planet
        Planet planet = ServiceLocator.Get<Planet>();
        Circle meteorDestCircle = new Circle(planet.Center, planet.Radius - 100);
        Vector2 dest = Util.GetRandomPointOnCircle(meteorDestCircle);
        fallLine = new Line(spr.Position, dest);
    }

    public void Draw()
    {
        Sprite.Draw();
        //Util.DrawRectangle(BoundingRectangle, new Color(128, 0, 0, 32));
        //Util.DrawLine(fallLine, new Color(0, 0, 128, 32));

        //lazy load a single, shared dust texture
        if (dustTexture == null) dustTexture = ServiceLocator.Get<ContentManager>().Load<Texture2D>("dust");
    }

    public void Update()
    {
        //meteor is active, moving towards its target
        if (Active)
        {
            //make meteor trace its fall line
            Vector2 newPosition = Util.CalculatePointOnLine(fallLine, fallLinePercent);
            Sprite.Position = newPosition;
            fallLinePercent += FALL_SPEED;
            if (fallLinePercent > 1) fallLinePercent = 1;

            //planet collision
            //TODO: play sound (add sound system to ServiceLocator)
            if (Util.CircleCollision(ServiceLocator.Get<Planet>().Bounds, new Circle(Sprite.Position, Sprite.ScaledWidth / 3)))
            {
                Active = false;
                Sprite.Texture = dustTexture;
                Sprite.Color = Color.SandyBrown;
                Sprite.Scale = 0.35f;
                Sprite.Alpha = 0.6f;
                return;
            }

            //player collision
            Player p = ServiceLocator.Get<Player>();
            if (p.BoundingRectangle.Intersects(BoundingRectangle))
            {
                MarkedForDeletion = true;
                p.Sprite.Color = Util.RandomColor();
            }
        }
        //meteor has hit the planet and exploded, slowly fade out then mark for deletion
        else
        {
            Sprite.Alpha -= 0.0075f;
            Sprite.Scale += 0.0015f;
            if (Sprite.Alpha <= 0) MarkedForDeletion = true;
        }
    }
}