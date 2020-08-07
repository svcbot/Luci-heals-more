using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace LHM
{
    public class HediffComp_HealPermanentWounds : HediffComp
    {
        private const int TicksInDay = 60000;
        private const float MinimalHealthAmount = 0.01f;

        private int ticksToHeal;

        public HediffCompProperties_HealPermanentWounds Props => (HediffCompProperties_HealPermanentWounds)props;

        public HashSet<string> AdditionalHedifsToHeal { get; set; } = new HashSet<string>()
        {
            "TraumaSavant", "ChemicalDamageSevere", "ChemicalDamageModerate", "Cirrhosis"
        };

        public HediffComp_HealPermanentWounds()
        {
            if(ticksToHeal > 6 * TicksInDay) ResetTicksToHeal();
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            // next heal event will happen after an hour in the debug mode or after 4 to 6 days (uniformly distributed) normaly
            ticksToHeal = Settings.Get().debugHealingSpeed ? 2500 : Rand.Range(4 * TicksInDay, 6 * TicksInDay); 
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal >= 6 * TicksInDay) ResetTicksToHeal();
            else if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                AffectPawnsAge();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            var selectHediffsQuery = from hd in Pawn.health.hediffSet.hediffs
                                     where hd.IsPermanent() || hd.def.chronic || AdditionalHedifsToHeal.Contains(hd.def.defName)
                                     select hd;

            if (selectHediffsQuery.TryRandomElement(out Hediff hediff))  
            {                
                float meanHeal = 0.2f;
                float rndHealPercentValue = meanHeal + (Rand.Gaussian() * meanHeal / 2f); // heal % is normaly distributed between 10 % and 30 %

                float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                float rawHealAmount = hediff.IsPermanent() ? bodyPartMaxHP * rndHealPercentValue : rndHealPercentValue;
                float healAmount = (rawHealAmount < MinimalHealthAmount) ? MinimalHealthAmount : rawHealAmount;

                if (hediff.Severity - healAmount < MinimalHealthAmount) HandleLowSeverity(hediff);
                else hediff.Severity -= healAmount;
            }
        }

        private void HandleLowSeverity(Hediff hediff)
        {
            if (hediff.IsPermanent()) hediff.Severity = 0f;
            else Pawn.health.RemoveHediff(hediff);

            if (PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("MessagePermanentWoundHealed".Translate(
                        parent.LabelCap,
                        Pawn.LabelShort,
                        hediff.Label,
                        Pawn.Named("PAWN")
                    ),
                    Pawn, MessageTypeDefOf.PositiveEvent, true
                );
            }
        }

        private void AffectPawnsAge()
        {
            if (Pawn.RaceProps.Humanlike)
            {
                if (Pawn.ageTracker.AgeBiologicalYears > 25) ReduceAgeOfHumanlike();
                else if (Pawn.ageTracker.AgeBiologicalYears < 25) Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * TicksInDay); // get one quadrum older
            }
            else // if not humanlike then optimal age is the start of the third stage
            {
                int lifeStage = Pawn.ageTracker.CurLifeStageIndex;
                long startOfThirdStage = (long)(Pawn.RaceProps.lifeStageAges[2].minAge * 60 * TicksInDay);
                long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;

                if (lifeStage >= 3 && diffFromOptimalAge > 0) // then need to become younger
                {
                    Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * TicksInDay); // get 5 days older
                }
            }

        }

        private void ReduceAgeOfHumanlike()
        {
            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out int biologicalYears, out int biologicalQuadrums, out int biologicalDays, out float biologicalHours);

            string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
            long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - (25 * 60 * TicksInDay);
            Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);

            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out biologicalHours);
            string ageAfter = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);

            if (Pawn.IsColonist && Settings.Get().showAgingMessages)
            {
                Messages.Message("MessageAgeReduced".Translate(
                        Pawn.LabelShort,
                        ageBefore,
                        ageAfter
                    ),
                    MessageTypeDefOf.PositiveEvent, true
                );
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }

}

