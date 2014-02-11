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
    private TimeSpan lastSpawnTime;

    private List<ScriptedMeteor> scriptedMeteors = new List<ScriptedMeteor>();
    private TimeSpan waveLoadTime;
    private TimeSpan lastTimeIndex;

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
                TimeSpan timeSinceLastSpawn = curTime.TotalGameTime.Subtract(lastSpawnTime);
                if (timeSinceLastSpawn >= SpawnInterval)
                {
                    //spawn a meteor
                    meteors.Add(new Meteor());
                    lastSpawnTime = curTime.TotalGameTime;
                }
            }
        }
        if (IsScriptActive)
        {
            //spawn any meteors whose time indexes are between the last update and now
            //TODO: FIXME, not working right when game is not active, or when multiple waves loaded
            if (scriptedMeteors.Count > 0)
            {
                List<ScriptedMeteor> meteorsToSpawn = scriptedMeteors.
                    Where(m => m.TimeIndex + waveLoadTime.TotalMilliseconds >= lastTimeIndex.TotalMilliseconds &&
                               m.TimeIndex + waveLoadTime.TotalMilliseconds < curTime.TotalGameTime.TotalMilliseconds).ToList();

                //remove from list of scripted meteors and add to list of current meteors
                meteorsToSpawn.ForEach(m => scriptedMeteors.Remove(m));
                meteors.AddRange(meteorsToSpawn.Select(m => m.Meteor));
                lastTimeIndex = curTime.TotalGameTime;
            }
            else
            {
                //no more meteors in the script left to spawn
                IsScriptActive = false;
            }
        }

        //iterate backwards to remove dead meteors inline
        for (int i = meteors.Count - 1; i >= 0; i--)
        {
            Meteor m = meteors[i];
            m.Update();
            if (m.MarkedForDeletion) meteors.Remove(m);

            //TODO: do something about draw order so dust clouds are always drawn over new meteors?
        }
    }

    public void Draw(SpriteBatch sb, bool debug = false)
    {
        foreach (Meteor m in meteors)
            m.Draw(sb);

        if (debug)
        {
            Viewport screen = sb.GraphicsDevice.Viewport;
            sb.DrawString(BaseGame.Font, String.Format("Meteors: {0}", meteors.Count), new Vector2(2, screen.Height - BaseGame.Font.LineSpacing), Color.White);

            string intervalText = String.Format("Interval: {0} ms", SpawnInterval.TotalMilliseconds);
            Vector2 textSize = BaseGame.Font.MeasureString(intervalText);
            sb.DrawString(BaseGame.Font, intervalText, new Vector2(screen.Width - textSize.X - 2, screen.Height - textSize.Y), Color.White);
        }
    }

    public void LoadWave(string pathname, GameTime curTime)
    {
        IsScriptActive = true;
        lastTimeIndex = TimeSpan.Zero;
        waveLoadTime = curTime.TotalGameTime;

        using (StreamReader sr = new StreamReader(pathname))
        {
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                string[] lineData = line.Split(' ');

                //TODO: add error handling
                //expected format: <time index in ms> <angle in degrees>
                long time = long.Parse(lineData[0]);
                float angle = MathHelper.ToRadians(float.Parse(lineData[1]));
                Meteor meteor = new Meteor(angle);
                scriptedMeteors.Add(new ScriptedMeteor(time, meteor));
            }
        }
    }

    public void OffsetAngles(float angle)
    {
        meteors.ForEach(m => m.Angle += angle);
        scriptedMeteors.ForEach(m => m.Meteor.Angle += angle);
    }
}
