#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using Eluant;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptPropertyGroup("Support Powers")]
	public class SSAirstrikeProperties : ScriptActorProperties, Requires<AirstrikePowerInfo>
	{
		readonly Actor self;

		public SSAirstrikeProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			this.self = self;
		}

		[Desc("Activate the actor's Airstrike Power. Returns the aircraft that will attack.")]
		public Actor[] TargetAirstrikeByOrderName(string name, WPos target, WAngle? facing = null)
		{
			var ap = self.TraitsImplementing<AirstrikePower>().FirstOrDefault(ap => ap.Info.OrderName == name);
			if (ap == null)
				throw new LuaException("No `AirstrikePower` with `OrderName: " + name + "` on actor " + self.Info.Name + ".");

			return ap.SendAirstrike(Self, target, facing);
		}
	}
}
