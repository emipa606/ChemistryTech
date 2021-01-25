using UnityEngine;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace CloudCore
{
    public class Core_Gas : Thing
	{
		public int destroyTick;

		public float graphicRotation;

		public float graphicRotationSpeed;

		private const int MOVE_TICKS = 1;
		private const int CHECK_TICKS = 5;
		private const int WIND_SPEED_REDUCTION = 20;
		private const int HIT_TICKS = 120;

		private int moveGasTick = 0;
		private int checkGasTick = 0;
		private int hitTick = 0;
		private bool isStopped = false;

		private MapComponent_WindSpeed wsUtil;
		private System.Random rndUtil;

		private Vector3 drawPosition;
		public int intensity = 1;
		private List<Pawn> touchingPawns = new List<Pawn> ();

		public Core_Gas () : base ()
		{
			wsUtil = null;
			rndUtil = null;
		}

		private void Move_Gas (bool check)
		{
			if (wsUtil != null && Map != null && rndUtil != null) {
				if (!isStopped || check) {
					var isOutdoors = Position.UsesOutdoorTemperature (Map);
					isStopped = false;
					Vector3 newPos = drawPosition;
					var rndWindSpeed = (float)rndUtil.NextDouble () / 5;
					float windDirection;
					float windSpeed;
					if (isOutdoors) {
						// direction
						var modifier = rndUtil.Next (19) - 10;
						windDirection = wsUtil.windDirectionRad + ((float)modifier * (float)Math.PI / 180f);
						// speed
						var modifierSpeed = rndUtil.Next (19) - 10;
						windSpeed = wsUtil.windSpeed + rndWindSpeed;
						windSpeed /= WIND_SPEED_REDUCTION + modifierSpeed;
					} else {
						windDirection = (float)rndUtil.Next (628) / 100;
						windSpeed = rndWindSpeed / 2;
					}

					newPos.x += (float)Math.Cos (windDirection) * windSpeed;
					newPos.z += (float)Math.Sin (windDirection) * windSpeed;
					var newPosInt = new IntVec3 ((int)Math.Round (newPos.x), (int)Math.Round (newPos.y), (int)Math.Round (newPos.z));

					// try to find gas on location
					List<Thing> thingList = newPosInt.GetThingList (Map);
					if (thingList != null) {
						for (var i = 0; i < thingList.Count; i++) {
                            if (thingList[i] is Core_Gas otherGas)
                            {
                                windDirection = (float)rndUtil.Next(628) / 100;
                                windSpeed = rndWindSpeed / 2;

                                newPos.x += (float)Math.Cos(windDirection) * windSpeed;
                                newPos.z += (float)Math.Sin(windDirection) * windSpeed;
                                newPosInt = new IntVec3((int)Math.Round(newPos.x), (int)Math.Round(newPos.y), (int)Math.Round(newPos.z));
                            }
                        }
					}				

					if (newPosInt != Position) {
						if (!newPosInt.InBounds (Map)) {
							isStopped = true;
							Destroy (DestroyMode.Vanish);
						} else {
							Thing th = newPosInt.GetRoofHolderOrImpassable (Map);
							if (th != null && !(th is Building_Vent)) {
								isStopped = true;
							}
						}
					}
					if (!isStopped) {
						drawPosition = newPos;
						Position = newPosInt;
					}
				}
			}
		}

		public override void Draw ()
		{
			Mesh mesh = MeshPool.GridPlane (def.graphicData.drawSize);
			Graphics.DrawMesh (mesh, drawPosition, Quaternion.AngleAxis (graphicRotation, Vector3.up), def.DrawMatSingle, 0);
		}

		public override void SpawnSetup (Map map, bool respawningAfterLoad)
		{
			/*Gas gasOnPos = this.Position.GetGas (map);
			var chemistry_Gas = gasOnPos as Core_Gas;
			if (chemistry_Gas != null) {
				this.intensity += chemistry_Gas.intensity;
			}*/
			base.SpawnSetup (map, respawningAfterLoad);
			rndUtil = new System.Random (thingIDNumber);
			destroyTick = Find.TickManager.TicksGame + def.gas.expireSeconds.RandomInRange.SecondsToTicks ();
			graphicRotationSpeed = Rand.Range (-def.gas.rotationSpeed, def.gas.rotationSpeed) / 60;
			Vector3 exactPos = this.TrueCenter ();
			exactPos.x += (float)rndUtil.NextDouble () - 0.5f;
			exactPos.z += (float)rndUtil.NextDouble () - 0.5f;
			drawPosition = exactPos;
			wsUtil = (MapComponent_WindSpeed)Map.GetComponent (typeof(MapComponent_WindSpeed));
		}

		public override void ExposeData ()
		{
			base.ExposeData ();
			Scribe_Values.Look<int> (ref destroyTick, "destroyTick", 0, false);
			Scribe_Collections.Look<Pawn> (ref touchingPawns, "testees", LookMode.Reference, new object[0]);
		}

		private void ApplyGas (Pawn p)
		{
			// get damage def
			DamageDef dDef = DefDatabase<DamageDef>.GetNamed (def.projectile.damageDef.ToString (), true);

			var preventGas = false;

			if (p.apparel != null) {
				List<Apparel> wornApparel = p.apparel.WornApparel;
				foreach (Apparel item in wornApparel) {
					GasProtectionExtension ext = item.def.GetModExtension<GasProtectionExtension>() ??
						GasProtectionExtension.defaultValues;

					if (ext.efficiency > 0) {					
						preventGas = true;
					}
				}
			}

			if (!preventGas) {
				Log.Message ("gas not prevented");
				if (def.projectile.GetDamageAmount (1f) > 0) {
					// gas does physical damage
					// generate injury and add it to pawn
					var num = def.projectile.GetDamageAmount (1f);
					BodyPartHeight height = (Rand.Value >= 0.666) ? BodyPartHeight.Middle : BodyPartHeight.Top;
					float armorPenetration = 0;

					var dinfo = new DamageInfo (dDef, num, armorPenetration, -1, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null);

					dinfo.SetBodyRegion (height, BodyPartDepth.Outside);

					p.TakeDamage (dinfo);
				} else {
					// gas does no physical damage
					// get hediff def
					HediffDef hDef = DefDatabase<HediffDef>.GetNamed (dDef.hediff.ToString (), true);

					// add hediff to pawn
					p.health.AddHediff (HediffMaker.MakeHediff (hDef, p, null), null, null, null);
				}
			} else {
				Log.Message ("gas prevented");
			}
		}

		public override void Tick ()
		{
			if (Spawned) {
				if (destroyTick <= Find.TickManager.TicksGame) {
					Destroy (DestroyMode.Vanish);
				} else {
					graphicRotation += graphicRotationSpeed + rndUtil.Next (5) - 3;

					if (hitTick > HIT_TICKS) {
						List<Thing> thingList = Position.GetThingList (Map);
						if (thingList != null) {
							for (var i = 0; i < thingList.Count; i++) {
                                if (thingList[i] is Pawn pawn && !touchingPawns.Contains(pawn) && !pawn.Dead)
                                {
                                    touchingPawns.Add(pawn);
                                    ApplyGas(pawn);
                                    hitTick = 0;
                                }
                            }
						}
						for (var j = 0; j < touchingPawns.Count; j++) {
							Pawn pawn2 = touchingPawns [j];
							if (!pawn2.Spawned || pawn2.Position != Position) {
								touchingPawns.Remove (pawn2);
							}
						}
					} else {
						hitTick++;
					}
				
					if (moveGasTick > MOVE_TICKS) {				
						moveGasTick = 0;
						var check = false;
						if (checkGasTick > CHECK_TICKS) {
							checkGasTick = 0;
							check = true;
						}
						Move_Gas (check);
					}
					moveGasTick++;
					checkGasTick++;
				}
			}
		}
	}
}

