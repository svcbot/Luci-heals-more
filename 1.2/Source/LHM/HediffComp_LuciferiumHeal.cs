using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace LHM
{
    public class HediffComp_LuciferiumHeal : HediffComp
    {
        private const int TicksInDay = 60000;
        private const int DaysInYear = 60;
        private const int OptimalAge = 25;
        private const float MinimalHealthAmount = 0.01f;

        private int ticksToHeal;

        public HediffCompProperties_LuciferiumHeal Props => (HediffCompProperties_LuciferiumHeal)props;

        public HashSet<string> AdditionalHedifsToHeal { get; } = new HashSet<string>()
        {
            "ChemicalDamageSevere", "ChemicalDamageModerate", "Cirrhosis"
        };

        public HediffComp_LuciferiumHeal()
        {
            if(ticksToHeal > 6 * GenDate.TicksPerDay) ResetTicksToHeal();
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            // next heal event will happen after an hour in the debug mode or after 4 to 6 days (uniformly distributed) normaly
            ticksToHeal = Settings.Get().enableDebugHealingSpeed ? 2500 : Rand.Range(4 * GenDate.TicksPerDay, 6 * GenDate.TicksPerDay); 
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal >= 6 * GenDate.TicksPerDay) ResetTicksToHeal();
            else if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                if (Settings.Get().shouldAffectAge) AffectPawnsAge();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            var selectHediffsQuery = from hd in Pawn.health.hediffSet.hediffs
                                     where 
                                         hd.IsPermanent() 
                                         || hd.def.chronic 
                                         || AdditionalHedifsToHeal.Contains(hd.def.defName) 
                                         || (hd.def.defName.Equals("TraumaSavant") && Settings.Get().healTraumaSavant)
                                     select hd;

            if (selectHediffsQuery.TryRandomElement(out Hediff hediff))
            {
                float meanHeal = 0.2f;
                float healDeviation = 0.1f;
                float rndHealPercentValue = meanHeal + Rand.Gaussian() * healDeviation; // heal % is normaly distributed between 10 % and 30 %

                float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                float rawHealAmount = hediff.IsPermanent() ? bodyPartMaxHP * rndHealPercentValue : rndHealPercentValue;
                float healAmount = (rawHealAmount < MinimalHealthAmount) ? MinimalHealthAmount : rawHealAmount;

                if (hediff.Severity - healAmount < MinimalHealthAmount)
                    HandleLowSeverity(hediff);
                else
                    hediff.Severity -= healAmount;
            }
        }

        private void HandleLowSeverity(Hediff hediff)
        {
            Pawn.health.RemoveHediff(hediff);
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
                else if (Pawn.ageTracker.AgeBiologicalYears < 25) Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * GenDate.TicksPerDay); // get one quadrum older
            }
            else // if not humanlike then optimal age is the start of the third stage
            {
                int lifeStage = Pawn.ageTracker.CurLifeStageIndex;
                long startOfThirdStage = (long)(Pawn.RaceProps.lifeStageAges[2].minAge * 60 * GenDate.TicksPerDay);
                long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;

                if (lifeStage >= 3 && diffFromOptimalAge > 0) // then need to become younger
                {
                    Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * GenDate.TicksPerDay); // get 5 days older
                }
            }

        }

        private void ReduceAgeOfHumanlike()
        {
            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out int biologicalYears, out int biologicalQuadrums, out int biologicalDays, out float biologicalHours);

            string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
            long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - OptimalAge * DaysInYear * TicksInDay;
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
