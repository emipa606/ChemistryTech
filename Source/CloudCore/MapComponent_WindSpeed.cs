﻿using System;
using Verse;

namespace CloudCore
{
    public class MapComponent_WindSpeed : MapComponent
    {
        private const int WIND_SPEED_TICKS = 60;

        private const int WIND_DIRECTION_TICKS = 300;

        private int tickCounterSpd, tickCounterDir;

        public float windDirection;

        public float windDirectionRad;
        public float windSpeed;

        public MapComponent_WindSpeed(Map map) : base(map)
        {
        }

        private float GetNewWindDirection()
        {
            var rnd = new Random();
            var newDir = windDirection + (((rnd.Next(2) * 2) - 1) * rnd.Next(5));
            if (newDir > 359)
            {
                newDir = 360 - newDir;
            }
            else if (newDir < 0)
            {
                newDir = 360 + newDir;
            }

            return newDir;
        }

        public override void MapComponentTick()
        {
            if (tickCounterSpd > WIND_SPEED_TICKS)
            {
                windSpeed = map.windManager.WindSpeed;
                tickCounterSpd = 0;
                if (tickCounterDir <= WIND_DIRECTION_TICKS)
                {
                    return;
                }

                windDirection = GetNewWindDirection();
                windDirectionRad = (float) (Math.PI * windDirection / 180f);
                tickCounterDir = 0;
            }
            else
            {
                tickCounterSpd++;
                tickCounterDir++;
            }
        }
    }
}