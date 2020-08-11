using Verse;

namespace LHM
{
    class HediffComp_RegrowingBodyPart : HediffComp_TendDuration
    {
        public HediffCompProperties_RegrowingBodyPart Props => (HediffCompProperties_RegrowingBodyPart)props;

        //public bool isPermanentInt = false;

        //public bool IsPermanent => false;

        public new bool IsTended => true;

        public new bool AllowTend => true;

        public override bool CompShouldRemove => true;

        public PainCategory PainCategory => PainCategory.Painless;

        public override void CompPostInjuryHeal(float amount)
        {
            return;
        }
    }

}
