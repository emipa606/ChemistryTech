using System;
using Verse;

namespace CloudCore;

public class MapComponent_WindSpeed(Map map) : MapComponent(map)
{
    private const int WIND_SPEED_TICKS = 60;

    private const int WIND_DIRECTION_TICKS = 300;

    private int tickCounterSpd, tickCounterDir;

    public float WindDirection;

    public float WindDirectionRad;
    public float WindSpeed;

    private float getNewWindDirection()
    {
        var rnd = new Random();
        var newDir = WindDirection + (((rnd.Next(2) * 2) - 1) * rnd.Next(5));
        switch (newDir)
        {
            case > 359:
                newDir = 360 - newDir;
                break;
            case < 0:
                newDir = 360 + newDir;
                break;
        }

        return newDir;
    }

    public override void MapComponentTick()
    {
        if (tickCounterSpd > WIND_SPEED_TICKS)
        {
            WindSpeed = map.windManager.WindSpeed;
            tickCounterSpd = 0;
            if (tickCounterDir <= WIND_DIRECTION_TICKS)
            {
                return;
            }

            WindDirection = getNewWindDirection();
            WindDirectionRad = (float)(Math.PI * WindDirection / 180f);
            tickCounterDir = 0;
        }
        else
        {
            tickCounterSpd++;
            tickCounterDir++;
        }
    }
}