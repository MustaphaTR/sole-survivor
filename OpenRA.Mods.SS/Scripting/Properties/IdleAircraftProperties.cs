#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Activities;
using OpenRA.Scripting;

namespace OpenRA.Mods.SS.Scripting
{
	[ScriptPropertyGroup("Movement")]
	public class IdleAircraftProperties : ScriptActorProperties
	{
		readonly Actor actor;

		public IdleAircraftProperties(ScriptContext context, Actor actor)
			: base(context, actor)
		{
			this.actor = actor;
		}

		[Desc("Returns true if actor's current activity is FlyIdle.")]
		public bool IsIdleAircraft { get { return actor.CurrentActivity is FlyIdle; } }
	}
}
