using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace LHM
{
    public class HediffComp_LuciferiumHeal : HediffComp
    {
        private const int optimalAge = 21;
        
        private const float meanHeal = 0.04f / 6f;
        private const float healDeviation = meanHeal / 2f;
        private const float healingThreshold = 0.01f;

        private int ticksToHeal;

        public HediffCompProperties_LuciferiumHeal Props => (HediffCompProperties_LuciferiumHeal) props;

        public static HashSet<string> AdditionalHedifsToHeal { get; } = new HashSet<string>()
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
            ticksToHeal = Settings.Get().EnableDebugHealingSpeed 
                ? GenDate.TicksPerHour / 4 
                : GenDate.TicksPerHour * 4;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal <= 0 || ticksToHeal > GenDate.TicksPerHour * 4)
            {
                if (Settings.Get().ShouldReduceAge || Settings.Get().ShouldIncreaseAge) AffectPawnsAge(Pawn);

                TryHealRandomPermanentWound(Pawn, parent.LabelCap);
                ResetTicksToHeal();
            }
        }

        public static void TryHealRandomPermanentWound(Pawn pawn, string cause)
        {
            var selectHediffsQuery = from hd in pawn.health.hediffSet.hediffs
                                     where 
                                         hd.IsPermanent() && !(hd is Hediff_RegrowingBodyPart)
                                         || hd.def.chronic 
                                         || AdditionalHedifsToHeal.Contains(hd.def.defName) 
                                         || (string.Equals(hd.def.defName, "TraumaSavant", System.StringComparison.Ordinal) 
                                            && Settings.Get().HealTraumaSavant)
                                     select hd;

            if (selectHediffsQuery.TryRandomElement(out Hediff hediff))
            {
                float rndHealPercentValue = Rand.Gaussian(meanHeal, healDeviation); // 0.667 percent value +- 50 % => between 0.333 % and 1 %
                float bodyPartMaxHP = hediff.Part == null ? 1 : hediff.Part.def.GetMaxHealth(hediff.pawn);
                float healAmount = hediff.IsPermanent() ? bodyPartMaxHP * rndHealPercentValue : rndHealPercentValue;

                if (hediff.Severity - healAmount < healingThreshold) HandleLowSeverity(pawn, hediff, cause);
                else hediff.Severity -= healAmount;
            }

            if (Settings.Get().EnableRegrowingBodyParts && Utils.HungerRate(pawn) < Settings.Get().HungerRateTreshold) TryRegrowMissingBodypart(pawn);
        }

        private static void HandleLowSeverity(Pawn pawn, Hediff hediff, string cause)
        {
            pawn.health.RemoveHediff(hediff);
            if (PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message("MessagePermanentWoundHealed".Translate(cause, pawn.LabelShort, hediff.Label, pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent);
            }
        }

        private static void AffectPawnsAge(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.ageTracker.AgeBiologicalYears > optimalAge) ReduceAgeOfHumanlike(pawn);
                else if (Settings.Get().ShouldIncreaseAge && pawn.ageTracker.AgeBiologicalYears < optimalAge)
                {
                    pawn.ageTracker.AgeBiologicalTicks += (long)(GenDate.TicksPerDay / 2);
                }
            }
            else // if not humanlike then optimal age is the start of the third stage
            {
                int lifeStage = pawn.ageTracker.CurLifeStageIndex;
                int adultLifeStageIndex = pawn.RaceProps.lifeStageAges.Count - 1;

                if (lifeStage == adultLifeStageIndex) // then need to become younger
                {
                    ReduceAgeOfNonHumanlike(pawn);
                }
                else if (lifeStage < adultLifeStageIndex && Settings.Get().ShouldIncreaseAge && pawn.ageTracker.AgeBiologicalYears < optimalAge) // in that case mature faster towards 3rd stage
                {
                    pawn.ageTracker.AgeBiologicalTicks += (long)(GenDate.TicksPerDay / 6);
                }
            }
        }

        private static void ReduceAgeOfHumanlike(Pawn pawn)
        {
            pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out int biologicalYears, out int biologicalQuadrums, out int biologicalDays, out float biologicalHours);

            string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
            long diffFromOptimalAge = pawn.ageTracker.AgeBiologicalTicks - optimalAge * GenDate.DaysPerYear * GenDate.TicksPerDay;
            pawn.ageTracker.AgeBiologicalTicks -= diffFromOptimalAge / 600;

            pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out biologicalHours);
            string ageAfter = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);

            pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);

            if (pawn.IsColonist && Settings.Get().ShowAgingMessages)
            {
                Messages.Message("MessageAgeReduced".Translate(
                        pawn.LabelShort,
                        ageBefore,
                        ageAfter
                    ),
                    MessageTypeDefOf.PositiveEvent, true
                );
            }
        }

        private static void ReduceAgeOfNonHumanlike(Pawn pawn)
        {
            int adultLifeStageIndex = pawn.RaceProps.lifeStageAges.Count - 1;
            long startOfAdultStage = (long)(pawn.RaceProps.lifeStageAges[adultLifeStageIndex].minAge * GenDate.TicksPerYear);
            long diffFromOptimalAge = pawn.ageTracker.AgeBiologicalTicks - startOfAdultStage;

            pawn.ageTracker.AgeBiologicalTicks -= diffFromOptimalAge / 600;
            if (Settings.Get().ShowAgingMessages)
            {
                pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out int biologicalYears, out int biologicalQuadrums, out int biologicalDays, out float biologicalHours);
                string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
                pawn.ageTracker.AgeBiologicalTicks -= diffFromOptimalAge / 600;

                pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out biologicalHours);
                string ageAfter = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);

                Messages.Message("MessageAgeReduced".Translate(
                        pawn.LabelShort,
                        ageBefore,
                        ageAfter
                    ),
                    MessageTypeDefOf.PositiveEvent, true
                );
            }
        }

        private static void TryRegrowMissingBodypart(Pawn pawn)
        {
            HediffDef regrowingHediffDef = LHM_HediffDefOf.RegrowingBodypart;
            BodyPartRecord missingPart = Utils.FindBiggestMissingBodyPart(pawn);

            if (missingPart != null)
            {
                pawn.health.RestorePart(missingPart);
                pawn.health.AddHediff(HediffMaker.MakeHediff(regrowingHediffDef, pawn, missingPart));
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
