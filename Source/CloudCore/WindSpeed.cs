using Harmony;
using UnityEngine;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace CloudCore
{
	[StaticConstructorOnStartup]
	static class WindSpeed
	{
		static WindSpeed ()
		{
			HarmonyInstance harmony = HarmonyInstance.Create ("rimworld.bb.windspeed");
			MethodInfo targetMethod;
			HarmonyMethod postfixMethod;

			targetMethod = AccessTools.Method (typeof(Verse.WindManager), "WindManagerTick");
			postfixMethod = new HarmonyMethod (typeof(CloudCore.WindSpeed).GetMethod ("WindTick_Postfix"));
			harmony.Patch (targetMethod, null, postfixMethod);
		}

		public static void WindTick_Postfix(Verse.WindManager __instance)
		{
			// Log.Message (__instance.WindSpeed.ToString());
		}
	}
}