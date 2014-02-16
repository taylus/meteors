﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public class Meteor : FallingObject
{
    private static Texture2D dustTexture = BaseGame.LoadTexture("dust");

    public Meteor(float? angle = null)
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
        FallSpeed = 2.0f;
    }

    public override void Update()
    {
        base.Update();

        if (Active)
        {
            //planet collision
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