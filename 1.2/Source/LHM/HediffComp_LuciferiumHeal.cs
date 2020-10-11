using RimWorld;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace LHM
{
    public class HediffComp_LuciferiumHeal : HediffComp
    {
        private const int optimalAge = 25;
        
        private const float meanHeal = 0.04f / 6f;
        private const float healDeviation = meanHeal / 2f;
        private const float healingThreshold = 0.01f;

        private int ticksToHeal;

        public HediffCompProperties_LuciferiumHeal Props => (HediffCompProperties_LuciferiumHeal) props;

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
            ticksToHeal = Settings.Get().EnableDebugHealingSpeed 
                ? GenDate.TicksPerHour / 4 
                : GenDate.TicksPerHour * 4;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal >= 6 * GenDate.TicksPerDay) 
                ResetTicksToHeal();
            else if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                if (Settings.Get().ShouldAffectAge) AffectPawnsAge();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            var selectHediffsQuery = from hd in Pawn.health.hediffSet.hediffs
                                     where 
                                         hd.IsPermanent() && !(hd is Hediff_RegrowingBodyPart)
                                         || hd.def.chronic 
                                         || AdditionalHedifsToHeal.Contains(hd.def.defName) 
                                         || (hd.def.defName.Equals("TraumaSavant") && Settings.Get().HealTraumaSavant)
                                     select hd;

            if (selectHediffsQuery.TryRandomElement(out Hediff hediff))
            {
                float rndHealPercentValue = Rand.Gaussian(meanHeal, healDeviation); // 0.667 percent value +- 50 % => between 0.333 % and 1 %
                float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                float healAmount = hediff.IsPermanent() ? bodyPartMaxHP * rndHealPercentValue : rndHealPercentValue;

                if (hediff.Severity - healAmount < healingThreshold) HandleLowSeverity(hediff);
                else hediff.Severity -= healAmount;
            }

            if (Settings.Get().EnableRegrowingBodyParts && Utils.HungerRate(Pawn) < Settings.Get().HungerRateTreshold) TryRegrowMissingBodypart();
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
                if (Pawn.ageTracker.AgeBiologicalYears > optimalAge) ReduceAgeOfHumanlike();
                else if (Pawn.ageTracker.AgeBiologicalYears < optimalAge)
                {
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(GenDate.TicksPerQuadrum);
                }
            }
            else // if not humanlike then optimal age is the start of the third stage
            {
                int lifeStage = Pawn.ageTracker.CurLifeStageIndex;
                long startOfThirdStage = (long)(Pawn.RaceProps.lifeStageAges[2].minAge * GenDate.TicksPerYear);
                long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;

                if (lifeStage >= 3 && diffFromOptimalAge > 0) // then need to become younger
                {
                    Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * GenDate.TicksPerDay);
                }
            }
        }

        private void ReduceAgeOfHumanlike()
        {
            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out int biologicalYears, out int biologicalQuadrums, out int biologicalDays, out float biologicalHours);

            string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
            long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - optimalAge * GenDate.DaysPerYear * GenDate.TicksPerDay;
            Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);

            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out biologicalHours);
            string ageAfter = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);

            if (Pawn.IsColonist && Settings.Get().ShowAgingMessages)
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

        private void TryRegrowMissingBodypart()
        {
            HediffDef regrowingHediffDef = LHM_HediffDefOf.RegrowingBodypart;
            BodyPartRecord missingPart = Utils.FindBiggestMissingBodyPart(Pawn);

            if (missingPart != null)
            {
                Pawn.health.RestorePart(missingPart);
                Pawn.health.AddHediff(HediffMaker.MakeHediff(regrowingHediffDef, Pawn, missingPart));
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
