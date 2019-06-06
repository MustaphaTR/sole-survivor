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
using OpenRA.Traits;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.SS.Traits
{
    [Desc("Grants a condition on the collector and heals it.")]
    public class WackyCrateActionInfo : GrantExternalConditionCrateActionInfo
    {
        public override object Create(ActorInitializer init) { return new WackyCrateAction(init.Self, this); }
    }

    public class WackyCrateAction : GrantExternalConditionCrateAction
    {
        public WackyCrateAction(Actor self, WackyCrateActionInfo info)
            : base(self, info) { }

        public override void Activate(Actor collector)
        {
            var health = collector.TraitOrDefault<Health>();
            if (health != null)
                health.InflictDamage(collector, collector, new Damage(-(health.MaxHP - health.HP)), true);

            base.Activate(collector);
        }
    }
}
