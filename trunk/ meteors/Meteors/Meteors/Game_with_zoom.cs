using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

//player moves a stick figure around the surface of a planet
//avoid meteors that fall towards the planet and explode
public class MeteorsGame : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    //TODO: combine planet sprite and bounding circle into Planet object
    private Player player;
    private Sprite planet;
    private Circle planetBounds;
    private Circle oortCloud;
    private MeteorManager meteors;
    private TimeSpan meteorsLastEnabledTime;

    private const float PLAYER_ROT_SPEED = 0.015f;

    public MeteorsGame()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 800;
        graphics.PreferredBackBufferHeight = 800;
        Content.RootDirectory = "Content";
        ServiceLocator.RegisterService<ContentManager>(Content);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        ServiceLocator.RegisterService<SpriteBatch>(spriteBatch);

        //load and register game content
        player = new Player(new Sprite("stickman", 5f), -MathHelper.PiOver2);
        ServiceLocator.RegisterService<Player>(player);

        planet = new Sprite("planet2", 5f);
        planet.Position = GetScreenCenter();

        //different gameplay perspective:
        planet.Y = GraphicsDevice.Viewport.Height + planet.ScaledHeight / 2;
            
        planetBounds = new Circle(planet.Position, planet.ScaledWidth / 2);
            
        //have meteors randomly spawn on this circle, ensuring they always begin at the same distance
        oortCloud = new Circle(planet.Position, 600);

        meteors = new MeteorManager(Content, 250, TimeSpan.FromMilliseconds(1000), planetBounds, oortCloud);
        meteors.Enabled = false;
    }

    protected override void UnloadContent()
    {
        //TODO: unload any non-ContentManager content here
    }

    protected override void Update(GameTime gameTime)
    {
        //exit on esc
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            this.Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.D))
            player.Angle += PLAYER_ROT_SPEED;
        if (Keyboard.GetState().IsKeyDown(Keys.A))
            player.Angle -= PLAYER_ROT_SPEED;

        //since player's position is centered, offset the circle he is projected onto by his height so his feet touch the surface
        Circle playerPlanetCircle = new Circle(planetBounds.Center, planetBounds.Radius + player.Sprite.ScaledHeight / 2);
        Vector2 pointOnPlanet = Util.GetPointOnCircle(playerPlanetCircle, player.Angle);
        //Console.WriteLine("Point at angle {0} = ({1}, {2})", MathHelper.ToDegrees(playerAngle), pointOnPlanet.X, pointOnPlanet.Y);
        player.Sprite.Position = pointOnPlanet;
        player.Sprite.Rotation = player.Angle + MathHelper.PiOver2;

        //SpawnMeteorsFaster(gameTime.TotalGameTime.TotalSeconds - meteorsLastEnabledTime.TotalSeconds);
        meteors.Update(gameTime);

        if (planet.Y > GetScreenCenter().Y)
        {
            //TODO: display text while zooming out
            //Planet: <planet name>
            //Wave: <meteor wave number>
            planet.Y -= 3f;
            planet.Scale -= 0.011f;
            player.Sprite.Scale -= 0.011f;

            //TODO: create Planet class that has its own sprite, bounds, and oort cloud
            planetBounds = new Circle(planet.Position, planet.ScaledWidth / 2);
            oortCloud = new Circle(planet.Position, 600);
            meteors.planetBounds = planetBounds;
            meteors.oortCloud = oortCloud;
        }
        else if (!meteors.Enabled)
        {
            //TODO: display text indicating we've begun
            meteors.Enabled = true;
            meteorsLastEnabledTime = gameTime.TotalGameTime;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: draw starfield

        // draw game objects
        spriteBatch.Begin();
        planet.Draw();
        player.Draw();
        meteors.Draw();
        spriteBatch.End();

        base.Draw(gameTime);
    }

    //progressively make meteors spawn faster as the game goes on
    private void SpawnMeteorsFaster(double totalGameTimeInSeconds)
    {
        int meteorSpawnIntervalInMs;

        //TODO: make this some continuous function?
        if (totalGameTimeInSeconds > 60)
            meteorSpawnIntervalInMs = 20;
        else if (totalGameTimeInSeconds > 30)
            meteorSpawnIntervalInMs = 100;
        else if (totalGameTimeInSeconds > 10)
            meteorSpawnIntervalInMs = 200;
        else if (totalGameTimeInSeconds > 5)
            meteorSpawnIntervalInMs = 500;
        else if (totalGameTimeInSeconds > 3)
            meteorSpawnIntervalInMs = 750;
        else
            meteorSpawnIntervalInMs = 1000;
                
        meteors.SpawnInterval = TimeSpan.FromMilliseconds(meteorSpawnIntervalInMs);
    }

    private Vector2 GetScreenCenter()
    {
        return new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
    }
}
