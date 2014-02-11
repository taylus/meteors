using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

//TODO: why aren't Star and Meteor the same class!?
public class Star
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
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle.Scale(0.8f, 3, 1); ; } }

    private float orbitRadius;
    private const float FALL_SPEED = 5.0f;

    public Star()
    {
        Planet planet = ServiceLocator.Get<Planet>();
        Sprite = new Sprite("star", 0.75f);
        Angle = Util.Random(0, MathHelper.TwoPi);
        orbitRadius = planet.OortCloud.Radius;
        Active = true;
        MarkedForDeletion = false;
    }

    public void Draw(SpriteBatch sb)
    {
        Sprite.Draw(sb);
        //Util.DrawRectangle(BoundingRectangle, new Color(128, 0, 0, 32));
    }

    public void Update()
    {
        //TODO: stars should award points, giving more the quicker the player gets to them 
        //(i.e. decrease points awarded as you sit on the planet, and eventually despawn)

        //active, moving towards target
        if (Active)
        {
            //make star fall by decreasing orbit radius
            orbitRadius -= FALL_SPEED;

            //planet collision
            //TODO: play sound (add sound system to ServiceLocator)
            if (Util.CircleCollision(ServiceLocator.Get<Planet>().Bounds, new Circle(Sprite.Position, Sprite.ScaledWidth / 2.5f)))
            {
                Active = false;
            }
        }

        CalculatePosition();

        //player collision
        Player p = ServiceLocator.Get<Player>();
        if (p.BoundingRectangle.Intersects(BoundingRectangle))
        {
            MarkedForDeletion = true;
        }
    }

    //calculates the star's current position given the planet and the star's current orbit
    private void CalculatePosition()
    {
        Circle orbit = new Circle(ServiceLocator.Get<Planet>().Center, orbitRadius);
        Vector2 newPosition = Util.GetPointOnCircle(orbit, Angle);
        Sprite.Position = newPosition;
    }
}