using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages a list of meteors and allows all to be updated and drawn at once.
/// </summary>
public class MeteorManager
{
    public int Max { get; set; }
    public TimeSpan SpawnInterval { get; set; }
    public bool Enabled { get; set; }
    public int Count { get { return meteors.Count; } }

    private List<Meteor> meteors = new List<Meteor>();
    private TimeSpan lastSpawnTime;

    public MeteorManager(int max, TimeSpan spawnInterval)
    {
        Max = max;
        SpawnInterval = spawnInterval;
        Enabled = true;
    }

    public void Update(GameTime curTime)
    {
        if (!Enabled) return;

        if (meteors.Count < Max)
        {
            TimeSpan timeSinceLastSpawn = curTime.TotalGameTime.Subtract(lastSpawnTime);
            if (timeSinceLastSpawn >= SpawnInterval)
            {
                //spawn a meteor
                meteors.Add(new Meteor());
                lastSpawnTime = curTime.TotalGameTime;
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

    public void Draw(bool debug)
    {
        foreach (Meteor m in meteors)
            m.Draw();

        if (debug)
        {
            SpriteBatch sb = ServiceLocator.Get<SpriteBatch>();
            SpriteFont font = ServiceLocator.Get<SpriteFont>();
            Viewport screen = sb.GraphicsDevice.Viewport;
            sb.DrawString(font, String.Format("Meteors: {0}", meteors.Count), new Vector2(2, screen.Height - font.LineSpacing), Color.White);

            string intervalText = String.Format("Interval: {0} ms", SpawnInterval.TotalMilliseconds);
            Vector2 textSize = font.MeasureString(intervalText);
            sb.DrawString(font, intervalText, new Vector2(screen.Width - textSize.X - 2, screen.Height - textSize.Y), Color.White);
        }
    }

    public void OffsetAngles(float angle)
    {
        meteors.ForEach(m => m.Angle += angle);
    }
}
