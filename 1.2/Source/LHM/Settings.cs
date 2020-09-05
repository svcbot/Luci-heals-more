using UnityEngine;
using Verse;

namespace LHM
{
    class Settings : ModSettings
    {
        public bool showAgingMessages;
        public bool shouldAffectAge;
        public bool healTraumaSavant;
        public bool enableDebugHealingSpeed;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<LHM.Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            options.CheckboxLabeled("Affect biological age", checkOn: ref shouldAffectAge, tooltip: "Reduce or accelerate biological age towards 25 years. In animals the fixed point is the start of the third stage."); 
            options.CheckboxLabeled("Show aging messages", checkOn: ref showAgingMessages, tooltip: "Show notification every time age was affected by luci.");
            options.CheckboxLabeled("Allow healing trauma savant", checkOn: ref healTraumaSavant, tooltip: "Double edged sword. Most of the time people don't want to heal it, until the best trader who also keeps the mood in the colony high is affected.");
            options.CheckboxLabeled("Debug luci healing", checkOn: ref enableDebugHealingSpeed, tooltip: "Luci heal procs every hour.");

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showAgingMessages, "showAgingMessages", false);
            Scribe_Values.Look(ref shouldAffectAge, "shouldAffectAge", true);
            Scribe_Values.Look(ref healTraumaSavant, "healTraumaSavant", false);
#if DEBUG
            Scribe_Values.Look(ref enableDebugHealingSpeed, "debugHealingSpeed", false);
#endif
        }
    }

}
