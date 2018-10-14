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
    public class StatDamageValueInfo : TraitInfo<StatDamageValue>
    {
        [FieldLoader.Require]
        [Desc("Use this value for base damage of the unit for stats.")]
        public readonly int Damage = 0;
    }

    public class StatDamageValue { }
}
