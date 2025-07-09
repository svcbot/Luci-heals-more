using System.Collections.Generic;
using RimWorld;
using Verse;

namespace LHM
{

	public class Gene_GradualHealing : Gene
	{
		private int ticksToHeal;

		public override void PostAdd()
		{
			base.PostAdd();
			ResetInterval();
		}

		public override void Tick()
		{
			base.Tick();
			ticksToHeal--;
			if (ticksToHeal <= 0 || ticksToHeal > GenDate.TicksPerHour * 4)
			{
				HediffComp_LuciferiumHeal.TryHealRandomPermanentWound(pawn, LabelCap);
				ResetInterval();
			}
		}

		private void ResetInterval()
		{
			ticksToHeal = Settings.Get().EnableDebugHealingSpeed
				? GenDate.TicksPerHour / 4
				: GenDate.TicksPerHour * 4;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			if (DebugSettings.ShowDevGizmos)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEV: Heal permanent wound",
					action = delegate
					{
						HediffComp_HealPermanentWounds.TryHealRandomPermanentWound(pawn, LabelCap);
						ResetInterval();
					}
				};
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0);
		}
	}
}
