using System;
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
    public int StarPower { get; set; }
    public int BombCount { get; set; }
    public long Score { get; set; }
    public bool Invulnerable { get { return untilVulnerable > TimeSpan.Zero; } }
    public bool Visible { get; set; }

    //opportunity for inheritance once all drawable objects inherit together
    //default bounding rect to cover the whole sprite, and some objects can override it smaller
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle; } }

    //amount to increase rotation by on keydown
    public const float PLAYER_ROT_SPEED = 0.015f;

    private TimeSpan untilVulnerable;
    private static readonly TimeSpan invulnerabilityTime = TimeSpan.FromMilliseconds(500);

    private TimeSpan untilNextBlink;
    private static readonly TimeSpan blinkInterval = TimeSpan.FromMilliseconds(100);

    public Player(Sprite spr, float angle)
    {
        Sprite = spr;
        Angle = angle;
        Visible = true;
        Reset();
    }

    public void Reset()
    {
        //untilVulnerable = TimeSpan.Zero;
        //untilNextBlink = TimeSpan.Zero;
        Visible = true;
        StarPower = 0;
        Score = 0;
    }

    public void Update(GameTime gameTime)
    {
        if (Invulnerable)
        {
            untilVulnerable -= gameTime.ElapsedGameTime;
            untilNextBlink -= gameTime.ElapsedGameTime;
            if (untilNextBlink <= TimeSpan.Zero)
            {
                Visible = !Visible;
                untilNextBlink = blinkInterval;
            }
        }
        else if (!Invulnerable && !Visible)
        {
            //always become visible after invulnerability ends
            Visible = true;
        }
    }

    public void PositionOnPlanet(Planet planet)
    {
        //calculate player's x,y coords given angle and radius (polar -> cartesian)
        //since sprite positions are centered, offset the circle the player projected onto by his height so his feet touch the surface
        Circle playerPlanetCircle = new Circle(planet.Center, planet.Radius + Sprite.ScaledHeight / 2);
        Vector2 pointOnPlanet = Util.GetPointOnCircle(playerPlanetCircle, Angle);

        //update sprite's position and angle
        //Console.WriteLine("Point at angle {0} = ({1}, {2})", MathHelper.ToDegrees(Angle), pointOnPlanet.X, pointOnPlanet.Y);
        Sprite.Position = pointOnPlanet;
        Sprite.Rotation = Angle + MathHelper.PiOver2;
    }

    public void Touch(FallingObject o)
    {
        //TODO: refactor into separate Touch methods in FallingObject subclasses
        if (typeof(Meteor).IsAssignableFrom(o.GetType()) && !Invulnerable)
        {
            Reset();
            MeteorsGame.EndGameToTitleScreen();
                
            //make player temporarily invulnerable when hit
            //untilVulnerable = invulnerabilityTime;
        }
        else if (o.GetType() == typeof(Star))
        {
            StarPower++;
            if (StarPower >= StarPowerMeter.MAX_POWER)
            {
                Reset();
                MeteorsGame.NextLevel();
            }

            //determine if the player "caught" the star without it touching the planet
            Planet planet = ServiceLocator.Get<Planet>();
            if (Math.Abs(o.OrbitRadius - planet.Radius) > o.Sprite.ScaledHeight / 4)
                Score += Star.POINTS_FOR_CATCHING;
            else
                Score += Star.POINTS_FOR_COLLECTING;
        }
        else if (o.GetType() == typeof(BombPowerup))
        {
            BombCount++;
        }
    }

    public void Draw(SpriteBatch sb)
    {
        if (Visible)
        {
            Sprite.Draw(sb);
            //Util.DrawRectangle(sb, Sprite.RotatedRectangle, new Color(0, 128, 0, 128));
        }
    }
}