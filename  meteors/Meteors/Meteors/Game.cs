using System;
using System.IO;
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
    private StarManager stars;

    private const int MAX_METEORS = 250;
    private static readonly TimeSpan INITIAL_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(150);

    private const int MAX_STARS = 1;
    private static readonly TimeSpan STAR_SPAWN_INTERVAL = TimeSpan.FromSeconds(5);

    private TimeSpan untilNextWave = TimeSpan.FromSeconds(5);

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

        stars = new StarManager(MAX_STARS, STAR_SPAWN_INTERVAL);
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        curKeyboard = Keyboard.GetState();
        curMouse = Mouse.GetState();

        if (curKeyboard.IsKeyDown(Keys.Escape)) this.Exit();
        HandleDebugInput();

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

        //comment out to make the game stop playing; useful when testing new waves
        UpdateGameBehavior(gameTime);

        player.Update();
        meteors.Update(gameTime);
        stars.Update(gameTime);

        prevKeyboard = curKeyboard;
        prevMouse = curMouse;
        base.Update(gameTime);
    }

    private void HandleDebugInput()
    {
        if (KeyPressedThisFrame(Keys.Tab))
        {
            player.Sprite.Color = Color.White;
        }
        if (KeyPressedThisFrame(Keys.Space))
        {
            meteors.IsRandomActive = !meteors.IsRandomActive;
        }
        if (KeyPressedThisFrame(Keys.D1))
        {
            meteors.LoadWave(@"waves\hemispiral.txt");
        }
        if (KeyPressedThisFrame(Keys.D2))
        {
            meteors.LoadWave(@"waves\ring.txt");
        }
        if (KeyPressedThisFrame(Keys.D3))
        {
            meteors.LoadWave(@"waves\quadrants.txt");
        }

        if (ScrollUpThisFrame() && meteors.SpawnInterval > TimeSpan.Zero)
        {
            meteors.SpawnInterval -= TimeSpan.FromMilliseconds(10);
        }
        else if (ScrollDownThisFrame())
        {
            meteors.SpawnInterval += TimeSpan.FromMilliseconds(10);
        }
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

    //current game behavior:
    //spawn random meteors for a random amount of time
    //disable random meteors and spawn a scripted wave at random
    //re-enable random meteors once the wave finishes spawning
    //repeat
    private void UpdateGameBehavior(GameTime gameTime)
    {
        untilNextWave -= gameTime.ElapsedGameTime;
        if (untilNextWave <= TimeSpan.Zero)
        {
            if (!meteors.IsWaveSpawning())
            {
                meteors.IsRandomActive = false;
                meteors.LoadWave(GetRandomWave());
            }

            //back off and wait again, even if we didn't load a wave
            untilNextWave = TimeSpan.FromSeconds(Util.Random(10, 20));
        }
        else if (!meteors.IsWaveSpawning())
        {
            //enable random meteors if time remains until the next wave, and we're not already spawning one
            meteors.IsRandomActive = true;
        }
    }

    private string GetRandomWave()
    {
        string[] files = Directory.GetFiles("waves");
        return files[Util.Random(0, files.Length)];
    }
}
