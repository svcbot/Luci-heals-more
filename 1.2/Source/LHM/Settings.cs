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
        private float hungerRateTreshold = 150f;
        private int opLevel = 0;

        public bool ShowAgingMessages => showAgingMessages;
        public bool ShouldAffectAge => shouldAffectAge;
        public bool HealTraumaSavant => healTraumaSavant;
        public bool EnableDebugHealingSpeed => enableDebugHealingSpeed;
        public bool EnableRegrowingBodyParts => enableRegrowingBodyParts;

        public float HungerRateTreshold => hungerRateTreshold;
        public int OPLevel => opLevel;

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
            options.CheckboxLabeled("Allow healing trauma savant", checkOn: ref healTraumaSavant, tooltip: "Double edged sword. Most of the time people don't want to heal it, until the best trader who also keeps the mood in the colony high is affected.");

            options.Gap();

            options.CheckboxLabeled(
                "Enable regrowing bodayparts", 
                checkOn: ref enableRegrowingBodyParts, 
                tooltip: "Allow luciferium effect to apply regrowing status to missing bodayparts. Regrowing effect heals slower than luciferium healing, and applies small pain offset, hunger rate and increased rate of rest fall."
            );


            if (enableRegrowingBodyParts)
            {
                options.Gap();
                //hungerRateTreshold = Widgets.HorizontalSlider(canvas, hungerRateTreshold, 100f, 200f, true, "Hunger rate treshold: " + HungerRateTreshold, null, null, 1);
                options.Label("Hunger rate treshold: " + (int)HungerRateTreshold);
                hungerRateTreshold = options.Slider(hungerRateTreshold, 100f, 200f);

                options.Gap();
                options.Label("Work in progress: option to regrow only certain body parts e.g. fingers and toes, will be added in the next update.");
            }

            options.Gap();
            options.GapLine();
            options.Gap();

            options.CheckboxLabeled("Debug luci healing", checkOn: ref enableDebugHealingSpeed, tooltip: "Luci heal procs much more often.");
            options.CheckboxLabeled("Show aging messages", checkOn: ref showAgingMessages, tooltip: "Show notification every time age was affected by luci.");

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showAgingMessages, "showAgingMessages", false);
            Scribe_Values.Look(ref shouldAffectAge, "shouldAffectAge", true);
            Scribe_Values.Look(ref healTraumaSavant, "healTraumaSavant", false);
            Scribe_Values.Look(ref enableDebugHealingSpeed, "debugHealingSpeed", false);
            Scribe_Values.Look(ref enableRegrowingBodyParts, "enableRegrowingBodyParts", false);
            Scribe_Values.Look(ref hungerRateTreshold, "HungerRateTreshold", 150f);
        }
    }

}
