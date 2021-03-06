using Verse;

namespace LHM
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content) => GetSettings<Settings>();

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<Settings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Luci heals more!";
        }
    }

}