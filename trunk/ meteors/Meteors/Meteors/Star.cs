using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Star
{
    public Sprite Sprite { get; set; }
    public bool Active { get; set; }
    public bool MarkedForDeletion { get; set; }

    //use a smaller bounding rect on meteors
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle.Scale(0.8f, 3, 1); ; } }

    private Line fallLine;
    private float fallLinePercent = 0;
    private const float FALL_SPEED = 0.015f;

    public Star(Sprite spr)
    {
        Sprite = spr;
        Active = true;
        MarkedForDeletion = false;

        //aim the star at a random position on a circle within the planet
        Planet planet = ServiceLocator.Get<Planet>();
        Circle destCircle = new Circle(planet.Center, planet.Radius - 100);
        Vector2 dest = Util.GetRandomPointOnCircle(destCircle);
        fallLine = new Line(spr.Position, dest);
    }

    public void Draw()
    {
        Sprite.Draw();
        //Util.DrawRectangle(BoundingRectangle, new Color(128, 0, 0, 32));
        //Util.DrawLine(fallLine, new Color(0, 0, 128, 32));
    }

    public void Update()
    {
        //TODO: stars should award points, giving more the quicker the player gets to them 
        //(i.e. decrease points awarded as you sit on the planet, and eventually despawn)

        //active, moving towards target
        if (Active)
        {
            //trace along the fall line
            Sprite.Position = Util.CalculatePointOnLine(fallLine, fallLinePercent);
            fallLinePercent += FALL_SPEED;
            if (fallLinePercent > 1) fallLinePercent = 1;

            //planet collision
            //TODO: play sound (add sound system to ServiceLocator)
            if (Util.CircleCollision(ServiceLocator.Get<Planet>().Bounds, new Circle(Sprite.Position, Sprite.ScaledWidth / 2.5f)))
            {
                Active = false;
            }
        }

        //player collision
        Player p = ServiceLocator.Get<Player>();
        if (p.BoundingRectangle.Intersects(BoundingRectangle))
        {
            MarkedForDeletion = true;
        }
    }
}