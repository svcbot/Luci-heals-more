using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace LHM
{
    public class LHM_HealOldWoundsProperties : HediffCompProperties
    {
        public LHM_HealOldWoundsProperties()
        {
            this.compClass = typeof(LHM_HealOldWounds);
        }
    }

    public class LHM_HealOldWounds : HediffComp
    {
        private int ticksToNextHeal;
        private List<string> chronicConditions = new List<string>()           
        {
            "BadBack", "Frail", "Cataract", "Blindness", "HearingLoss", "Dementia", "Alzheimers", 
            "Asthma", "HeartArteryBlockage", "Carcinoma", "TraumaSavant", "Cirrhosis",
            "ChemicalDamageSevere", "ChemicalDamageModerate"
        };

        
        public LHM_HealOldWounds()
        {
            //// Load all chronic diseases from game's database
            //Log.Message("Search for chronic conditions started: ");
            //foreach (HediffDef hediff in DefDatabase<HediffDef>.AllDefsListForReading)
            //{
            //    if (hediff.hediffClass.Name.Equals("HediffWithComps") )
            //    {
            //        Log.Message(hediff.defName + " " + hediff.hediffClass + " added");
            //        chronicConditions.Add(hediff.defName);
            //    }
            //}
        }

        public LHM_HealOldWoundsProperties Props
        {
            get
            {
                return (LHM_HealOldWoundsProperties)this.props;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            this.ResetTicksToHeal();
        }

        private void ResetTicksToHeal()
        {
            this.ticksToNextHeal = Rand.Range(4 * 60000, 6 * 60000); // one day = 60'000 ticks
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            this.ticksToNextHeal--;
            if (this.ticksToNextHeal <= 0)
            {
                this.TryHealRandomOldWound();
                this.AffectPawnsAge();
                this.ResetTicksToHeal();
            }
        }

        private void TryHealRandomOldWound()
        {
            Hediff hediff;
            // there should be a way to find chronic diseases without this list for better mod compatibility 
            if (!(from hd in base.Pawn.health.hediffSet.hediffs
                  where hd.IsOld() || chronicConditions.Contains(hd.def.defName)
                  select hd).TryRandomElement(out hediff))
            {
                return;
            }
            
            if (hediff != null)
            {
                int minHeal = 0.1f;
                int maxHeal = 0.3f;
                int meanHeal = (maxHeal - minHeal)/2.0f;
                float rndHealPercent = meanHeal + Rand.Gaussian() * meanHeal/2.0f; // heal % is normaly distributed between 10 % and 30 %

                float bodyPartMaxHP = hediff.Part.def.GetMaxHealth(hediff.pawn);
                float healAmount = bodyPartMaxHP * rndHealPercent;
                if (healAmount < 0.1f) healAmount = 0.1f;
                if (hediff.Severity - healAmount < 0.1f) base.Pawn.health.hediffSet.hediffs.Remove(hediff);
                else hediff.Severity -= healAmount;
              
            }

            if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
            {
                Messages.Message("MessageOldWoundHealed".Translate(new object[]
				{
					this.parent.Label,
					base.Pawn.LabelShort,
					hediff.Label
				}), MessageTypeDefOf.PositiveEvent);
            }
        }

        private void AffectPawnsAge()
        {
            if (base.Pawn.RaceProps.Humanlike)
            {
                if (base.Pawn.ageTracker.AgeBiologicalYears > 25)
                {
                    int biologicalYears;
                    int biologicalQuadrums;
                    int biologicalDays;
                    float num4;

                    base.Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out num4);

                    string ageBefore = "AgeBiological".Translate(new object[] { biologicalYears, biologicalQuadrums, biologicalDays });
                    long diffFromOptimalAge = base.Pawn.ageTracker.AgeBiologicalTicks - 25 * 60 * 60000;
                    base.Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);

                    base.Pawn.ageTracker.AgeBiologicalTicks.TicksToPeriod(out biologicalYears, out biologicalQuadrums, out biologicalDays, out num4);
                    string ageAfter = "AgeBiological".Translate(new object[] { biologicalYears, biologicalQuadrums, biologicalDays });

                    if (base.Pawn.IsColonist)
                    {
                        Messages.Message("MessageAgeReduced".Translate(new object[]
                            {
                                base.Pawn.LabelShort,
                                ageBefore,
                                ageAfter
                            }), MessageTypeDefOf.PositiveEvent);
                    }

                }
                else if (base.Pawn.ageTracker.AgeBiologicalYears < 25) // if 25 do nothing, if younger that 25, that mature faster towards 25
                {
                    base.Pawn.ageTracker.AgeBiologicalTicks += (long)(15 * 60000); // get one quadrum older
                }

            }
            else // if not humanlike then opimal age is the start of the third stage
            {               
                int lifeStage = base.Pawn.ageTracker.CurLifeStageIndex;
                long startOfThirdStage = (long)(base.Pawn.RaceProps.lifeStageAges[2].minAge * 60 * 60000);
                long diffFromOptimalAge = base.Pawn.ageTracker.AgeBiologicalTicks - startOfThirdStage;
                if (lifeStage >= 3) // then need to become younger
                {
                    base.Pawn.ageTracker.AgeBiologicalTicks -= (long)(diffFromOptimalAge * 0.05f);
                }
                else // in that case mature faster towards 3rd stage
                {
                    base.Pawn.ageTracker.AgeBiologicalTicks += (long)(5 * 60000); // get 5 days older
                }

            }
            
        }


        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToNextHeal, "ticksToHeal", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + this.ticksToNextHeal;
        }
    }
}
