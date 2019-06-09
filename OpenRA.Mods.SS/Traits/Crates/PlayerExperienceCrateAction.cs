#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Gives collector's owner experience.")]
	class PlayerExperienceCrateActionInfo : CrateActionInfo
	{
		[Desc("How much player experience this crate action gives.")]
		public int Experience = 0;

		public override object Create(ActorInitializer init) { return new PlayerExperienceCrateAction(init.Self, this); }
	}

	class PlayerExperienceCrateAction : CrateAction
	{
		readonly PlayerExperienceCrateActionInfo info;

		public PlayerExperienceCrateAction(Actor self, PlayerExperienceCrateActionInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		public override int GetSelectionShares(Actor collector)
		{
			var playerExperience = collector.Owner.PlayerActor.TraitOrDefault<PlayerExperience>();
			if (playerExperience == null)
				return 0;

			return base.GetSelectionShares(collector);
		}

		public override void Activate(Actor collector)
		{
			var playerExperience = collector.Owner.PlayerActor.TraitOrDefault<PlayerExperience>();
			playerExperience.GiveExperience(info.Experience);

			base.Activate(collector);
		}
	}
}
