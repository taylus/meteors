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
    private static Texture2D background;
    private static Player player;
    private static Planet planet;
    private static MeteorManager meteors;
    private static StarManager stars;
    private static StarPowerMeter starMeter;

    private const int MAX_METEORS = 250;
    private static readonly TimeSpan INITIAL_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(160);

    private const int MAX_STARS = 1;
    private static readonly TimeSpan STAR_SPAWN_INTERVAL = TimeSpan.FromSeconds(10);

    private static TimeSpan untilNextWave;

    private static readonly TimeSpan gameOverTitleScreenInactiveTime = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan nextLevelTitleScreenInactiveTime = TimeSpan.FromSeconds(1);
    private static TimeSpan untilTitleScreenActive;

    private const string instructions = "Press any key!";
    private static TimeSpan untilInstructionsBlink = TimeSpan.FromMilliseconds(700);
    private static bool instructionsVisible = false;

    private static SpriteFont titleFont;
    private const string GAME_TITLE = "Meteor Madness!";
    private const string NEXT_LEVEL = "Level Up!";
    private static string titleScreenText;
    public static bool TitleScreen { get; private set; }

    public MeteorsGame()
    {
        IsMouseVisible = true;
        Content.RootDirectory = "Content";
        Window.Title = GAME_TITLE;
        titleScreenText = GAME_TITLE;
        TitleScreen = true;
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        background = LoadTexture("starfield");
        titleFont = Content.Load<SpriteFont>("titlefont");

        player = new Player(new Sprite("stickman", 1.0f), -MathHelper.PiOver2);
        ServiceLocator.Register<Player>(player);

        planet = new Planet(new Sprite("planet2", 1.25f), GameWindow.Center.ToVector2(), 600);
        ServiceLocator.Register<Planet>(planet);

        meteors = new MeteorManager(MAX_METEORS, INITIAL_METEOR_SPAWN_INTERVAL) { IsRandomActive = true };

        stars = new StarManager(MAX_STARS, STAR_SPAWN_INTERVAL);
        starMeter = new StarPowerMeter();
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive) return;

        curKeyboard = Keyboard.GetState();
        curMouse = Mouse.GetState();

        if (curKeyboard.IsKeyDown(Keys.Escape)) this.Exit();

        if (TitleScreen)
        {
            if (untilTitleScreenActive >= TimeSpan.Zero)
            {
                untilTitleScreenActive -= gameTime.ElapsedGameTime;
            }
            else
            {
                untilInstructionsBlink -= gameTime.ElapsedGameTime;
                if (untilInstructionsBlink <= TimeSpan.Zero)
                {
                    instructionsVisible = !instructionsVisible;
                    untilInstructionsBlink = TimeSpan.FromMilliseconds(700);
                }
                if (AnyKeyPressedThisFrame() ||
                    curMouse.LeftButton == ButtonState.Pressed ||
                    curMouse.RightButton == ButtonState.Pressed)
                {
                    StartGameFromTitleScreen();
                }
            }
        }
        else
        {
            HandleDebugInput();

            if (curKeyboard.IsKeyDown(Keys.D))
            {
                planet.Angle -= Player.PLAYER_ROT_SPEED;
                meteors.OffsetAngles(-Player.PLAYER_ROT_SPEED);
                stars.OffsetAngles(-Player.PLAYER_ROT_SPEED);
            }
            if (curKeyboard.IsKeyDown(Keys.A))
            {
                planet.Angle += Player.PLAYER_ROT_SPEED;
                meteors.OffsetAngles(Player.PLAYER_ROT_SPEED);
                stars.OffsetAngles(Player.PLAYER_ROT_SPEED);
            }

            //comment out to make the game stop playing; useful when testing new waves
            UpdateGameBehavior(gameTime);

            player.Update();
            stars.Update(gameTime);
        }

        meteors.Update(gameTime);

        prevKeyboard = curKeyboard;
        prevMouse = curMouse;
        base.Update(gameTime);
    }

    private void HandleDebugInput()
    {
        if (KeyPressedThisFrame(Keys.Tab))
        {
            EndGameToTitleScreen();
        }
        if (KeyPressedThisFrame(Keys.Space))
        {
            meteors.IsRandomActive = !meteors.IsRandomActive;
        }
        if (KeyPressedThisFrame(Keys.D1))
        {
            meteors.IsRandomActive = false;
            meteors.LoadWave(@"waves\spirals.txt");
            untilNextWave = TimeSpan.FromSeconds(Util.Random(10, 20));
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
        meteors.DrawMeteors(spriteBatch, !TitleScreen);
        if (TitleScreen)
        {
            Util.DrawRectangle(spriteBatch, GameWindow, Color.Lerp(Color.Black, Color.Transparent, 0.5f));
            Vector2 measureTitleString = titleFont.MeasureString(titleScreenText);
            Vector2 titleStringPosition = new Vector2(GameWidth / 2 - measureTitleString.X / 2, 70).Round();
            spriteBatch.DrawString(titleFont, titleScreenText, titleStringPosition, Color.White);

            if (instructionsVisible)
            {
                Vector2 measureInstructionString = Font.MeasureString(instructions);
                Vector2 instructionsStringPosition = new Vector2(GameWidth / 2 - measureInstructionString.X / 2, GameHeight - 120).Round();
                spriteBatch.DrawString(Font, instructions, instructionsStringPosition, Color.White);
            }
        }
        else
        {
            stars.Draw(spriteBatch, true);
            player.Draw(spriteBatch);
            starMeter.Draw(spriteBatch, player.StarPower);
        }
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
            //untilNextWave = TimeSpan.FromSeconds(Util.Random(10, 20));
            untilNextWave = TimeSpan.FromSeconds(20);
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

    public static void StartGameFromTitleScreen()
    {
        TitleScreen = false;
        meteors.Clear();
        stars.Clear();
        meteors.SpawnInterval -= TimeSpan.FromMilliseconds(10);
        //untilNextWave = TimeSpan.FromSeconds(Util.Random(10, 20));
        untilNextWave = TimeSpan.FromSeconds(20);
    }

    public static void EndGameToTitleScreen()
    {
        TitleScreen = true;
        instructionsVisible = false;
        untilTitleScreenActive = gameOverTitleScreenInactiveTime;
        titleScreenText = GAME_TITLE;
        meteors.SpawnInterval = INITIAL_METEOR_SPAWN_INTERVAL;
    }

    public static void NextLevel()
    {
        TitleScreen = true;
        instructionsVisible = false;
        untilTitleScreenActive = nextLevelTitleScreenInactiveTime;
        titleScreenText = NEXT_LEVEL;
        player.StarPower = 0;
    }
}
