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
public class MeteorsGame : BaseGame
{
    private Texture2D background;
    private Player player;
    private Planet planet;
    private MeteorManager meteors;
    private TimerCallback accelerateMeteors;
    private StarManager stars;

    private const int MAX_METEORS = 250;
    private static readonly TimeSpan INITIAL_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan MIN_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan METEOR_ACCELERATION_INTERVAL = TimeSpan.FromMilliseconds(2500);
    private static readonly TimeSpan METEOR_ACCELERATION_STEP = TimeSpan.FromMilliseconds(10);

    private const int MAX_STARS = 0;
    private static readonly TimeSpan STAR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(5000);

    public MeteorsGame()
    {
        IsMouseVisible = true;
        Content.RootDirectory = "Content";
        Window.Title = "Meteors!";
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        background = LoadTexture("starfield");

        player = new Player(new Sprite("stickman", 1.0f), -MathHelper.PiOver2);
        ServiceLocator.Register<Player>(player);

        planet = new Planet(new Sprite("planet2", 1.25f), GameWindow.Center.ToVector2(), 600);
        ServiceLocator.Register<Planet>(planet);

        meteors = new MeteorManager(MAX_METEORS, INITIAL_METEOR_SPAWN_INTERVAL) { IsRandomActive = false };
        accelerateMeteors = new TimerCallback(AccelerateMeteorSpawnRate, METEOR_ACCELERATION_INTERVAL);

        stars = new StarManager(MAX_STARS, STAR_SPAWN_INTERVAL);
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        curKeyboard = Keyboard.GetState();
        curMouse = Mouse.GetState();

        if (curKeyboard.IsKeyDown(Keys.Escape)) this.Exit();

        if (KeyPressedThisFrame(Keys.Tab))
        {
            player.Sprite.Color = Color.White;
        }
        if (KeyPressedThisFrame(Keys.Space))
        {
            if (meteors.IsRandomActive)
            {
                meteors.IsRandomActive = false;
                meteors.IsScriptActive = false;
            }
            else
            {
                meteors.IsRandomActive = true;
                meteors.IsScriptActive = true;
            }
        }
        if (KeyPressedThisFrame(Keys.D1))
        {
            meteors.IsRandomActive = false;
            meteors.LoadWave(@"waves\spirals.txt");
        }
        if (KeyPressedThisFrame(Keys.D2))
        {
            meteors.LoadWave(@"waves\ring.txt");
        }
        if (KeyPressedThisFrame(Keys.D3))
        {
            meteors.LoadWave(@"waves\quadrants.txt");
        }

        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            planet.Angle -= Player.PLAYER_ROT_SPEED;
            meteors.OffsetAngles(-Player.PLAYER_ROT_SPEED);
            stars.OffsetAngles(-Player.PLAYER_ROT_SPEED);
        }
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            planet.Angle += Player.PLAYER_ROT_SPEED;
            meteors.OffsetAngles(Player.PLAYER_ROT_SPEED);
            stars.OffsetAngles(Player.PLAYER_ROT_SPEED);
        }

        player.Update();
        meteors.Update(gameTime);
        stars.Update(gameTime);

        //don't accelerate the meteor spawn rate while a star is waiting to be picked up
        if (stars.Max > 0 && !stars.HasActiveStar()) accelerateMeteors.Update(gameTime);

        prevKeyboard = curKeyboard;
        prevMouse = curMouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        //draw game objects
        spriteBatch.Begin();
        spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, Color.White);
        planet.Draw(spriteBatch);
        meteors.DrawMeteors(spriteBatch, true);
        stars.Draw(spriteBatch, true);
        player.Draw(spriteBatch);
        meteors.DrawDustClouds(spriteBatch);
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
