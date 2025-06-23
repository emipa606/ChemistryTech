using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace CloudCore;

public class Core_Gas : Thing
{
    private const int MoveTicks = 1;
    private const int CheckTicks = 5;
    private const int WindSpeedReduction = 20;
    private const int HitTicks = 120;
    private int checkGasTick;
    private int destroyTick;

    private Vector3 drawPosition;

    private float graphicRotation;

    private float graphicRotationSpeed;
    private int hitTick;
    private bool isStopped;

    private int moveGasTick;
    private Random rndUtil;
    private List<Pawn> touchingPawns = [];

    private MapComponent_WindSpeed wsUtil;

    public Core_Gas()
    {
        wsUtil = null;
        rndUtil = null;
    }

    private void Move_Gas(bool check)
    {
        if (wsUtil == null || Map == null || rndUtil == null)
        {
            return;
        }

        if (isStopped && !check)
        {
            return;
        }

        var isOutdoors = Position.UsesOutdoorTemperature(Map);
        isStopped = false;
        var newPos = drawPosition;
        var rndWindSpeed = (float)rndUtil.NextDouble() / 5;
        float windDirection;
        float windSpeed;
        if (isOutdoors)
        {
            // direction
            var modifier = rndUtil.Next(19) - 10;
            windDirection = wsUtil.WindDirectionRad + (modifier * (float)Math.PI / 180f);
            // speed
            var modifierSpeed = rndUtil.Next(19) - 10;
            windSpeed = wsUtil.WindSpeed + rndWindSpeed;
            windSpeed /= WindSpeedReduction + modifierSpeed;
        }
        else
        {
            windDirection = (float)rndUtil.Next(628) / 100;
            windSpeed = rndWindSpeed / 2;
        }

        newPos.x += (float)Math.Cos(windDirection) * windSpeed;
        newPos.z += (float)Math.Sin(windDirection) * windSpeed;
        var newPosInt = new IntVec3((int)Math.Round(newPos.x), (int)Math.Round(newPos.y),
            (int)Math.Round(newPos.z));

        // try to find gas on location
        var thingList = newPosInt.GetThingList(Map);
        if (thingList != null)
        {
            foreach (var thing in thingList)
            {
                if (thing is not Core_Gas)
                {
                    continue;
                }

                windDirection = (float)rndUtil.Next(628) / 100;
                windSpeed = rndWindSpeed / 2;

                newPos.x += (float)Math.Cos(windDirection) * windSpeed;
                newPos.z += (float)Math.Sin(windDirection) * windSpeed;
                newPosInt = new IntVec3((int)Math.Round(newPos.x), (int)Math.Round(newPos.y),
                    (int)Math.Round(newPos.z));
            }
        }

        if (newPosInt != Position)
        {
            if (!newPosInt.InBounds(Map))
            {
                isStopped = true;
                Destroy();
            }
            else
            {
                var th = newPosInt.GetRoofHolderOrImpassable(Map);
                if (th != null && th is not Building_Vent)
                {
                    isStopped = true;
                }
            }
        }

        if (isStopped)
        {
            return;
        }

        drawPosition = newPos;
        Position = newPosInt;
    }


    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        var mesh = MeshPool.GridPlane(def.graphicData.drawSize);
        Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(graphicRotation, Vector3.up), def.DrawMatSingle,
            0);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        /*Gas gasOnPos = this.Position.GetGas (map);
        var chemistry_Gas = gasOnPos as Core_Gas;
        if (chemistry_Gas != null) {
            this.intensity += chemistry_Gas.intensity;
        }*/
        base.SpawnSetup(map, respawningAfterLoad);
        rndUtil = new Random(thingIDNumber);
        destroyTick = Find.TickManager.TicksGame + def.gas.expireSeconds.RandomInRange.SecondsToTicks();
        graphicRotationSpeed = Rand.Range(-def.gas.rotationSpeed, def.gas.rotationSpeed) / 60;
        var exactPos = this.TrueCenter();
        exactPos.x += (float)rndUtil.NextDouble() - 0.5f;
        exactPos.z += (float)rndUtil.NextDouble() - 0.5f;
        drawPosition = exactPos;
        wsUtil = (MapComponent_WindSpeed)Map.GetComponent(typeof(MapComponent_WindSpeed));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref destroyTick, "destroyTick");
        Scribe_Collections.Look(ref touchingPawns, "testees", LookMode.Reference);
    }

    private void ApplyGas(Pawn p)
    {
        // get damage def
        var dDef = DefDatabase<DamageDef>.GetNamed(def.projectile.damageDef.ToString());

        var preventGas = false;

        if (p.apparel != null)
        {
            var wornApparel = p.apparel.WornApparel;
            foreach (var item in wornApparel)
            {
                var ext = item.def.GetModExtension<GasProtectionExtension>() ??
                          GasProtectionExtension.defaultValues;

                if (ext.efficiency > 0)
                {
                    preventGas = true;
                }
            }
        }

        if (!preventGas)
        {
            Log.Message("gas not prevented");
            if (def.projectile.GetDamageAmount(1f, this) > 0)
            {
                // gas does physical damage
                // generate injury and add it to pawn
                var num = def.projectile.GetDamageAmount(1f, this);
                var height = Rand.Value >= 0.666 ? BodyPartHeight.Middle : BodyPartHeight.Top;
                float armorPenetration = 0;

                var dinfo = new DamageInfo(dDef, num, armorPenetration, -1, this);

                dinfo.SetBodyRegion(height, BodyPartDepth.Outside);

                p.TakeDamage(dinfo);
            }
            else
            {
                // gas does no physical damage
                // get hediff def
                var hDef = DefDatabase<HediffDef>.GetNamed(dDef.hediff.ToString());

                // add hediff to pawn
                p.health.AddHediff(HediffMaker.MakeHediff(hDef, p));
            }
        }
        else
        {
            Log.Message("gas prevented");
        }
    }

    protected override void Tick()
    {
        if (!Spawned)
        {
            return;
        }

        if (destroyTick <= Find.TickManager.TicksGame)
        {
            Destroy();
        }
        else
        {
            graphicRotation += graphicRotationSpeed + rndUtil.Next(5) - 3;

            if (hitTick > HitTicks)
            {
                var thingList = Position.GetThingList(Map);
                if (thingList != null)
                {
                    foreach (var thing in thingList)
                    {
                        if (thing is not Pawn pawn || touchingPawns.Contains(pawn) || pawn.Dead)
                        {
                            continue;
                        }

                        touchingPawns.Add(pawn);
                        ApplyGas(pawn);
                        hitTick = 0;
                    }
                }

                for (var j = 0; j < touchingPawns.Count; j++)
                {
                    var pawn2 = touchingPawns[j];
                    if (!pawn2.Spawned || pawn2.Position != Position)
                    {
                        touchingPawns.Remove(pawn2);
                    }
                }
            }
            else
            {
                hitTick++;
            }

            if (moveGasTick > MoveTicks)
            {
                moveGasTick = 0;
                var check = false;
                if (checkGasTick > CheckTicks)
                {
                    checkGasTick = 0;
                    check = true;
                }

                Move_Gas(check);
            }

            moveGasTick++;
            checkGasTick++;
        }
    }
}