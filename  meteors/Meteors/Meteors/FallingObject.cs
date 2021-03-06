﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public abstract class FallingObject
{
    public Sprite Sprite { get; set; }
    public bool Active { get; set; }
    public bool MarkedForDeletion { get; set; }
    public float FallSpeed { get; set; }
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
    public bool OnScreen
    {
        get
        {
            return Sprite.X >= 0 && Sprite.X < MeteorsGame.GameWidth &&
                   Sprite.Y >= 0 && Sprite.Y < MeteorsGame.GameHeight;
        }
    }

    //use a smaller rect for collision
    public RotatedRectangle BoundingRectangle { get { return Sprite.RotatedRectangle.Scale(0.8f, 3, 1); } }

    public float OrbitRadius { get; protected set; }

    public FallingObject()
    {
        OrbitRadius = ServiceLocator.Get<Planet>().OortCloud.Radius;
        Active = true;
        MarkedForDeletion = false;
    }

    public virtual void Draw(SpriteBatch sb)
    {
        Sprite.Draw(sb);
    }

    public virtual void Update()
    {
        //active, falling to planet
        if (Active)
        {
            //make object fall by decreasing orbit radius
            OrbitRadius -= FallSpeed;
        }

        CalculatePosition();
    }

    //calculates the object's current position given its current orbit, and the game's planet
    private void CalculatePosition()
    {
        Circle orbit = new Circle(ServiceLocator.Get<Planet>().Center, OrbitRadius);
        Vector2 newPosition = Util.GetPointOnCircle(orbit, Angle);
        Sprite.Position = newPosition;
    }

    protected virtual void Touch(Player p)
    {

    }
}