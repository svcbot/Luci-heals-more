using RimWorld;
using Verse;

namespace LHM
{
    static class Utils
    {
        public static float HungerRate(Pawn pawn)
        {
            return pawn.ageTracker.CurLifeStage.hungerRateFactor 
                + (pawn.RaceProps.baseHungerRate - 1)
                + (pawn.health.hediffSet.HungerRateFactor -1f)
                + ((pawn.story == null || pawn.story.traits == null) ? 0f : pawn.story.traits.HungerRateFactor - 1f);
        }

        public static BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart missingPartsCommonAncestor in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (
                    !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(missingPartsCommonAncestor.Part) &&
                    (bodyPartRecord == null || missingPartsCommonAncestor.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren))
                {
                    bodyPartRecord = missingPartsCommonAncestor.Part;
                }
            }
            return bodyPartRecord;
        }
    }
}
