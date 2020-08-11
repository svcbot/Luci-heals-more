using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace LHM
{
    class Hediff_RegrowingBodyPart : Hediff_Injury
    {
        public override bool ShouldRemove => Severity <= 0.001f;

        public override void PostAdd(DamageInfo? dinfo)
        {
            Severity = Part.def.GetMaxHealth(pawn) - 1;
            CurStage.restFallFactorOffset = Part.def.GetMaxHealth(pawn) / 100;
            
        }

        public override float PainOffset => Severity / base.Part.def.GetMaxHealth(pawn) * 0.1f;

        public override float BleedRate => 0f;

        //public bool IsFresh => false;

        public bool IsTended => true;

        private int ticksUntilNextHeal;

        public override Color LabelColor
        {
            get
            {
                return new Color(0.2f, 0.8f, 0.2f);
            }
        }

        public override float SummaryHealthPercentImpact
        {
            get
            {
                return Severity / (75f * pawn.HealthScale);
            }
        }

        public override void Heal(float amount)
        {
            return;
        }

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
            pawn.health.RestorePart(base.Part);
            Messages.Message($"{Part.def.label} regrown", MessageTypeDefOf.PositiveEvent, true);
            
        }

        public void SetNextTick()
        {
            ticksUntilNextHeal = Current.Game.tickManager.TicksGame + GenDate.TicksPerHour;
        }

    }

}
