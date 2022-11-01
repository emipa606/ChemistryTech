using HarmonyLib;
using Verse;

namespace CloudCore;

[StaticConstructorOnStartup]
internal static class WindSpeed
{
    static WindSpeed()
    {
        var harmony = new Harmony("rimworld.bb.windspeed");

        var targetMethod = AccessTools.Method(typeof(WindManager), "WindManagerTick");
        var postfixMethod = new HarmonyMethod(typeof(WindSpeed).GetMethod("WindTick_Postfix"));
        harmony.Patch(targetMethod, null, postfixMethod);
    }

    public static void WindTick_Postfix(WindManager __instance)
    {
        // Log.Message (__instance.WindSpeed.ToString());
    }
}