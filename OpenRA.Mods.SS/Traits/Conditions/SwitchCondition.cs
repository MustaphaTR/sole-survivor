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

using OpenRA.Mods.Common;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Allows a condition to be granted from an external source (Lua, warheads, etc).")]
	public class SwitchConditionInfo : TraitInfo
	{
		[GrantedConditionReference]
		[FieldLoader.Require]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new SwitchCondition(init.Self, this); }
	}

	public class SwitchCondition
	{
		public readonly SwitchConditionInfo Info;
		public int Token = Actor.InvalidConditionToken;

		public SwitchCondition(Actor self, SwitchConditionInfo info)
		{
			Info = info;
		}

		public void GrantCondition(Actor self)
		{
			Token = self.GrantCondition(Info.Condition);
		}

		public void RevokeCondition(Actor self)
		{
			if (self.TokenValid(Token))
				Token = self.RevokeCondition(Token);
		}
	}
}
