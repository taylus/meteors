using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class BombPowerup : FallingObject
{
    public BombPowerup()
    {
        Sprite = new Sprite("bomb", 0.2f);
        Angle = Util.Random(0, MathHelper.TwoPi);
        FallSpeed = 4.0f;
    }

    public override void Update()
    {
        base.Update();

        if (Active)
        {
            //planet collision
            if (Util.CircleCollision(ServiceLocator.Get<Planet>().Bounds, new Circle(Sprite.Position, Sprite.ScaledWidth / 3.0f)))
            {
                Active = false;
            }
        }

        //player collision
        Player player = ServiceLocator.Get<Player>();
        if (!MarkedForDeletion && player.BoundingRectangle.Intersects(BoundingRectangle))
        {
            MarkedForDeletion = true;
            player.Touch(this);
        }
    }
}