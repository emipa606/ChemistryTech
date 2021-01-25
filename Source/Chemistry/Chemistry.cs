namespace Chemistry
{
    /*
	[StaticConstructorOnStartup]
    static class FartClass
    {

		static FartClass ()
		{
			HarmonyInstance harmony = HarmonyInstance.Create ("rimworld.chemistry");
			MethodInfo targetMethod = AccessTools.Method (typeof(RimWorld.IngestionOutcomeDoer), "DoIngestionOutcome");
			HarmonyMethod postfixMethod = new HarmonyMethod (typeof(Chemistry.FartClass).GetMethod ("Create_fart_postfix"));
			harmony.Patch (targetMethod, null, postfixMethod);
		}

		public static void Create_fart_postfix (Pawn pawn, Thing ingested)
		{
			ThingDef thingDef = DefDatabase<ThingDef>.GetNamed("Fart_Gas", true);
			Thing thing = (Thing) ThingMaker.MakeThing(thingDef, null);

			Log.Message ("farting hell");

			SoundDef.Named ("Fart_Event").PlayOneShot (new TargetInfo (pawn.Position, pawn.Map, false));
			GenSpawn.Spawn(thing, pawn.Position, pawn.Map);
		}

    }*/
}