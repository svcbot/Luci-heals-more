using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace LHM
{
    class Hediff_RegrowingBodyPart : Hediff_Injury
    {
        private const int healInterval = 500;

        private int ticksUntilNextHeal;

        private readonly float healPerDay = 2f;

        private float healAmount => healPerDay / (GenDate.TicksPerDay / healInterval);

        public override bool ShouldRemove => Severity <= 0.001f;

        public override void PostAdd(DamageInfo? dinfo)
        {
            Severity = Part.def.GetMaxHealth(pawn) - 1;
            CurStage.restFallFactorOffset = Part.def.GetMaxHealth(pawn) / 100;
            HediffComp_GetsPermanent permanentComp = (HediffComp_GetsPermanent)comps.Find(comp => comp is HediffComp_GetsPermanent);
            permanentComp.IsPermanent = true;
        }

        public override float PainOffset => Severity / base.Part.def.GetMaxHealth(pawn) * 0.1f;

        public override float BleedRate => 0f;

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
            ticksUntilNextHeal = Current.Game.tickManager.TicksGame + healInterval;
        }

    }

}
