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
	[Desc("Grants a condition on the collector when Explored Map is disabled or FoW is enabled.")]
	public class GrantExternalConditionWhenShroudOrFogEnabledCrateActionInfo : GrantExternalConditionCrateActionInfo
	{
		public override object Create(ActorInitializer init) { return new GrantExternalConditionWhenShroudOrFogEnabledCrateAction(init.Self, this); }
	}

	public class GrantExternalConditionWhenShroudOrFogEnabledCrateAction : GrantExternalConditionCrateAction
	{
		readonly Shroud shroud;

		public GrantExternalConditionWhenShroudOrFogEnabledCrateAction(Actor self, GrantExternalConditionWhenShroudOrFogEnabledCrateActionInfo info)
			: base(self, info)
		{
			shroud = self.Owner.PlayerActor.TraitOrDefault<Shroud>();
		}

		public override int GetSelectionShares(Actor collector)
		{
			if (shroud != null && shroud.ExploreMapEnabled && !shroud.FogEnabled)
				return 0;

			return base.GetSelectionShares(collector);
		}
	}
}
