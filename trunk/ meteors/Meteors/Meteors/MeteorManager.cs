using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

public struct ScriptedMeteor
{
    public long TimeIndex;
    public Meteor Meteor;

    public ScriptedMeteor(long time, Meteor meteor)
    {
        TimeIndex = time;
        Meteor = meteor;
    }
}

//think of MeteorWaves like a song
//a time index window slides over the notes and spawns them as meteors
public class MeteorWave
{
    public List<ScriptedMeteor> ScriptedMeteors;
    public TimeSpan LastSpawnTimeIndex;
    public TimeSpan CurrentSpawnTimeIndex;

    //determines, removes, and returns the meteors to spawn given this wave's current and last time indexes
    public List<ScriptedMeteor> GetMeteorsToSpawn()
    {
        //List<ScriptedMeteor> meteorsToSpawn =  
        //    ScriptedMeteors.Where(m => m.TimeIndex >= LastSpawnTimeIndex.TotalMilliseconds &&
        //                               m.TimeIndex < CurrentSpawnTimeIndex.TotalMilliseconds).ToList();
        var meteorsToSpawn = ScriptedMeteors.Where(m => m.TimeIndex < CurrentSpawnTimeIndex.TotalMilliseconds).ToList();
        meteorsToSpawn.ForEach(m => ScriptedMeteors.Remove(m));
        return meteorsToSpawn;
    }

    //returns true if all meteors
    public bool IsComplete()
    {
        return ScriptedMeteors.All(m => m.TimeIndex < CurrentSpawnTimeIndex.TotalMilliseconds);
    }
}

/// <summary>
/// Manages a list of meteors and allows all to be updated and drawn at once.
/// </summary>
public class MeteorManager
{
    public int MaxRandomMeteors { get; set; }
    public TimeSpan SpawnInterval { get; set; }
    public int Count { get { return activeMeteors.Count; } }
    public bool IsRandomActive { get; set; }
    public bool IsScriptActive { get; set; }

    private List<Meteor> activeMeteors = new List<Meteor>();
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
            if (activeMeteors.Count < MaxRandomMeteors)
            {
                TimeSpan timeSinceLastSpawn = curTime.TotalGameTime.Subtract(lastRandomSpawnTime);
                if (timeSinceLastSpawn >= SpawnInterval)
                {
                    //spawn a meteor
                    activeMeteors.Add(new Meteor());
                    lastRandomSpawnTime = curTime.TotalGameTime;
                }
            }
        }
        foreach(MeteorWave wave in scriptedWaves)
        {
            //advance the current time by however much time elapsed since the last update
            wave.CurrentSpawnTimeIndex += curTime.ElapsedGameTime;

            //determine which meteors to spawn
            List<ScriptedMeteor> meteorsToSpawn = wave.GetMeteorsToSpawn();
            activeMeteors.AddRange(meteorsToSpawn.Select(m => m.Meteor));
            wave.LastSpawnTimeIndex = wave.CurrentSpawnTimeIndex;
        }

        //remove completed waves
        for (int i = scriptedWaves.Count - 1; i >= 0; i--)
        {
            MeteorWave wave = scriptedWaves[i];
            if (wave.IsComplete()) scriptedWaves.Remove(wave);
        }

        //remove dead meteors
        for (int i = activeMeteors.Count - 1; i >= 0; i--)
        {
            Meteor m = activeMeteors[i];
            m.Update();
            if (m.MarkedForDeletion) activeMeteors.Remove(m);

            //TODO: do something about draw order so dust clouds are always drawn over new meteors?
            //need to draw them as separate objects, on top of player and other falling meteors
        }
    }

    public void DrawMeteors(SpriteBatch sb, bool debug = false)
    {
        foreach (Meteor m in activeMeteors)
        {
            if(m.Active) m.Draw(sb);
        }

        if (debug)
        {
            Viewport screen = sb.GraphicsDevice.Viewport;
            sb.DrawString(BaseGame.Font, string.Format("Waves: {0}", scriptedWaves.Count), new Vector2(2, screen.Height - 2 * BaseGame.Font.LineSpacing), Color.White);
            sb.DrawString(BaseGame.Font, string.Format("Meteors: {0}", activeMeteors.Count), new Vector2(2, screen.Height - BaseGame.Font.LineSpacing), Color.White);

            string intervalText = String.Format("Interval: {0} ms", SpawnInterval.TotalMilliseconds);
            Vector2 textSize = BaseGame.Font.MeasureString(intervalText);
            sb.DrawString(BaseGame.Font, intervalText, new Vector2(screen.Width - textSize.X - 2, screen.Height - textSize.Y), Color.White);
        }
    }

    public void DrawDustClouds(SpriteBatch sb)
    {
        foreach (Meteor m in activeMeteors)
        {
            if(!m.Active) m.Draw(sb);
        }
    }

    public void LoadWave(string pathname)
    {
        MeteorWave wave = new MeteorWave();
        wave.LastSpawnTimeIndex = TimeSpan.Zero;
        wave.CurrentSpawnTimeIndex = TimeSpan.Zero;
        wave.ScriptedMeteors = new List<ScriptedMeteor>();

        using (StreamReader sr = new StreamReader(pathname))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                string[] lineData = line.Split(' ');

                long time; float angle;
                if (!long.TryParse(lineData[0], out time) || !float.TryParse(lineData[1], out angle))
                {
                    throw new Exception("Error loading wave \"" + pathname + "\". Expected format: <time index in ms> <angle in degrees>");
                }
                angle = MathHelper.ToRadians(float.Parse(lineData[1]));
                Meteor meteor = new Meteor(angle);
                wave.ScriptedMeteors.Add(new ScriptedMeteor(time, meteor));
            }
        }

        scriptedWaves.Add(wave);
    }

    public void OffsetAngles(float angle)
    {
        activeMeteors.ForEach(m => m.Angle += angle);
        foreach (MeteorWave wave in scriptedWaves)
        {
            wave.ScriptedMeteors.ForEach(m => m.Meteor.Angle += angle);
        }
    }
}
