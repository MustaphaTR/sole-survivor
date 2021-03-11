#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Automatically sells the actor when trait is enabled.")]
	public class AutoSellInfo : ConditionalTraitInfo, Requires<SellableInfo>
	{
		public override object Create(ActorInitializer init) { return new AutoSell(init.Self, this); }
	}

	public class AutoSell : ConditionalTrait<AutoSellInfo>
	{
		Sellable sellable;

		public AutoSell(Actor self, AutoSellInfo info)
			: base(info)
		{
			sellable = self.Trait<Sellable>();
		}

		protected override void TraitEnabled(Actor self)
		{
			sellable.Sell(self);
		}
	}
}
