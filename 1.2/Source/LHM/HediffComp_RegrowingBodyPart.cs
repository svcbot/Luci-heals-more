using System;
using RimWorld;
using Verse;

namespace LHM
{
    class HediffComp_RegrowingBodyPart : Hediff_AddedPart
    {
        public override bool ShouldRemove => Severity <= 0.1f;

        public int ticksUntilNextHeal;

        public override void Tick()
        {
            base.Tick();
            if (Current.Game.tickManager.TicksGame >= ticksUntilNextHeal)
            {
                Severity -= 0.1f;
                SetNextTick();
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            if (Severity >= 1f)
            {
                pawn.health.RestorePart(base.Part);

                Messages.Message("Limb regrown", MessageTypeDefOf.PositiveEvent, true);
            }
        }

        public void SetNextTick()
        {
            ticksUntilNextHeal = Current.Game.tickManager.TicksGame + 500;
        }

    }

}
