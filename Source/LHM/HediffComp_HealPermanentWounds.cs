using RimWorld;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Verse;

namespace LHM
{
    public class HediffComp_HealPermanentWounds : HediffComp
    {
        private int ticksToHeal;

        public HediffCompProperties_HealPermanentWounds Props => (HediffCompProperties_HealPermanentWounds)props;

        private HashSet<string> chronicConditions = new HashSet<string>()
        {
            "ChemicalDamageSevere", "ChemicalDamageModerate"
        };

        public HediffComp_HealPermanentWounds()
        {
            if(ticksToHeal > 4 * 60000) ResetTicksToHeal();

            // Add all hediffs given by HediffGiver_Birthday 
            foreach (HediffGiverSetDef hediffGiverSetDef in DefDatabase<HediffGiverSetDef>.AllDefsListForReading)
            {
                hediffGiverSetDef.hediffGivers
                    .FindAll(hg => hg.GetType() == typeof(HediffGiver_Birthday))
                    .ForEach(hg => {
                        if (hg.hediff.isBad && hg.hediff.everCurableByItem) chronicConditions.Add(hg.hediff.defName);
                        });
            }
            Log.Message("Additional chronic conditions that will be healed by luci:\n" + string.Join(", ", chronicConditions.ToArray()));
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            if (Settings.Get().debugHealingSpeed) ticksToHeal = 3000;
            else ticksToHeal = Rand.Range(4 * 60000, 6 * 60000); // one day = 60'000 ticks
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal >= 4 * 60000) ResetTicksToHeal();
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
                                     where hd.IsPermanent() || chronicConditions.Contains(hd.def.defName)
                                     select hd;
            if (selectHediffsQuery.Any())
            {
                selectHediffsQuery.TryRandomElement(out Hediff hediff);

                if (hediff != null)
                {
                    float meanHeal = 0.2f;
                    float rndHealPercent = meanHeal + (Rand.Gaussian() * meanHeal / 2f); // heal % is normaly distributed between 10 % and 30 %

                    float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                    float rawHealAmount = bodyPartMaxHP * rndHealPercent;
                    float healAmount = (rawHealAmount < 0.1f) ? 0.1f : rawHealAmount;

                    if (hediff.Severity - healAmount < 0.1f) HandleLowSeverity(hediff);
                    else hediff.Severity -= healAmount;
                }
            }
        }

        private void HandleLowSeverity(Hediff hediff)
        {
            string hediffName = hediff.def.defName;

            if (hediff.IsPermanent()) hediff.Severity = 0f;
            else Pawn.health.RemoveHediff(hediff);

            if (PawnUtility.ShouldSendNotificationAbout(Pawn))
            {
                Messages.Message("MessagePermanentWoundHealed".Translate(
                        parent.LabelCap,
                        Pawn.LabelShort, hediffName,
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
                else if (Pawn.ageTracker.AgeBiologicalYears < 25) Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * 60000); // get one quadrum older
            }
            else // if not humanlike then optimal age is the start of the third stage
            {
                int lifeStage = Pawn.ageTracker.CurLifeStageIndex;
                long startOfThirdStage = (long)(Pawn.RaceProps.lifeStageAges[2].minAge * 60 * 60000);
                long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;
                if (lifeStage >= 3 && diffFromOptimalAge > 0) // then need to become younger
                {
                    Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * 60000); // get 5 days older
                }
            }

        }

        private void ReduceAgeOfHumanlike()
        {
            int biologicalYears;
            int biologicalQuadrums;
            int biologicalDays;
            float biologicalHours;

            Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out biologicalHours);

            string ageBefore = "AgeBiological".Translate(biologicalYears, biologicalQuadrums, biologicalDays);
            long diffFromOptimalAge = Pawn.ageTracker.AgeBiologicalTicks - 25 * 60 * 60000;
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
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }
    }

}

