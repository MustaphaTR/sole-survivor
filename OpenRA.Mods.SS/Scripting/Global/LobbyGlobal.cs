﻿#region Copyright & License Information
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
using Eluant;
using OpenRA.Mods.Common.Scripting;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SS.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Scripting
{
	[ScriptGlobal("Lobby")]
	public class LobbyGlobal : ScriptGlobal
	{
		readonly World world;

		public LobbyGlobal(ScriptContext context)
			: base(context)
		{
			world = context.World;
		}

		[Desc("Returns the value of Explored Map lobby option.")]
		public bool ExploredMap()
		{
			var shroud = world.WorldActor.Owner.PlayerActor.Trait<Shroud>();

			return shroud.ExploreMapEnabled;
		}

		[Desc("Returns the value of Fog of War lobby option.")]
		public bool FogOfWar()
		{
			var shroud = world.WorldActor.Owner.PlayerActor.Trait<Shroud>();

			return shroud.FogEnabled;
		}

		[Desc("Returns the value of Wacky Mode lobby option.")]
		public bool Wacky()
		{
			var sscs = world.WorldActor.Trait<SSCrateSpawner>();

			return sscs.Wacky;
		}

		[Desc("Returns the value of Team Spawns lobby option.")]
		public bool TeamSpawns()
		{
			var sssu = world.WorldActor.Trait<SpawnSSUnit>();

			return sssu.TeamSpawns;
		}
	}
}
