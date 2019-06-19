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

using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Grants some modifiers to the actor.")]
	public class SSMultiplierCrateActionInfo : CrateActionInfo
	{
		[FieldLoader.Require]
		[Desc("Type of the modifier, valid values are 'Armor', 'Damage', 'Sight', 'Range', 'Reload' and 'Speed'.")]
		public readonly SSMultiplier Type;

		[Desc("Amount of bonus to add.")]
		public readonly int Amount = 10;

		[Desc("Bonus can still appear when stat is maxed.")]
		public readonly bool AvailableWhenMaxed = true;

		[Desc("Amount of bonus to add to other team members.")]
		public readonly int TeamBonus = 2;

		[Desc("Prerequisites for the team bonus to apply.")]
		public readonly string[] TeamBonusPrerequisites = { "global-teambonus" };

		public override object Create(ActorInitializer init) { return new SSMultiplierCrateAction(init.Self, this); }
	}

	public class SSMultiplierCrateAction : CrateAction
	{
		SSMultiplierCrateActionInfo info;

		public SSMultiplierCrateAction(Actor self, SSMultiplierCrateActionInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		public override int GetSelectionShares(Actor collector)
		{
			if (!collector.Owner.Playable)
				return 0;

			var manager = collector.TraitOrDefault<SSMultiplierManager>();
			if (manager == null)
				return 0;

			if (!info.AvailableWhenMaxed)
			{
				if (info.Type == SSMultiplier.Armor && manager.ArmorModifier <= 50)
					return 0;

				if (info.Type == SSMultiplier.Damage && manager.DamageModifier >= 200)
					return 0;

				if (info.Type == SSMultiplier.Sight && manager.SightModifier >= 200)
					return 0;

				if (info.Type == SSMultiplier.Range && manager.RangeModifier >= 200)
					return 0;

				if (info.Type == SSMultiplier.Reload && manager.ReloadModifier <= 50)
					return 0;

				if (info.Type == SSMultiplier.Speed && manager.SpeedModifier >= 200)
					return 0;
			}

			if (info.Type == SSMultiplier.Sight)
			{
				var shroud = collector.Owner.PlayerActor.TraitOrDefault<Shroud>();
				if (shroud != null && shroud.ExploreMapEnabled && !shroud.FogEnabled)
					return 0;
			}

			return info.SelectionShares;
		}

		public override void Activate(Actor collector)
		{
			base.Activate(collector);

			var manager = collector.TraitOrDefault<SSMultiplierManager>();
			if (manager != null)
				ApplyBonus(manager, info.Amount);

			if (info.TeamBonus > 0)
			{
				if (info.TeamBonusPrerequisites.Any())
				{
					var techtree = collector.Owner.PlayerActor.TraitOrDefault<TechTree>();
					if (!techtree.HasPrerequisites(info.TeamBonusPrerequisites))
						return;
				}

				var spawner = collector.World.WorldActor.TraitOrDefault<SpawnSSUnit>();
				if (spawner != null && spawner.Teams[collector.Owner] != 0)
				{
					var players = collector.World.Players.Where(p => p.Playable);
					foreach (var player in players.Where(p => p != collector.Owner && spawner.Teams[p] == spawner.Teams[collector.Owner]))
					{
						var actor = collector.World.ActorsWithTrait<SSMultiplierManager>().Where(a => a.Actor.Owner == player).FirstOrDefault();
						if (actor != null)
							ApplyBonus(actor.Trait, info.TeamBonus);
					}
				}
			}
		}

		public void ApplyBonus(SSMultiplierManager manager, int amount)
		{
			if (manager == null)
				return;

			if (info.Type == SSMultiplier.Armor)
				manager.ArmorModifier = Math.Max(50, manager.ArmorModifier - amount);
			else if (info.Type == SSMultiplier.Damage)
				manager.DamageModifier = Math.Min(200, manager.DamageModifier + amount);
			else if (info.Type == SSMultiplier.Sight)
				manager.SightModifier = Math.Min(200, manager.SightModifier + amount);
			else if (info.Type == SSMultiplier.Range)
				manager.RangeModifier = Math.Min(200, manager.RangeModifier + amount);
			else if (info.Type == SSMultiplier.Reload)
				manager.ReloadModifier = Math.Max(50, manager.ReloadModifier - amount);
			else
				manager.SpeedModifier = Math.Min(200, manager.SpeedModifier + amount);
		}
	}
}
