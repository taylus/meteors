using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Convenience class to bind together a player sprite and its current angle of rotation on the planet's surface.
/// </summary>
public class Player
{
    public Sprite Sprite { get; private set; }
    public float Angle { get; set; }
    public float PlanetDistance { get; set; }

    //opportunity for inheritance once all drawable objects inherit together
    //default bounding rect to cover the whole sprite, and some objects can override it smaller
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle; } }

    //amount to increase rotation by on keydown
    public const float PLAYER_ROT_SPEED = 0.015f;

    public Player(Sprite spr, float angle)
    {
        Sprite = spr;
        Angle = angle;
    }

    public void Update()
    {
        //since sprite positions are centered, offset the circle the player projected onto by his height so his feet touch the surface
        Planet planet = ServiceLocator.Get<Planet>();
        Circle playerPlanetCircle = new Circle(planet.Center, planet.Radius + Sprite.ScaledHeight / 2);
        Vector2 pointOnPlanet = Util.GetPointOnCircle(playerPlanetCircle, Angle);

        //update sprite's position and angle
        //Console.WriteLine("Point at angle {0} = ({1}, {2})", MathHelper.ToDegrees(playerAngle), pointOnPlanet.X, pointOnPlanet.Y);
        Sprite.Position = pointOnPlanet;
        Sprite.Rotation = Angle + MathHelper.PiOver2;
    }

    public void Draw(SpriteBatch sb)
    {
        Sprite.Draw(sb);
        //Util.DrawRectangle(Sprite.RotatedRectangle, new Color(0, 128, 0, 32));
    }
}
