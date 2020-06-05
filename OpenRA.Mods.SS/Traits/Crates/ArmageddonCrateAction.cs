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
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Kills all player owned units on the map.")]
	class ArmageddonCrateActionInfo : CrateActionInfo
	{
		[Desc("The deathtypes used to kill the units.")]
		public readonly BitSet<DamageType> DeathTypes = default(BitSet<DamageType>);

		public override object Create(ActorInitializer init) { return new ArmageddonCrateAction(init.Self, this); }
	}

	class ArmageddonCrateAction : CrateAction
	{
		readonly Actor self;
		readonly ArmageddonCrateActionInfo info;

		public ArmageddonCrateAction(Actor self, ArmageddonCrateActionInfo info)
			: base(self, info)
		{
			this.info = info;
			this.self = self;
		}

		public override void Activate(Actor collector)
		{
			Game.AddSystemLine("Battlefield Control", collector.Owner.PlayerName + " has taken the armageddon crate!");

			if (!string.IsNullOrEmpty(Info.Notification))
				foreach (var player in self.World.Players)
				{
					// Don't play it twice to the collector, it is already handled in base.Activate()
					if (collector.Owner == player)
						continue;

					Game.Sound.PlayNotification(self.World.Map.Rules, player, "Speech",
						Info.Notification, collector.Owner.Faction.InternalName);
				}

			var actors = self.World.Actors.Where(a => a.Owner.Playable && a.TraitOrDefault<SSMultiplierManager>() != null);
			foreach (var actor in actors)
				actor.Kill(self, info.DeathTypes);

			base.Activate(collector);
		}
	}
}
