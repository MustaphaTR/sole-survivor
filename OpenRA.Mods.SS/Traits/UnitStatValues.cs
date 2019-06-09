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

using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	public class UnitStatValuesInfo : TraitInfo<UnitStatValues>
	{
		[Desc("Use this value for base damage of the unit for stats.")]
		public readonly int Damage = 0;

		[Desc("Overrides the sight value from RevealsShroud trait for the stats.")]
		public readonly WDist Sight = WDist.Zero;

		[Desc("Overrides the health value from Health trait for the stats.")]
		public readonly int Health = 0;

		[Desc("Overrides the range value from the weapons for the stats.")]
		public readonly WDist Range = WDist.Zero;

		[Desc("Overrides the reload delat value from the weapons for the stats.")]
		public readonly int ReloadDelay = 0;

		[Desc("Overrides the movement speed value from Mobile or Aircraft traits for the stats.")]
		public readonly int Speed = 0;
	}

	public class UnitStatValues { }
}
