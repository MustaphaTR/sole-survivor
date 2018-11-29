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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Allows a condition to be granted from an external source (Lua, warheads, etc).")]
	public class SwitchConditionInfo : ITraitInfo, Requires<ConditionManagerInfo>
	{
		[GrantedConditionReference]
		[FieldLoader.Require]
		public readonly string Condition = null;

		public object Create(ActorInitializer init) { return new SwitchCondition(init.Self, this); }
	}

	public class SwitchCondition : INotifyCreated
	{
		public readonly SwitchConditionInfo Info;
        public int Token = ConditionManager.InvalidConditionToken;
        ConditionManager conditionManager;

        public SwitchCondition(Actor self, SwitchConditionInfo info)
		{
			Info = info;
		}

        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.Trait<ConditionManager>();
        }

		public void GrantCondition(Actor self)
        {
            if (conditionManager == null)
                return;

            Token = conditionManager.GrantCondition(self, Info.Condition);
		}
        
		public void RevokeCondition(Actor self)
		{
			if (conditionManager == null)
				return;

			if (conditionManager.TokenValid(self, Token))
				Token = conditionManager.RevokeCondition(self, Token);
		}
	}
}
