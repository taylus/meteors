using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages a list of stars and allows all to be updated and drawn at once.
/// </summary>
public class PowerupManager
{
    public int Max { get; set; }
    public TimeSpan SpawnInterval { get; set; }
    public bool Enabled { get; set; }
    public int Count { get { return powerups.Count; } }

    private List<FallingObject> powerups = new List<FallingObject>();
    private TimeSpan untilNextSpawn;

    public PowerupManager(int max, TimeSpan spawnInterval)
    {
        Max = max;
        untilNextSpawn = spawnInterval;
        SpawnInterval = spawnInterval;
        Enabled = true;
    }

    public void Update(GameTime curTime)
    {
        if (!Enabled) return;

        if (powerups.Count < Max)
        {
            untilNextSpawn -= curTime.ElapsedGameTime;
            if (untilNextSpawn <= TimeSpan.Zero)
            {
                //TODO: spawn a random powerup
                powerups.Add(new BombPowerup());
                untilNextSpawn = SpawnInterval;
            }
        }

        //iterate backwards to remove dead powerups inline
        for (int i = powerups.Count - 1; i >= 0; i--)
        {
            FallingObject o = powerups[i];
            o.Update();
            if (o.MarkedForDeletion)
            {
                powerups.Remove(o);
                untilNextSpawn = SpawnInterval;
            }
        }
    }

    public void Draw(SpriteBatch sb, bool debug)
    {
        foreach (FallingObject o in powerups)
            o.Draw(sb);
    }

    public bool HasActivePowerup()
    {
        return powerups != null && powerups.Count(s => !s.Active) > 0;
    }

    public void OffsetAngles(float angle)
    {
        powerups.ForEach(s => s.Angle += angle);
    }

    public void Clear()
    {
        powerups.Clear();
    }
}
