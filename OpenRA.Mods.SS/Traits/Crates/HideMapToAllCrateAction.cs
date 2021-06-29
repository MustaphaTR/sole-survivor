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

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Hides the entire map in shroud for all players.")]
	class HideMapToAllCrateActionInfo : CrateActionInfo
	{
		public override object Create(ActorInitializer init) { return new HideMapToAllCrateAction(init.Self, this); }
	}

	class HideMapToAllCrateAction : CrateAction
	{
		public HideMapToAllCrateAction(Actor self, HideMapToAllCrateActionInfo info)
			: base(self, info) { }

		public override int GetSelectionShares(Actor collector)
		{
			// Don't hide the map if the shroud is force-revealed
			var preventReset = collector.Owner.PlayerActor.TraitsImplementing<IPreventsShroudReset>()
				.Any(p => p.PreventShroudReset(collector.Owner.PlayerActor));
			if (preventReset || collector.Owner.Shroud.ExploreMapEnabled)
				return 0;

			return base.GetSelectionShares(collector);
		}

		public override void Activate(Actor collector)
		{
			TextNotificationsManager.AddSystemLine("Battlefield Control", collector.Owner.PlayerName + " has reshrouded the map.");

			foreach (var player in collector.World.Players)
				player.Shroud.ResetExploration();

			base.Activate(collector);
		}
	}
}
