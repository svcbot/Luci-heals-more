using RimWorld;
using Verse;

namespace LHM
{
    class HediffComp_RegrowingBodyPart : Hediff_AddedPart
    {
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (Severity >= 1f)
            {
                pawn.health.RestorePart(base.Part);

                Messages.Message("Limb regrown", MessageTypeDefOf.PositiveEvent, true);
            }
        }

    }

}
