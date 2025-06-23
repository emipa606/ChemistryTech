using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace CloudCore;

public class GasTestBuilding : Building
{
    //private int spawnGasTick = 0;

    private void Spawn_Gas()
    {
        var thingDef = DefDatabase<ThingDef>.GetNamed("Helium_Gas");
        var thing = ThingMaker.MakeThing(thingDef);
        var loc = Position;
        var map = Map;

        GenSpawn.Spawn(thing, loc, map);
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        var cws = (MapComponent_WindSpeed)Find.CurrentMap.GetComponent(typeof(MapComponent_WindSpeed));
        if (cws != null)
        {
            stringBuilder.AppendLine($"Speed: {cws.WindSpeed}");
            stringBuilder.AppendLine($"Direction: {cws.WindDirection}");
            stringBuilder.AppendLine($"Direction rad: {cws.WindDirectionRad}");
        }

        stringBuilder.AppendLine(base.GetInspectString());
        return stringBuilder.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            action = Spawn_Gas,
            defaultLabel = "Spawn Gas",
            defaultDesc = "",
            icon = ContentFinder<Texture2D>.Get("Things/Gas/Puff")
        };
    }
}