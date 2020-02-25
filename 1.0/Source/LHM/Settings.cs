using UnityEngine;
using Verse;

namespace LHM
{
    class Settings : ModSettings
    {
        public bool showAgingMessages = false;
        public bool debugHealingSpeed = false;

        public static Settings Get()
        {
            return LoadedModManager.GetMod<LHM.Mod>().GetSettings<Settings>();
        }

        public void DoWindowContents(Rect wrect)
        {
            var options = new Listing_Standard();
            options.Begin(wrect);

            options.CheckboxLabeled("Show aging messages", checkOn: ref showAgingMessages, tooltip: "Show notification every time age was affected by luci");
            options.CheckboxLabeled("Debug luci healing", checkOn: ref debugHealingSpeed, tooltip: "Luci heal procs way more often");

            options.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref showAgingMessages, "showAgingMessages", false);
#if DEBUG
            Scribe_Values.Look(ref debugHealingSpeed, "debugHealingSpeed", false);
#endif
        }
    }

}
