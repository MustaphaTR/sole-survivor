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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
    [Desc("Spawns the initial unit for a Sole Survivor game. Handles different spawning logics.")]
    public class SpawnSSUnitInfo : ITraitInfo, ILobbyOptions
    {
        [Translate]
        [Desc("Descriptive label for the team spawns checkbox in the lobby.")]
        public readonly string TeamSpawnsCheckboxLabel = "Team Spawns";

        [Translate]
        [Desc("Tooltip description for the team spawns checkbox in the lobby.")]
        public readonly string TeamSpawnsCheckboxDescription = "Players with the same team spawn next to each other";

        [Desc("Default value of the team spawns checkbox in the lobby.")]
        public readonly bool TeamSpawnsCheckboxEnabled = true;

        [Desc("Prevent the team spawns state from being changed in the lobby.")]
        public readonly bool TeamSpawnsCheckboxLocked = false;

        [Desc("Whether to display the team spawns checkbox in the lobby.")]
        public readonly bool TeamSpawnsCheckboxVisible = true;

        [Desc("Display order for the team spawns checkbox in the lobby.")]
        public readonly int TeamSpawnsCheckboxDisplayOrder = 0;

        [Desc("Inner radius for spawning teammates")]
        public readonly int InnerTeammateRadius = 2;

        [Desc("Outer radius for spawning teammates")]
        public readonly int OuterTeammateRadius = 4;

        [Desc("Initial facing of the units.")]
        public readonly int UnitFacing = 0;

        IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
        {
            yield return new LobbyBooleanOption(
                "teamspawns",
                TeamSpawnsCheckboxLabel,
                TeamSpawnsCheckboxDescription,
                TeamSpawnsCheckboxVisible,
                TeamSpawnsCheckboxDisplayOrder,
                TeamSpawnsCheckboxEnabled,
                TeamSpawnsCheckboxLocked);
        }

        public object Create(ActorInitializer init) { return new SpawnSSUnit(this); }
    }

    public class SpawnSSUnit : IWorldLoaded
    {
        readonly SpawnSSUnitInfo info;

        WorldRenderer wr;
        bool teamSpawns;

        public Dictionary<Player, CPos> PlayerSpawnPoints = new Dictionary<Player, CPos>();
        Dictionary<CPos, bool> spawnPointOccupation = new Dictionary<CPos, bool>();

        public SpawnSSUnit(SpawnSSUnitInfo info)
        {
            this.info = info;
        }

        public void WorldLoaded(World world, WorldRenderer wr)
        {
            this.wr = wr;

            teamSpawns = world.LobbyInfo.GlobalSettings
                .OptionOrDefault("teamspawns", info.TeamSpawnsCheckboxEnabled);

            spawnPointOccupation = world.Actors.Where(a => a.Info.Name == "mpspawn")
                .Select(a => a.Location)
                .ToDictionary(s => s, s => false);

            var players = world.LobbyInfo.Clients;
            if (teamSpawns)
            {
                var teams = players.Select(p => p.Team).Distinct();
                var leaders = new List<Session.Client>();
                foreach (var team in teams.Where(t => t != 0))
                    leaders.Add(players.First(p => p.Team == team));

                foreach (var l in leaders)
                {
                    var sp = GetSpawnPointForPlayer(world, l);

                    SpawnUnitForPlayer(world, FindPlayerInSlot(world, l.Slot), sp);

                    var teamMateSpawnCells = world.Map.FindTilesInAnnulus(sp, info.InnerTeammateRadius + 1, info.OuterTeammateRadius);
                    foreach (var p in players.Where(p => p != l && p.Team == l.Team))
                    {
                        var player = FindPlayerInSlot(world, p.Slot);
                        var actorRules = world.Map.Rules.Actors[player.Faction.InternalName.ToLowerInvariant()];
                        var ip = actorRules.TraitInfo<IPositionableInfo>();
                        var validCell = teamMateSpawnCells.Shuffle(world.SharedRandom).FirstOrDefault(c => ip.CanEnterCell(world, null, c));

                        SpawnUnitForPlayer(world, player, validCell);
                    }
                }
                foreach (var p in players.Where(p => p.Team == 0))
                {
                    SpawnUnitForPlayer(world, FindPlayerInSlot(world, p.Slot), GetSpawnPointForPlayer(world, p));
                }
            }
            else
            {
                foreach (var p in players)
                {
                    SpawnUnitForPlayer(world, FindPlayerInSlot(world, p.Slot), GetSpawnPointForPlayer(world, p));
                }
            }
        }

        static Player FindPlayerInSlot(World w, string pr)
        {
            return w.Players.FirstOrDefault(p => p.PlayerReference.Name == pr);
        }

        CPos GetSpawnPointForPlayer(World w, Session.Client p)
        {
            var availableSpawns = spawnPointOccupation.Where(s => !s.Value);
            if (availableSpawns.Any())
            {
                return availableSpawns.Random(w.SharedRandom).Key;
            }
            else
            {
                var circle = w.Map.FindTilesInAnnulus(spawnPointOccupation.Random(w.SharedRandom).Key, 25, 50);
                var player = FindPlayerInSlot(w, p.Slot);
                var actorRules = w.Map.Rules.Actors[player.Faction.InternalName.ToLowerInvariant()];
                var ip = actorRules.TraitInfo<IPositionableInfo>();
                return circle.Shuffle(w.SharedRandom).FirstOrDefault(c => ip.CanEnterCell(w, null, c));
            }
        }

        void SpawnUnitForPlayer(World w, Player p, CPos sp)
        {
            w.CreateActor(p.Faction.InternalName.ToLowerInvariant(), new TypeDictionary
            {
                new LocationInit(sp),
                new OwnerInit(p),
                new SkipMakeAnimsInit(),
                new FacingInit(info.UnitFacing < 0 ? w.SharedRandom.Next(256) : info.UnitFacing),
            });

            spawnPointOccupation[sp] = true;
            PlayerSpawnPoints[p] = sp;

            if (w.LocalPlayer == p)
                wr.Viewport.Center(w.Map.CenterOfCell(sp));
        }
    }
}
