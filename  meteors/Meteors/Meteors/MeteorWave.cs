using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

//a record for a scripted meteor read from a meteor wave text file
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

//a record for a scripted random on/off instruction read from a meteor wave text file
public struct ScriptedRandom
{
    public long TimeIndex;

    //the interval between spawning random meteors
    //null indicates this is an instruction to turn OFF random meteor spawning
    public TimeSpan? SpawnInterval;

    //percentage of meteors spawned that should be randomly curved
    public float? CurvePercent;

    public ScriptedRandom(long time, TimeSpan? spawnInterval, float? curvePercent)
    {
        TimeIndex = time;
        SpawnInterval = spawnInterval;
        CurvePercent = curvePercent;
    }
}

//a wave of meteors read from a text file
//provides meteors for MeteorManager to spawn, and turns on or off random meteors per the contents of its file
//think of running a meteor wave like playing a song
//a time window slides over the time-indexed events and handles them at that time
public class MeteorWave
{
    public List<ScriptedMeteor> ScriptedMeteors;
    public List<ScriptedRandom> RandomSettings;
    public TimeSpan LastSpawnTimeIndex;
    public TimeSpan CurrentSpawnTimeIndex;

    //determines, removes, and returns the meteors to spawn given this wave's current and last time indexes
    public List<ScriptedMeteor> GetMeteorsToSpawn()
    {
        var meteorsToSpawn = ScriptedMeteors.Where(m => m.TimeIndex < CurrentSpawnTimeIndex.TotalMilliseconds).ToList();
        meteorsToSpawn.ForEach(m => ScriptedMeteors.Remove(m));
        return meteorsToSpawn;
    }

    public ScriptedRandom? GetRandomSetting()
    {
        var randomSettings = RandomSettings.Where(r => r.TimeIndex < CurrentSpawnTimeIndex.TotalMilliseconds).ToList();
        if (randomSettings.Count > 0)
        {
            randomSettings.ForEach(r => RandomSettings.Remove(r));
            return randomSettings.OrderBy(r => r.TimeIndex).Last();    //if more than one are effective in the same update cycle, take the latest
        }
        else
        {
            return null;
        }
    }

    //returns true if all meteors in this wave have been spawned
    public bool IsComplete()
    {
        return ScriptedMeteors.Count <= 0 && RandomSettings.Count <= 0;
    }
}