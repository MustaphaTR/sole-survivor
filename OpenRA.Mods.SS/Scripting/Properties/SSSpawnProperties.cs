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

using OpenRA.Mods.SS.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.SS.Scripting
{
	[ScriptPropertyGroup("General")]
	public class SSSpawnProperties : ScriptPlayerProperties
	{
		readonly SpawnSSUnit spawnSSUnit;
		readonly Player player;

		public SSSpawnProperties(ScriptContext context, Player player)
			: base(context, player)
		{
			this.player = player;
			spawnSSUnit = player.World.WorldActor.TraitOrDefault<SpawnSSUnit>();
		}

		[Desc("Returns the player's spawn position in CPos.")]
		public CPos SpawnCellPosition
		{
			get { return spawnSSUnit.PlayerSpawnPoints[player]; }
			set { spawnSSUnit.PlayerSpawnPoints[player] = value; }
		}

		[Desc("Returns the player's spawn position in WPos.")]
		public WPos SpawnWorldPosition
		{
			get { return player.World.Map.CenterOfCell(spawnSSUnit.PlayerSpawnPoints[player]); }
		}

		[Desc("Returns to the leader of the team player is in.")]
		public Player TeamLeader
		{
			get { return spawnSSUnit.TeamLeaders[player]; }
		}

		[Desc("Returns to value of Team Spawns lobby option.")]
		public bool TeamSpawns
		{
			get { return spawnSSUnit.TeamSpawns; }
		}

		[Desc("Returns to value of Quick Class Change lobby option.")]
		public bool QuickClassChange
		{
			get { return spawnSSUnit.QuickClassChange; }
		}

		[Desc("Returns to or sets the player's class.")]
		public string Class
		{
			get { return spawnSSUnit.Classes[player]; }
			set { spawnSSUnit.Classes[player] = value; }
		}

		[Desc("Returns to or sets the player's unit.")]
		public Actor Unit
		{
			get { return spawnSSUnit.Units[player]; }
			set { spawnSSUnit.Units[player] = value; }
		}

		[Desc("Returns to or sets if player is allowed to change its class at all.")]
		public bool ClassChanging
		{
			get { return spawnSSUnit.ClassChanging; }
			set { spawnSSUnit.ClassChanging = value; }
		}

		[Desc("Returns to or sets if player is currently allowed to change its class.")]
		public bool ClassChangingPaused
		{
			get { return spawnSSUnit.ClassChangingPaused; }
			set { spawnSSUnit.ClassChangingPaused = value; }
		}
	}
}
