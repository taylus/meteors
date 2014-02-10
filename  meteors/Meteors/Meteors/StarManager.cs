﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Manages a list of stars and allows all to be updated and drawn at once.
/// TODO: lots of potential for inheritance between Star/Meteor and their managers.
/// </summary>
public class StarManager
{
    public int Max { get; set; }
    public TimeSpan SpawnInterval { get; set; }
    public bool Enabled { get; set; }
    public int Count { get { return stars.Count; } }

    private List<Star> stars = new List<Star>();
    private TimeSpan lastSpawnTime;

    public StarManager(int max, TimeSpan spawnInterval)
    {
        Max = max;
        SpawnInterval = spawnInterval;
        Enabled = true;
    }

    public void Update(GameTime curTime)
    {
        if (!Enabled) return;

        if (stars.Count < Max)
        {
            TimeSpan timeSinceLastSpawn = curTime.TotalGameTime.Subtract(lastSpawnTime);
            if (timeSinceLastSpawn >= SpawnInterval)
            {
                //spawn a star
                stars.Add(new Star());
                lastSpawnTime = curTime.TotalGameTime;
            }
        }

        //iterate backwards to remove dead stars inline
        for (int i = stars.Count - 1; i >= 0; i--)
        {
            Star s = stars[i];
            s.Update();
            if (s.MarkedForDeletion)
            {
                stars.Remove(s);
                lastSpawnTime = curTime.TotalGameTime;
            }
        }
    }

    public void Draw(bool debug)
    {
        foreach (Star s in stars)
            s.Draw();
    }

    public bool HasActiveStar()
    {
        return stars != null && stars.Count(s => !s.Active) > 0;
    }

    public void OffsetAngles(float angle)
    {
        stars.ForEach(s => s.Angle += angle);
    }
}
