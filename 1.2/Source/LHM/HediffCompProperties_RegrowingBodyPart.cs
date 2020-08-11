using Verse;

namespace LHM
{
    class HediffCompProperties_RegrowingBodyPart : HediffCompProperties_TendDuration
    {
        public HediffCompProperties_RegrowingBodyPart()
        {
            compClass = typeof(HediffComp_RegrowingBodyPart);
        }
        
        public new bool TendIsPermanent => true;

        public new bool showTendQuality = true;
    }
}
