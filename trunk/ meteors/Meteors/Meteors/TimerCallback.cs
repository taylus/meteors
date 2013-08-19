using System;
using Microsoft.Xna.Framework;

public delegate void Callback();

public class TimerCallback
{
    public TimeSpan Interval { get; set; }
    private TimeSpan lastFiredTime;
    private Callback f;

    public TimerCallback(Callback c, TimeSpan interval)
    {
        f = c;
        Interval = interval;
        lastFiredTime = TimeSpan.Zero;
    }

    public void Update(GameTime currentGameTime)
    {
        if (currentGameTime.TotalGameTime.Subtract(lastFiredTime) >= Interval)
        {
            f();
            lastFiredTime = currentGameTime.TotalGameTime;
        }
    }
}
