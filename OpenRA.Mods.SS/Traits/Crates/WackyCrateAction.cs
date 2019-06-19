#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Fill collectors multipliers for a duration and heals it, after duration passes all multipliers are removed.")]
	public class WackyCrateActionInfo : CrateActionInfo
	{
		[Desc("Duration of the wacky bonus.")]
		public readonly int Duration = 750;

		public override object Create(ActorInitializer init) { return new WackyCrateAction(init.Self, this); }
	}

	public class WackyCrateAction : CrateAction
	{
		WackyCrateActionInfo info;

		public WackyCrateAction(Actor self, WackyCrateActionInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		public override int GetSelectionShares(Actor collector)
		{
			return collector.TraitOrDefault<SSMultiplierManager>() != null ? info.SelectionShares : 0;
		}

		public override void Activate(Actor collector)
		{
			var health = collector.TraitOrDefault<Health>();
			if (health != null)
				health.InflictDamage(collector, collector, new Damage(-(health.MaxHP - health.HP)), true);

			var manager = collector.TraitOrDefault<SSMultiplierManager>();
			if (manager != null)
			{
				manager.ArmorModifier = 50;
				manager.DamageModifier = 200;
				manager.SightModifier = 200;
				manager.RangeModifier = 200;
				manager.ReloadModifier = 50;
				manager.SpeedModifier = 200;

				manager.WackyTick = info.Duration;
				manager.WackyDuration = info.Duration;
				manager.WackyEnabled = true;
			}

			base.Activate(collector);
		}
	}
}
