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
    private SpriteFont font;

    private Texture2D background;
    private Player player;
    private Planet planet;
    private MeteorManager meteors;
    private TimerCallback accelerateMeteors;
    private StarManager stars;

    private const int MAX_METEORS = 250;
    private static readonly TimeSpan INITIAL_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan MIN_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan METEOR_ACCELERATION_INTERVAL = TimeSpan.FromMilliseconds(2500);
    private static readonly TimeSpan METEOR_ACCELERATION_STEP = TimeSpan.FromMilliseconds(10);

    private const int MAX_STARS = 1;
    private static readonly TimeSpan STAR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(5000);

    //TODO: "endurance" by just slightly moving around in one spot is okay, but gets boring fast
    // need to add some mechanics that force the player to move around, requiring them to risk getting hit
    // 1.) shooting stars that land and stay on the planet surface, disappearing after a short time
    // 2.) some sort of disaster that prevents you from walking on some slice of the planet for a time (lava?)
    // it should not feel unfair when you get hit by a meteor; player should feel like they messed up

    public MeteorsGame()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 800;
        graphics.PreferredBackBufferHeight = 800;
        graphics.ApplyChanges();
        Content.RootDirectory = "Content";
        Window.Title = "Meteors!";
        ServiceLocator.Register<ContentManager>(Content);
    }

    protected override void LoadContent()
    {
        //load and register game content and objects
        spriteBatch = new SpriteBatch(GraphicsDevice);
        ServiceLocator.Register<SpriteBatch>(spriteBatch);

        font = Content.Load<SpriteFont>("font");
        ServiceLocator.Register<SpriteFont>(font);

        background = Content.Load<Texture2D>("starfield");

        player = new Player(new Sprite("stickman", 1.0f), -MathHelper.PiOver2);
        ServiceLocator.Register<Player>(player);

        planet = new Planet(new Sprite("planet2", 1.25f), Util.GetScreenCenter(), 600);
        ServiceLocator.Register<Planet>(planet);

        meteors = new MeteorManager(MAX_METEORS, INITIAL_METEOR_SPAWN_INTERVAL);
        accelerateMeteors = new TimerCallback(AccelerateMeteorSpawnRate, METEOR_ACCELERATION_INTERVAL);

        stars = new StarManager(MAX_STARS, STAR_SPAWN_INTERVAL);
    }

    protected override void Update(GameTime gameTime)
    {
        //exit on esc
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            this.Exit();

        //two ways of handling perspective
        //1.) static world, mobile player -> player circles around the planet
        //2.) static player, mobile world -> planet rotates under player, meteors, etc appear to rotate too
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            //1.)
            //player.Angle += Player.PLAYER_ROT_SPEED;

            //2.)
            planet.Angle -= Player.PLAYER_ROT_SPEED;
            meteors.OffsetAngles(-Player.PLAYER_ROT_SPEED);
            stars.OffsetAngles(-Player.PLAYER_ROT_SPEED);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            //1.)
            //player.Angle -= Player.PLAYER_ROT_SPEED;

            //2.)
            planet.Angle += Player.PLAYER_ROT_SPEED;
            meteors.OffsetAngles(Player.PLAYER_ROT_SPEED);
            stars.OffsetAngles(Player.PLAYER_ROT_SPEED);
        }

        player.Update();
        meteors.Update(gameTime);
        stars.Update(gameTime);

        //don't accelerate the meteor spawn rate while a star is waiting to be picked up
        if (!stars.HasActiveStar()) accelerateMeteors.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        //draw game objects
        spriteBatch.Begin();
        spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, Color.White);
        planet.Draw();
        player.Draw();
        meteors.Draw(true);
        stars.Draw(true);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    //timer callback function to slowly speed up the spawn rate of meteors at a constant speed
    private void AccelerateMeteorSpawnRate()
    {
        if (meteors.SpawnInterval > MIN_METEOR_SPAWN_INTERVAL)
        {
            meteors.SpawnInterval = meteors.SpawnInterval.Subtract(METEOR_ACCELERATION_STEP);            
        }
    }
}
