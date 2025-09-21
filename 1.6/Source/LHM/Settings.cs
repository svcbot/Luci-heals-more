using UnityEngine;
using Verse;

namespace LHM
{
    class Settings : ModSettings
    {
        private bool showAgingMessages = false;
        private bool shouldReduceAge = true;
        private bool shouldIncreaseAge = false;
        private bool healTraumaSavant = false;
        private bool enableDebugHealingSpeed = false;
        private bool enableRegrowingBodyParts = false;
        private float hungerRateTreshold = 200f;
        private int optimalAge = 21;

        public bool ShowAgingMessages => showAgingMessages;
        public bool ShouldReduceAge => shouldReduceAge;
        public bool ShouldIncreaseAge => shouldIncreaseAge;
        public bool HealTraumaSavant => healTraumaSavant;
        public bool EnableDebugHealingSpeed => enableDebugHealingSpeed;
        public bool EnableRegrowingBodyParts => enableRegrowingBodyParts;
        public float HungerRateTreshold => hungerRateTreshold;
        public int OptiomalAge => optimalAge;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<LHM.Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect canvas)
        {
            var options = new Listing_Standard();
            
            options.Begin(canvas);

            options.Gap();

            options.Label("Optimal age: " + OptiomalAge);
            optimalAge = (int)options.Slider(OptiomalAge, 1, 100);
            options.CheckboxLabeled("Reduce biological age", checkOn: ref shouldReduceAge, tooltip: "Reduce or accelerate biological age towards optimal age. In animals the fixed point is the start of the third stage."); 
            options.CheckboxLabeled("Increase biological age", checkOn: ref shouldIncreaseAge, tooltip: "Increase biological age towards optimal age. In animals the fixed point is the start of the third stage."); 
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

                //options.Gap();
                //options.Label("Healing power");
                //options.RadioButton("Vanilla", true, tooltip: "Comparable to vanilla logic. A full limb will take 15 to 30 days to regrow.");
                //options.RadioButton("More healing", false, tooltip: "3x healing power compared to vanila. A full limb will take 5 to 15 days to regrow.");
                //options.RadioButton("OP", false, tooltip: "OP healing, not recomended. A full limb will take 1 to 3 days to regrow.");
                //options.RadioButton("God mode", false, tooltip: "x32 healing, useful for debug purposes. A full limb will take a few hours.");

            }

            options.Gap();
            options.GapLine();
            options.Gap();

            options.Label("Debug settings");
            options.Gap();

            options.CheckboxLabeled("Debug luci healing", checkOn: ref enableDebugHealingSpeed, tooltip: "Luci heal procs much more often.");
            options.CheckboxLabeled("Show aging messages", checkOn: ref showAgingMessages, tooltip: "Show notification every time age was affected by luci.");

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showAgingMessages, "showAgingMessages", false, true);
            Scribe_Values.Look(ref shouldReduceAge, "shouldAffectAge", true, true);
            Scribe_Values.Look(ref healTraumaSavant, "healTraumaSavant", false, true);
            Scribe_Values.Look(ref enableDebugHealingSpeed, "debugHealingSpeed", false, true);
            Scribe_Values.Look(ref enableRegrowingBodyParts, "enableRegrowingBodyParts", false, true);
            Scribe_Values.Look(ref hungerRateTreshold, "HungerRateTreshold", 200f, true);
            Scribe_Values.Look(ref optimalAge, "optimalAge", 21, true);
        }
    }

}
