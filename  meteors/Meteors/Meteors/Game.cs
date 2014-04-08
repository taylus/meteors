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
    private static PowerupManager powerups;

    private const int MAX_METEORS = 250;
    private static readonly TimeSpan INITIAL_METEOR_SPAWN_INTERVAL = TimeSpan.FromMilliseconds(200);

    private const int MAX_STARS = 1;
    private static readonly TimeSpan STAR_SPAWN_INTERVAL = TimeSpan.FromSeconds(15);

    private const int MAX_POWERUPS = 1;
    private static readonly TimeSpan POWERUP_SPAWN_INTERVAL = TimeSpan.FromSeconds(20);

    private static TimeSpan untilNextWave = TimeSpan.FromSeconds(20);

    private static readonly TimeSpan TITLE_SCREEN_INACTIVE_TIME = TimeSpan.FromSeconds(1);
    private static TimeSpan untilTitleScreenActive;

    private const string INSTRUCTIONS = "Press any key!";
    private static TimeSpan untilInstructionsBlink = TimeSpan.FromMilliseconds(700);
    private static bool instructionsVisible = false;

    private static readonly TimeSpan SCORE_UP_INTERVAL = TimeSpan.FromMilliseconds(50);
    private static TimeSpan untilNextScoreUp = SCORE_UP_INTERVAL;
    private const int SCORE_UP_AMOUNT = 1;

    private const bool DEBUG = false;
    private static SpriteFont uiFont;
    private static SpriteFont titleFont;
    private const string GAME_TITLE = "Meteor Madness!";
    private const string NEXT_LEVEL = "Level Up!";
    private static string titleScreenText;
    public static bool TitleScreen { get; private set; }

    private static Song bgMusic;

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
        Font = Content.Load<SpriteFont>("debugfont");
        titleFont = Content.Load<SpriteFont>("titlefont");
        uiFont = Content.Load<SpriteFont>("uifont");

        planet = new Planet(new Sprite("planet2", 1.25f), GameWindow.Center.ToVector2(), 600);
        ServiceLocator.Register<Planet>(planet);

        player = new Player(new Sprite("stickman", 1.0f), -MathHelper.PiOver2);
        ServiceLocator.Register<Player>(player);

        player.PositionOnPlanet(planet);

        meteors = new MeteorManager(MAX_METEORS, INITIAL_METEOR_SPAWN_INTERVAL) { IsRandomActive = true, CurveMeteorPercent = 0.25f, CurveMeteorDegrees = 0.25f };
        stars = new StarManager(MAX_STARS, STAR_SPAWN_INTERVAL);
        powerups = new PowerupManager(MAX_POWERUPS, POWERUP_SPAWN_INTERVAL);
        starMeter = new StarPowerMeter();

        bgMusic = Content.Load<Song>("doom");
        MediaPlayer.Volume = 0.6f;
    }

    protected override void Update(GameTime gameTime)
    {
        if (!IsActive)
        {
            //pause background music if game goes inactive
            if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause();

            //and stop updating everything else too
            return;
        }
        else if (IsActive && MediaPlayer.State == MediaState.Paused)
        {
            //resume audio once game is active again
            MediaPlayer.Resume();
        }

        curKeyboard = Keyboard.GetState();
        curMouse = Mouse.GetState();

        if (curKeyboard.IsKeyDown(Keys.Escape)) this.Exit();

        meteors.Update(gameTime);

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
            if(DEBUG) HandleDebugInput();

            if (curKeyboard.IsKeyDown(Keys.D))
            {
                planet.Angle -= Player.PLAYER_ROT_SPEED;
                meteors.OffsetAngles(-Player.PLAYER_ROT_SPEED);
                stars.OffsetAngles(-Player.PLAYER_ROT_SPEED);
                powerups.OffsetAngles(-Player.PLAYER_ROT_SPEED);
            }
            if (curKeyboard.IsKeyDown(Keys.A))
            {
                planet.Angle += Player.PLAYER_ROT_SPEED;
                meteors.OffsetAngles(Player.PLAYER_ROT_SPEED);
                stars.OffsetAngles(Player.PLAYER_ROT_SPEED);
                powerups.OffsetAngles(Player.PLAYER_ROT_SPEED);
            }
            if (KeyPressedThisFrame(Keys.Space) && player.BombCount > 0)
            {
                meteors.BombExplosion();
                player.BombCount--;
            }

            //comment out to make the default game behavior of random wave spawning stop (useful when testing)
            UpdateGameBehavior(gameTime);

            UpdateScore(gameTime);
            player.Update(gameTime);
            stars.Update(gameTime);
            powerups.Update(gameTime);
        }

        prevKeyboard = curKeyboard;
        prevMouse = curMouse;
        base.Update(gameTime);
    }

    private void HandleDebugInput()
    {
        if (KeyPressedThisFrame(Keys.Tab))
        {
            //EndGameToTitleScreen();
            NextLevel();
        }
        if (KeyPressedThisFrame(Keys.Space))
        {
            meteors.IsRandomActive = !meteors.IsRandomActive;
        }
        if (KeyPressedThisFrame(Keys.D1))
        {
            meteors.IsRandomActive = false;
            meteors.LoadWave(@"waves\cw_spirals.txt");
            //untilNextWave = TimeSpan.FromSeconds(Util.Random(10, 20));
        }
        if (KeyPressedThisFrame(Keys.D2))
        {
            meteors.IsRandomActive = false;
            meteors.LoadWave(@"waves\ccw_spirals.txt");
        }
        if (KeyPressedThisFrame(Keys.D3))
        {
            meteors.LoadWave(@"waves\safezone.txt");
        }

        if (ScrollUpThisFrame() && meteors.SpawnInterval > TimeSpan.Zero)
        {
            meteors.SpawnInterval -= TimeSpan.FromMilliseconds(10);
            if (meteors.SpawnInterval < TimeSpan.Zero) meteors.SpawnInterval = TimeSpan.Zero;
        }
        else if (ScrollDownThisFrame())
        {
            meteors.SpawnInterval += TimeSpan.FromMilliseconds(10);
        }
    }

    private void UpdateScore(GameTime gameTime)
    {
        if (untilNextScoreUp >= TimeSpan.Zero)
        {
            untilNextScoreUp -= gameTime.ElapsedGameTime;
        }
        else
        {
            untilNextScoreUp = SCORE_UP_INTERVAL;
            player.Score += SCORE_UP_AMOUNT;
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        //draw game objects
        spriteBatch.Begin();
        spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, Color.White);
        planet.Draw(spriteBatch);
        meteors.DrawMeteors(spriteBatch, DEBUG);
        if (TitleScreen)
        {
            Util.DrawRectangle(spriteBatch, GameWindow, Color.Lerp(Color.Black, Color.Transparent, 0.5f));
            Vector2 measureTitleString = titleFont.MeasureString(titleScreenText);
            Vector2 titleStringPosition = new Vector2(GameWidth / 2 - measureTitleString.X / 2, 70).Round();
            spriteBatch.DrawString(titleFont, titleScreenText, titleStringPosition, Color.White);

            if (instructionsVisible)
            {
                Vector2 measureInstructionString = Font.MeasureString(INSTRUCTIONS);
                Vector2 instructionsStringPosition = new Vector2(GameWidth / 2 - measureInstructionString.X / 2, GameHeight - 120).Round();
                spriteBatch.DrawString(Font, INSTRUCTIONS, instructionsStringPosition, Color.White);
            }
        }
        else
        {
            DrawUIText(spriteBatch);
            stars.Draw(spriteBatch);
            powerups.Draw(spriteBatch);
            player.Draw(spriteBatch);
            starMeter.Draw(spriteBatch, player.StarPower);
        }
        meteors.DrawDustClouds(spriteBatch);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawUIText(SpriteBatch sb)
    {
        string score = string.Format("Score: {0}", player.Score);
        sb.DrawString(uiFont, score, new Vector2(StarPowerMeter.Height, StarPowerMeter.Height), Color.White);

        string bombs = string.Format("Bombs: {0}", player.BombCount);
        Vector2 measureBombString = uiFont.MeasureString(bombs);
        sb.DrawString(uiFont, bombs, new Vector2(GameWidth - measureBombString.X - StarPowerMeter.Height, StarPowerMeter.Height), Color.White);
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
            untilNextWave = TimeSpan.FromSeconds(Util.Random(20, 40));
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
        meteors.ClearMeteors();
        stars.Clear();
        powerups.Clear();
        //meteors.SpawnInterval -= TimeSpan.FromMilliseconds(10);
        //untilNextWave = TimeSpan.FromSeconds(Util.Random(10, 20));
        //untilNextWave = TimeSpan.FromSeconds(20);
        //meteors.LoadLevel(@"levels\doom.txt");
        //MediaPlayer.Play(bgMusic);
    }

    public static void EndGameToTitleScreen()
    {
        TitleScreen = true;
        meteors.ClearWaves();
        instructionsVisible = false;
        untilTitleScreenActive = TITLE_SCREEN_INACTIVE_TIME;
        titleScreenText = GAME_TITLE;
        meteors.SpawnInterval = INITIAL_METEOR_SPAWN_INTERVAL;
        MediaPlayer.Stop();
    }

    public static void NextLevel()
    {
        TitleScreen = true;
        instructionsVisible = false;
        untilTitleScreenActive = TITLE_SCREEN_INACTIVE_TIME;
        titleScreenText = NEXT_LEVEL;
        meteors.SpawnInterval -= TimeSpan.FromMilliseconds(25); //make random meteors spawn faster
        player.Reset();
    }
}
