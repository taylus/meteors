using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Meteor
{
    public Sprite Sprite { get; set; }
    public bool Active { get; set; }
    public bool MarkedForDeletion { get; set; }
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

    //use a smaller bounding rect on meteors
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle.Scale(0.8f, 3, 1); } }

    private float orbitRadius;
    private const float FALL_SPEED = 2.0f;
    private static Texture2D dustTexture;

    public Meteor()
    {
        Planet planet = ServiceLocator.Get<Planet>();
        Sprite = new Sprite("meteor");
        Angle = Util.Random(0, MathHelper.TwoPi);
        orbitRadius = planet.OortCloud.Radius;
        Active = true;
        MarkedForDeletion = false;
    }

    public void Draw()
    {
        Sprite.Draw();
        //Util.DrawRectangle(BoundingRectangle, new Color(128, 0, 0, 32));

        //lazy load a single, shared dust texture
        if (dustTexture == null) dustTexture = ServiceLocator.Get<ContentManager>().Load<Texture2D>("dust");
    }

    public void Update()
    {
        //meteor is active, moving towards its target
        if (Active)
        {
            //make meteor fall by decreasing orbit radius
            orbitRadius -= FALL_SPEED;
            Circle orbit = new Circle(ServiceLocator.Get<Planet>().Center, orbitRadius);
            Vector2 newPosition = Util.GetPointOnCircle(orbit, Angle);
            Sprite.Position = newPosition;

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