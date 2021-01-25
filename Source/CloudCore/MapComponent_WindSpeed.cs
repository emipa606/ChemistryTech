using System;
using Verse;

namespace CloudCore
{
    public class MapComponent_WindSpeed : MapComponent
	{
		public float windSpeed;

		public float windDirection = 0f;

		public float windDirectionRad = 0f;

		private int tickCounterSpd = 0, tickCounterDir = 0;

		private const int WIND_SPEED_TICKS = 60;

		private const int WIND_DIRECTION_TICKS = 300;

		public MapComponent_WindSpeed (Map map) : base (map)
		{

		}

		private float GetNewWindDirection ()
		{
			var rnd = new System.Random ();
			var newDir = windDirection + (((rnd.Next (2) * 2) - 1) * rnd.Next (5));
			if (newDir > 359) {
				newDir = 360 - newDir;
			} else if (newDir < 0) {
				newDir = 360 + newDir;
			}
			return newDir;
		}

		public override void MapComponentTick ()
		{			
			if (tickCounterSpd > WIND_SPEED_TICKS) {
				windSpeed = map.windManager.WindSpeed;
				tickCounterSpd = 0;
				if (tickCounterDir > WIND_DIRECTION_TICKS) {
					windDirection = GetNewWindDirection ();
					windDirectionRad = (float)(Math.PI * windDirection / 180f);
					tickCounterDir = 0;
				}
			} else {
				tickCounterSpd++;
				tickCounterDir++;
			}
		}

	}
}

