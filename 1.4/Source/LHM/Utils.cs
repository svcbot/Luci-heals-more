using RimWorld;
using Verse;

namespace LHM
{
    static class Utils
    {
        public static float HungerRate(Pawn pawn)
        {
            return pawn.ageTracker.CurLifeStage.hungerRateFactor 
                * pawn.RaceProps.baseHungerRate 
                * pawn.health.hediffSet.HungerRateFactor 
                * ((pawn.story == null || pawn.story.traits == null) ? 1f : pawn.story.traits.HungerRateFactor) 
                * pawn.GetStatValue(StatDefOf.RawNutritionFactor);
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
