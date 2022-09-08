using RimWorld;
using Verse;

namespace LHM
{
    [DefOf]
    class LHM_HediffDefOf
    {
        public static HediffDef RegrowingBodypart;

        static LHM_HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LHM_HediffDefOf));
        }
    }
}
