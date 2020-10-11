using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace LHM
{
    class Hediff_RegrowingBodyPart : Hediff_Injury
    {
        private const int healInterval = GenDate.TicksPerDay / healsPerDay;
        private const int healsPerDay = 6;

        private const float healPercentPerDay = 0.03f;
        private const float meanHeal = healPercentPerDay / healsPerDay;
        private const float healDeviation = meanHeal / 2f;

        private int ticksUntilNextHeal;

        private float healAmount => base.Part.def.GetMaxHealth(pawn) * Rand.Gaussian(meanHeal, healDeviation);

        private float hpPercent => Severity / base.Part.def.GetMaxHealth(pawn);

        public override bool ShouldRemove => Severity <= 0.001f;

        public override void PostAdd(DamageInfo? dinfo)
        {
            Severity = Part.def.GetMaxHealth(pawn) - 1f;
            
            CurStage.restFallFactorOffset = Part.def.GetMaxHealth(pawn) / 100f;
            CurStage.hungerRateFactorOffset = Part.def.GetMaxHealth(pawn) / 100f;
            HediffComp_GetsPermanent permanentComp = (HediffComp_GetsPermanent)comps.Find(comp => comp is HediffComp_GetsPermanent);
            permanentComp.IsPermanent = true;
        }

        public override float PainOffset => (float)(Math.Pow(hpPercent, 2) * Math.Sqrt(Part.def.GetMaxHealth(pawn)) / 100d);

        public override float BleedRate => 0f;

        public override Color LabelColor
        {
            get
            {
                return new Color(0.2f, 0.8f, 0.2f);
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            pawn.health.RestorePart(base.Part);
            Messages.Message($"{Part.def.label} regrown", MessageTypeDefOf.PositiveEvent, true);
            
        }

        public override void Tick()
        {
            base.Tick();
            if (Current.Game.tickManager.TicksGame >= ticksUntilNextHeal)
            {
                Severity -= healAmount;
                SetNextTick();
            }
        }

        public void SetNextTick()
        {
            ticksUntilNextHeal = Settings.Get().EnableDebugHealingSpeed
                ? Current.Game.tickManager.TicksGame + GenDate.TicksPerHour / 4
                : Current.Game.tickManager.TicksGame + healInterval;
        }

    }

}
