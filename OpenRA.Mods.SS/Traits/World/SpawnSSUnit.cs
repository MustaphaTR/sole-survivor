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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Spawns the initial unit for a Sole Survivor game. Handles different spawning logics.")]
	[TraitLocation(SystemActors.World)]
	public class SpawnSSUnitInfo : TraitInfo, NotBefore<LocomotorInfo>, ILobbyOptions
	{
		[FluentReference]
		[Desc("Descriptive label for the team spawns checkbox in the lobby.")]
		public readonly string TeamSpawnsCheckboxLabel = "checkbox-team-spawns.label";

		[FluentReference]
		[Desc("Tooltip description for the team spawns checkbox in the lobby.")]
		public readonly string TeamSpawnsCheckboxDescription = "checkbox-team-spawns.description";

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
		public readonly WAngle? UnitFacing = null;

		[FluentReference]
		[Desc("Descriptive label for the quick class change checkbox in the lobby.")]
		public readonly string QuickClassChangeCheckboxLabel = "checkbox-quick-class-change.label";

		[FluentReference]
		[Desc("Tooltip description for the quick class change checkbox in the lobby.")]
		public readonly string QuickClassChangeCheckboxDescription = "checkbox-quick-class-change.description";

		[Desc("Default value of the quick class change checkbox in the lobby.")]
		public readonly bool QuickClassChangeCheckboxEnabled = false;

		[Desc("Prevent the quick class change state from being changed in the lobby.")]
		public readonly bool QuickClassChangeCheckboxLocked = false;

		[Desc("Whether to display the quick class change checkbox in the lobby.")]
		public readonly bool QuickClassChangeCheckboxVisible = true;

		[Desc("Display order for the quick class change checkbox in the lobby.")]
		public readonly int QuickClassChangeCheckboxDisplayOrder = 0;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(MapPreview map)
		{
			yield return new LobbyBooleanOption(
				map,
				"teamspawns",
				TeamSpawnsCheckboxLabel,
				TeamSpawnsCheckboxDescription,
				TeamSpawnsCheckboxVisible,
				TeamSpawnsCheckboxDisplayOrder,
				TeamSpawnsCheckboxEnabled,
				TeamSpawnsCheckboxLocked);

			yield return new LobbyBooleanOption(
				map,
				"quickclasschange",
				QuickClassChangeCheckboxLabel,
				QuickClassChangeCheckboxDescription,
				QuickClassChangeCheckboxVisible,
				QuickClassChangeCheckboxDisplayOrder,
				QuickClassChangeCheckboxEnabled,
				QuickClassChangeCheckboxLocked);
		}

		public override object Create(ActorInitializer init) { return new SpawnSSUnit(this); }
	}

	public class SpawnSSUnit : IWorldLoaded
	{
		readonly SpawnSSUnitInfo info;

		WorldRenderer wr;
		readonly HashSet<(string[] Actors, int Amount, int Inner, int Outer)> bases = new();
		readonly Dictionary<int, Session.Client> occupiedSpawnPoints = new();

		public bool TeamSpawns;
		public bool QuickClassChange;
		public bool ClassChanging = true;
		public bool ClassChangingPaused = false;
		public Dictionary<Player, CPos> PlayerSpawnPoints = new();
		public Dictionary<Player, Player> TeamLeaders = new();
		public Dictionary<Player, int> Teams = new();
		public Dictionary<Player, string> Classes = new();
		public Dictionary<Player, Actor> Units = new();

		CPos[] spawnLocations;
		List<int> availableSpawnPoints;

		public SpawnSSUnit(SpawnSSUnitInfo info)
		{
			this.info = info;
		}

		public void WorldLoaded(World world, WorldRenderer wr)
		{
			this.wr = wr;

			TeamSpawns = world.LobbyInfo.GlobalSettings
				.OptionOrDefault("teamspawns", info.TeamSpawnsCheckboxEnabled);

			QuickClassChange = world.LobbyInfo.GlobalSettings
				.OptionOrDefault("quickclasschange", info.QuickClassChangeCheckboxEnabled);

			foreach (var dropdown in world.Map.Rules.Actors[SystemActors.World].TraitInfos<BaseSizeLobbyDropdownInfo>())
			{
				int.TryParse(world.LobbyInfo.GlobalSettings
					.OptionOrDefault(dropdown.ID, dropdown.Default.ToString()), out var value);

				bases.Add((dropdown.BaseBuildings, value, dropdown.InnerBaseRadius, dropdown.OuterBaseRadius));
			}

			var spawns = new List<CPos>();
			foreach (var n in world.Map.ActorDefinitions)
				if (n.Value.Value == "mpspawn")
					spawns.Add(new ActorReference(n.Key, n.Value.ToDictionary()).GetValue<LocationInit, CPos>());

			spawnLocations = spawns.ToArray();

			// Initialize the list of unoccupied spawn points for AssignSpawnLocations to pick from
			availableSpawnPoints = LobbyUtils.AvailableSpawnPoints(spawnLocations.Length, world.LobbyInfo);
			foreach (var kv in world.LobbyInfo.Slots)
			{
				var client = world.LobbyInfo.ClientInSlot(kv.Key);
				if (client == null || client.SpawnPoint == 0)
					continue;

				availableSpawnPoints.Remove(client.SpawnPoint);
				occupiedSpawnPoints.Add(client.SpawnPoint, client);
			}

			var players = world.LobbyInfo.Clients.Where(c => !c.IsObserver);
			if (TeamSpawns)
			{
				var teams = players.Select(p => p.Team).Distinct();
				var leaders = new List<Session.Client>();
				foreach (var team in teams.Where(t => t != 0))
					leaders.Add(players.First(p => p.Team == team));

				foreach (var l in leaders)
				{
					var sp = GetSpawnPointForPlayer(world, l);
					var leader = FindPlayerInSlot(world, l.Slot);
					Teams[leader] = l.Team;
					TeamLeaders[leader] = leader;
					SpawnUnitForPlayer(world, leader, sp);
					SpawnBuildingsForPlayer(world, leader, sp);

					var teamMateSpawnCells = world.Map.FindTilesInAnnulus(sp, info.InnerTeammateRadius + 1, info.OuterTeammateRadius);
					foreach (var p in players.Where(p => p != l && p.Team == l.Team))
					{
						var player = FindPlayerInSlot(world, p.Slot);
						var actorRules = world.Map.Rules.Actors[player.Faction.InternalName.ToLowerInvariant()];
						var ip = actorRules.TraitInfo<IPositionableInfo>();
						var validCell = teamMateSpawnCells.Shuffle(world.SharedRandom).FirstOrDefault(c => ip.CanEnterCell(world, null, c));

						Teams[player] = p.Team;
						TeamLeaders[player] = leader;
						SpawnUnitForPlayer(world, player, validCell);
					}
				}

				foreach (var p in players.Where(p => p.Team == 0))
				{
					var player = FindPlayerInSlot(world, p.Slot);
					var cell = GetSpawnPointForPlayer(world, p);
					Teams[player] = p.Team;
					TeamLeaders[player] = player;
					SpawnUnitForPlayer(world, player, cell);
					SpawnBuildingsForPlayer(world, player, cell);
				}
			}
			else
			{
				foreach (var p in players)
				{
					var player = FindPlayerInSlot(world, p.Slot);
					var cell = GetSpawnPointForPlayer(world, p);
					Teams[player] = p.Team;
					TeamLeaders[player] = player;
					SpawnUnitForPlayer(world, player, cell);
					SpawnBuildingsForPlayer(world, player, cell);
				}
			}
		}

		static Player FindPlayerInSlot(World w, string pr)
		{
			return w.Players.FirstOrDefault(p => p.PlayerReference.Name == pr);
		}

		CPos GetSpawnPointForPlayer(World w, Session.Client p)
		{
			if (p.SpawnPoint > 0 && p.SpawnPoint <= spawnLocations.Length)
				return spawnLocations[p.SpawnPoint - 1];

			if (availableSpawnPoints.Count > 0)
			{
				var spawnPoint = availableSpawnPoints.Random(w.SharedRandom);
				availableSpawnPoints.Remove(spawnPoint);
				occupiedSpawnPoints.Add(spawnPoint, p);

				return spawnLocations[spawnPoint - 1];
			}
			else
			{
				var circle = w.Map.FindTilesInAnnulus(spawnLocations.Random(w.SharedRandom), 25, 50);
				var player = FindPlayerInSlot(w, p.Slot);
				var actorRules = w.Map.Rules.Actors[player.Faction.InternalName.ToLowerInvariant()];
				var ip = actorRules.TraitInfo<IPositionableInfo>();
				return circle.Shuffle(w.SharedRandom).FirstOrDefault(c => ip.CanEnterCell(w, null, c));
			}
		}

		void SpawnUnitForPlayer(World w, Player p, CPos sp)
		{
			var facing = info.UnitFacing ?? new WAngle(w.SharedRandom.Next(1024));
			Classes[p] = p.Faction.InternalName.ToLowerInvariant();
			Units[p] = w.CreateActor(p.Faction.InternalName.ToLowerInvariant(), new TypeDictionary
			{
				new LocationInit(sp),
				new OwnerInit(p),
				new SkipMakeAnimsInit(),
				new FacingInit(facing),
			});

			PlayerSpawnPoints[p] = sp;

			if (w.LocalPlayer == p)
				wr.Viewport.Center(w.Map.CenterOfCell(sp));
		}

		void SpawnBuildingsForPlayer(World w, Player p, CPos sp)
		{
			foreach (var (actors, amount, inner, outer) in bases)
			{
				var buildings = actors;
				var buildingSpawnCells = w.Map.FindTilesInAnnulus(sp, inner + 1, outer).ToArray();
				for (var i = 0; i < amount - amount % buildings.Length; i++)
				{
					var actor = buildings[i % buildings.Length];
					SpawnBuildingForPlayer(w, p, buildingSpawnCells, actor);
				}

				for (var i = 0; i < amount % buildings.Length; i++)
				{
					var actor = buildings.Random(w.SharedRandom);
					SpawnBuildingForPlayer(w, p, buildingSpawnCells, actor);
				}
			}
		}

		void SpawnBuildingForPlayer(World w, Player p, CPos[] cells, string actor)
		{
			var actorRules = w.Map.Rules.Actors[actor.ToLowerInvariant()];
			var building = actorRules.TraitInfo<BuildingInfo>();
			var validCells = cells.Where(c => w.CanPlaceBuilding(c, actorRules, building, null));
			if (!validCells.Any())
			{
				Log.Write("debug", $"No cells available to spawn base building {actor} for player {p}");
				return;
			}

			var cell = validCells.Random(w.SharedRandom);

			var facing = info.UnitFacing ?? new WAngle(w.SharedRandom.Next(1024));
			w.CreateActor(actor, new TypeDictionary
			{
				new LocationInit(cell),
				new OwnerInit(p),
				new SkipMakeAnimsInit(),
				new FacingInit(facing),
			});
		}
	}
}
