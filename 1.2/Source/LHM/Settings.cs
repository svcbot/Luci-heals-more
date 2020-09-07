using UnityEngine;
using Verse;

namespace LHM
{
    class Settings : ModSettings
    {
        private bool showAgingMessages = false;
        private bool shouldAffectAge = true;
        private bool healTraumaSavant = false;
        private bool enableDebugHealingSpeed = false;
        private bool enableRegrowingBodyParts = false;

        public bool ShowAgingMessages => showAgingMessages;
        public bool ShouldAffectAge => shouldAffectAge;
        public bool HealTraumaSavant => healTraumaSavant;
        public bool EnableDebugHealingSpeed => enableDebugHealingSpeed;
        public bool EnableRegrowingBodyParts => enableRegrowingBodyParts;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<LHM.Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect canvas)
        {
            var options = new Listing_Standard();
            
            options.Begin(canvas);

            options.Gap();

            options.CheckboxLabeled("Affect biological age", checkOn: ref shouldAffectAge, tooltip: "Reduce or accelerate biological age towards 25 years. In animals the fixed point is the start of the third stage."); 
            options.CheckboxLabeled("Show aging messages", checkOn: ref showAgingMessages, tooltip: "Show notification every time age was affected by luci.");
            options.CheckboxLabeled("Allow healing trauma savant", checkOn: ref healTraumaSavant, tooltip: "Double edged sword. Most of the time people don't want to heal it, until the best trader who also keeps the mood in the colony high is affected.");

            options.Gap();

            options.CheckboxLabeled("Debug luci healing", checkOn: ref enableDebugHealingSpeed, tooltip: "Luci heal procs every hour.");

            options.End();

        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showAgingMessages, "showAgingMessages", false);
            Scribe_Values.Look(ref shouldAffectAge, "shouldAffectAge", true);
            Scribe_Values.Look(ref healTraumaSavant, "healTraumaSavant", false);
            Scribe_Values.Look(ref enableDebugHealingSpeed, "debugHealingSpeed", false);
            Scribe_Values.Look(ref enableRegrowingBodyParts, "debugHealingSpeed", false);

        }
    }

}
