using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Meteor : FallingObject
{
    private static Texture2D dustTexture = BaseGame.LoadTexture("dust");
    public const float DEFAULT_FALL_SPEED = 2.0f;

    public Meteor(float? angle = null, float fallSpeed = DEFAULT_FALL_SPEED)
    {
        Sprite = new Sprite("meteor");
        if (angle == null)
        {
            Angle = Util.Random(0, MathHelper.TwoPi);
        }
        else
        {
            Angle = angle.Value;
        }
        FallSpeed = fallSpeed;
    }

    public override void Update()
    {
        base.Update();

        if (Active)
        {
            //planet collision
            if (Util.CircleCollision(ServiceLocator.Get<Planet>().Bounds, new Circle(Sprite.Position, Sprite.ScaledWidth / 3)))
            {
                Explode();
                return;
            }

            //player collision
            Player p = ServiceLocator.Get<Player>();
            if (!MeteorsGame.TitleScreen && p.BoundingRectangle.Intersects(BoundingRectangle))
            {
                Explode();
                p.Touch(this);
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

    public void Explode()
    {
        Active = false;
        Sprite.Texture = dustTexture;
        Sprite.Color = Color.SandyBrown;
        Sprite.Scale = 0.35f;
        Sprite.Alpha = 0.6f;
    }
}

public class CurveMeteor : Meteor
{
    private float curve;

    public CurveMeteor(float curveDegrees = 1.0f, float? angle = null, float fallSpeed = DEFAULT_FALL_SPEED) : base(angle, fallSpeed)
    {
        curve = Util.Random(MathHelper.ToRadians(-curveDegrees), MathHelper.ToRadians(curveDegrees));
    }

    public override void Update()
    {
        if(Active) Angle += curve;
        base.Update();
    }
}

public class OscillatingMeteor : Meteor
{
    //current angle offset oscillates between [-amplitude, amplitude]
    private float currentOffset;
    private int offsetSign;
    private float amplitude;

    public OscillatingMeteor(float amplitude, float? angle = null, float fallSpeed = DEFAULT_FALL_SPEED) : base(angle, fallSpeed)
    {
        this.amplitude = amplitude;
        offsetSign = 1;
    }

    public override void Update()
    {
        Angle += (offsetSign * MathHelper.ToRadians(0.25f));
        currentOffset += (offsetSign * MathHelper.ToRadians(0.25f));
        if (currentOffset < -amplitude || currentOffset > amplitude)
        {
            offsetSign *= -1;
        }

        base.Update();
    }
}