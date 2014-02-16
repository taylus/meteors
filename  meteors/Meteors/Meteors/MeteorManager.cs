using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages a list of meteors and allows all to be updated and drawn at once.
/// </summary>
public class MeteorManager
{
    public int MaxRandomMeteors { get; set; }
    public TimeSpan SpawnInterval { get; set; }
    public int Count { get { return meteors.Count; } }
    public bool IsRandomActive { get; set; }
    public bool IsScriptActive { get; set; }

    private List<Meteor> meteors = new List<Meteor>();
    private TimeSpan lastRandomSpawnTime;

    private List<MeteorWave> scriptedWaves = new List<MeteorWave>();

    public MeteorManager(int max, TimeSpan spawnInterval)
    {
        MaxRandomMeteors = max;
        SpawnInterval = spawnInterval;
        IsRandomActive = false;
        IsScriptActive = false;
    }

    public void Update(GameTime curTime)
    {
        if (IsRandomActive)
        {
            if (meteors.Count < MaxRandomMeteors)
            {
                TimeSpan timeSinceLastSpawn = curTime.TotalGameTime.Subtract(lastRandomSpawnTime);
                if (timeSinceLastSpawn >= SpawnInterval)
                {
                    //spawn a meteor
                    meteors.Add(new Meteor());
                    lastRandomSpawnTime = curTime.TotalGameTime;
                }
            }
        }
        foreach(MeteorWave wave in scriptedWaves)
        {
            //advance the current time by however much time elapsed since the last update
            wave.CurrentSpawnTimeIndex += curTime.ElapsedGameTime;

            //spawn meteors
            List<ScriptedMeteor> meteorsToSpawn = wave.GetMeteorsToSpawn();
            meteors.AddRange(meteorsToSpawn.Select(m => m.Meteor));

            //turn on/off random spawns
            ScriptedRandom? random = wave.GetRandomSetting();
            if (random != null)
            {
                if (random.Value.SpawnInterval != null)
                {
                    IsRandomActive = true;
                    SpawnInterval = random.Value.SpawnInterval.Value;
                }
                else
                {
                    IsRandomActive = false;
                }
            }

            wave.LastSpawnTimeIndex = wave.CurrentSpawnTimeIndex;
        }

        //remove completed waves
        for (int i = scriptedWaves.Count - 1; i >= 0; i--)
        {
            MeteorWave wave = scriptedWaves[i];
            if (wave.IsComplete()) 
                scriptedWaves.Remove(wave);
        }

        //remove dead meteors
        for (int i = meteors.Count - 1; i >= 0; i--)
        {
            Meteor m = meteors[i];
            m.Update();
            if (m.MarkedForDeletion) meteors.Remove(m);
        }
    }

    public void DrawMeteors(SpriteBatch sb, bool debug = false)
    {
        foreach (Meteor m in meteors)
        {
            if(m.Active) m.Draw(sb);
        }

        if (debug)
        {
            Viewport screen = sb.GraphicsDevice.Viewport;
            sb.DrawString(BaseGame.Font, string.Format("Waves: {0}", scriptedWaves.Count), new Vector2(2, screen.Height - 2 * BaseGame.Font.LineSpacing), Color.White);
            sb.DrawString(BaseGame.Font, string.Format("Meteors: {0}", meteors.Count), new Vector2(2, screen.Height - BaseGame.Font.LineSpacing), Color.White);

            string intervalText = String.Format("Interval: {0} ms", SpawnInterval.TotalMilliseconds);
            Vector2 textSize = BaseGame.Font.MeasureString(intervalText);
            string randomIndicator = IsRandomActive ? "On" : "Off";
            sb.DrawString(BaseGame.Font, string.Format("Random: {0}", randomIndicator), new Vector2(screen.Width - textSize.X - 2, screen.Height - textSize.Y * 2), Color.White);
            sb.DrawString(BaseGame.Font, intervalText, new Vector2(screen.Width - textSize.X - 2, screen.Height - textSize.Y), Color.White);
        }
    }

    public void DrawDustClouds(SpriteBatch sb)
    {
        foreach (Meteor m in meteors)
        {
            if(!m.Active) m.Draw(sb);
        }
    }

    public void LoadWave(string pathname, long timeIndexOffset = 0)
    {
        MeteorWave wave = new MeteorWave();
        wave.LastSpawnTimeIndex = TimeSpan.Zero;
        wave.CurrentSpawnTimeIndex = TimeSpan.Zero;
        wave.ScriptedMeteors = new List<ScriptedMeteor>();
        wave.RandomSettings = new List<ScriptedRandom>();

        using (StreamReader sr = new StreamReader(pathname))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                string[] lineData = line.Split(' ');

                long time;
                if (!long.TryParse(lineData[0], out time))
                {
                    throw new Exception("Error loading line in wave \"" + pathname + "\". Expected <time index in ms>");
                }

                //expecting lines like "1000 random 100" to start spawning meteors 100 ms apart 1 second into this wave
                //or "1000 random off" to stop spawning random meteors 1 second into this wave
                if(string.Equals(lineData[1], "random", StringComparison.OrdinalIgnoreCase))
                {
                    int spawnInterval;
                    if (lineData.Length > 2 && int.TryParse(lineData[2], out spawnInterval))
                    {
                        wave.RandomSettings.Add(new ScriptedRandom(time + timeIndexOffset, TimeSpan.FromMilliseconds(spawnInterval)));
                    }
                    else
                    {
                        wave.RandomSettings.Add(new ScriptedRandom(time + timeIndexOffset, null));
                    }
                }
                else
                {
                    float angle;
                    if (!float.TryParse(lineData[1], out angle))
                    {
                        throw new Exception("Error loading meteor in wave \"" + pathname + "\". Expected format: <time index in ms> <angle in degrees>");
                    }

                    angle = MathHelper.ToRadians(float.Parse(lineData[1]));
                    Meteor meteor = new Meteor(angle);
                    wave.ScriptedMeteors.Add(new ScriptedMeteor(time + timeIndexOffset, meteor));
                }
            }
        }

        scriptedWaves.Add(wave);
    }

    public void LoadLevel(string pathname)
    {
        using (StreamReader sr = new StreamReader(pathname))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                string[] lineData = line.Split(' ');

                long time;
                if (!long.TryParse(lineData[0], out time))
                {
                    throw new Exception("Error loading wave \"" + pathname + "\". Expected format: <time index in ms> <wave file>");
                }

                string wavefile = lineData[1].Replace("\"", "");
                LoadWave(wavefile, time);
            }
        }
    }

    public void OffsetAngles(float angle)
    {
        meteors.ForEach(m => m.Angle += angle);
        foreach (MeteorWave wave in scriptedWaves)
        {
            wave.ScriptedMeteors.ForEach(m => m.Meteor.Angle += angle);
        }
    }
}
