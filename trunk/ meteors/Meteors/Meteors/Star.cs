using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Star : FallingObject
{
    public Star()
    {
        Sprite = new Sprite("star", 0.75f);
        Angle = Util.Random(0, MathHelper.TwoPi);
        FallSpeed = 5.0f;
    }

    public override void Update()
    {
        base.Update();

        if (Active)
        {
            //planet collision
            if (Util.CircleCollision(ServiceLocator.Get<Planet>().Bounds, new Circle(Sprite.Position, Sprite.ScaledWidth / 4.0f)))
            {
                Active = false;
            }
        }

        //player collision
        if (ServiceLocator.Get<Player>().BoundingRectangle.Intersects(BoundingRectangle))
        {
            MarkedForDeletion = true;
        }
    }
}