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

        [Desc("Returns to or sets the player's.")]
        public Actor Unit
        {
            get { return spawnSSUnit.Units[player]; }
            set { spawnSSUnit.Units[player] = value; }
        }
    }
}
