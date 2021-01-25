using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace CloudCore
{
    public class GasTestBuilding : Building
	{
		public GasTestBuilding ()
		{
		}

		//private int spawnGasTick = 0;

		private void Spawn_Gas () {
			ThingDef thingDef = DefDatabase<ThingDef>.GetNamed ("Helium_Gas", true);
			var thing = (Thing)ThingMaker.MakeThing (thingDef, null);
			IntVec3 loc = Position;
			Map map = Map;

			GenSpawn.Spawn (thing, loc, map);
		}

		public override string GetInspectString()
		{
			var stringBuilder = new StringBuilder();
			var cws = (MapComponent_WindSpeed)Find.CurrentMap.GetComponent (typeof(MapComponent_WindSpeed));
			if (cws != null) {
				stringBuilder.AppendLine("Speed: " + cws.windSpeed);
				stringBuilder.AppendLine("Direction: " + cws.windDirection);
				stringBuilder.AppendLine("Direction rad: " + cws.windDirectionRad);
			}
			stringBuilder.AppendLine(base.GetInspectString());
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override IEnumerable<Gizmo> GetGizmos ()
		{
			yield return new Command_Action {
				action = new Action (Spawn_Gas),
				defaultLabel = "Spawn Gas",
				defaultDesc = "",
				icon = ContentFinder<Texture2D>.Get ("Things/Gas/Puff", true)
			};
		}

		public override void Tick ()
		{
			/*if (this.spawnGasTick > 720) {				
				this.spawnGasTick = 0;
				this.Spawn_Gas ();
			}
			this.spawnGasTick++;*/
			base.Tick ();
		}
	}
}

