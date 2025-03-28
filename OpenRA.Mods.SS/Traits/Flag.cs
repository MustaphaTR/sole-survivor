﻿#region Copyright & License Information
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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Flag for the Capture the Flag mode.")]
	public class FlagInfo : TraitInfo, IPositionableInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Allowed to land on.")]
		public readonly HashSet<string> TerrainTypes = new();

		[Desc("Define actors that can collect flag by setting this into the Crushes field from the Mobile trait.")]
		public readonly string CrushClass = "flag";

		public override object Create(ActorInitializer init) { return new Flag(init, this); }

		public IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any)
		{
			return new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } };
		}

		bool IOccupySpaceInfo.SharesCell { get { return false; } }

		public bool CanEnterCell(World world, Actor self, CPos cell, SubCell subCell = SubCell.FullCell, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			return GetAvailableSubCell(world, cell, ignoreActor, check) != SubCell.Invalid;
		}

		public bool CanExistInCell(World world, CPos cell)
		{
			if (!world.Map.Contains(cell))
				return false;

			var type = world.Map.GetTerrainInfo(cell).Type;
			if (!TerrainTypes.Contains(type))
				return false;

			return true;
		}

		public SubCell GetAvailableSubCell(World world, CPos cell, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			if (!CanExistInCell(world, cell))
				return SubCell.Invalid;

			if (check == BlockedByActor.None)
				return SubCell.FullCell;

			return !world.ActorMap.GetActorsAt(cell).Any(x => x != ignoreActor)
				? SubCell.FullCell : SubCell.Invalid;
		}
	}

	public class Flag : IPositionable, ICrushable, ISync, INotifyCreated,
		INotifyAddedToWorld, INotifyRemovedFromWorld, INotifyCrushed
	{
		readonly Actor self;
		readonly FlagInfo info;
		readonly SpawnSSUnit spawner;
		public bool Collected;
		INotifyCenterPositionChanged[] notifyCenterPositionChanged;

		[Sync]
		public CPos Location;

		public Flag(ActorInitializer init, FlagInfo info)
		{
			self = init.Self;
			this.info = info;

			spawner = self.World.WorldActor.Trait<SpawnSSUnit>();
			var locationInit = init.GetOrDefault<LocationInit>(info);
			if (locationInit != null)
				Location = locationInit.Value;
		}

		void INotifyCreated.Created(Actor self)
		{
			SetPosition(self, Location);
			notifyCenterPositionChanged = self.TraitsImplementing<INotifyCenterPositionChanged>().ToArray();
		}

		void INotifyCrushed.WarnCrush(Actor self, Actor crusher, BitSet<CrushClass> crushClasses) { }

		void INotifyCrushed.OnCrush(Actor self, Actor crusher, BitSet<CrushClass> crushClasses)
		{
			if (!crushClasses.Contains(info.CrushClass))
				return;

			// Only players can take flags.
			if (!crusher.Owner.Playable)
				return;

			// You can't take your team's flag if it is already on the spawn point.
			var flagTeam = FindPlayersClient(self.World, self.Owner).Team;
			var crusherTeam = FindPlayersClient(crusher.World, crusher.Owner).Team;
			if (self.Location == spawner.PlayerSpawnPoints[self.Owner]
				&& ((flagTeam != 0 && flagTeam == crusherTeam) || (flagTeam == 0 && self.Owner == crusher.Owner)))
				return;

			self.World.AddFrameEndTask(w => OnCrushInner(crusher, flagTeam, crusherTeam));
		}

		void OnCrushInner(Actor crusher, int flagTeam, int crusherTeam)
		{
			if (Collected)
				return;

			self.World.Remove(self);
			Collected = true;

			var carriesFlag = crusher.TraitOrDefault<CarriesFlag>();
			if (carriesFlag != null)
			{
				if (self.Owner == crusher.Owner || (flagTeam != 0 && flagTeam == crusherTeam))
					TextNotificationsManager.AddSystemLine("Battlefield Control", crusher.Owner.ResolvedPlayerName + " has taken their flag.");
				else
					TextNotificationsManager.AddSystemLine("Battlefield Control", crusher.Owner.ResolvedPlayerName + " has taken flag of " + (flagTeam == 0 || !spawner.TeamSpawns ? self.Owner.ResolvedPlayerName : "Team " + flagTeam) + ".");

				carriesFlag.GrantCondition(crusher);
				carriesFlag.TakeFlag(crusher, self);
			}
		}

		public CPos TopLeft { get { return Location; } }
		public (CPos, SubCell)[] OccupiedCells() { return new[] { (Location, SubCell.FullCell) }; }

		public WPos CenterPosition { get; private set; }

		// Sets the location (Location) and visual position (CenterPosition)
		public void SetPosition(Actor self, WPos pos)
		{
			var cell = self.World.Map.CellContaining(pos);
			SetLocation(self, cell);
			SetCenterPosition(self, self.World.Map.CenterOfCell(cell) + new WVec(WDist.Zero, WDist.Zero, self.World.Map.DistanceAboveTerrain(pos)));
		}

		// Sets the location (Location) and visual position (CenterPosition)
		public void SetPosition(Actor self, CPos cell, SubCell subCell = SubCell.Any)
		{
			SetLocation(self, cell);
			SetCenterPosition(self, self.World.Map.CenterOfCell(cell));
		}

		// Sets only the visual position (CenterPosition)
		public void SetCenterPosition(Actor self, WPos pos)
		{
			CenterPosition = pos;
			self.World.UpdateMaps(self, this);

			// This can be called from the constructor before notifyCenterPositionChanged is assigned.
			if (notifyCenterPositionChanged != null)
				foreach (var n in notifyCenterPositionChanged)
					n.CenterPositionChanged(self, 0, 0);
		}

		// Sets only the location (Location)
		void SetLocation(Actor self, CPos cell)
		{
			self.World.ActorMap.RemoveInfluence(self, this);
			Location = cell;
			self.World.ActorMap.AddInfluence(self, this);
		}

		public bool IsLeavingCell(CPos location, SubCell subCell = SubCell.Any) { return false; }
		public SubCell GetValidSubCell(SubCell preferred = SubCell.Any) { return SubCell.FullCell; }
		public SubCell GetAvailableSubCell(CPos cell, SubCell preferredSubCell = SubCell.Any, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			return info.GetAvailableSubCell(self.World, cell, ignoreActor, check);
		}

		public bool CanExistInCell(CPos cell) { return info.CanExistInCell(self.World, cell); }

		public bool CanEnterCell(CPos a, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			return GetAvailableSubCell(a, SubCell.Any, ignoreActor, check) != SubCell.Invalid;
		}

		bool ICrushable.CrushableBy(Actor self, Actor crusher, BitSet<CrushClass> crushClasses)
		{
			return crushClasses.Contains(info.CrushClass);
		}

		LongBitSet<PlayerBitMask> ICrushable.CrushableBy(Actor self, BitSet<CrushClass> crushClasses)
		{
			return self.IsAtGroundLevel() && crushClasses.Contains(info.CrushClass) ? self.World.AllPlayersMask : self.World.NoPlayersMask;
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			self.World.AddToMaps(self, this);

			var cs = self.World.WorldActor.TraitOrDefault<CrateSpawner>();
			cs?.IncrementCrates();
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.RemoveFromMaps(self, this);

			var cs = self.World.WorldActor.TraitOrDefault<CrateSpawner>();
			cs?.DecrementCrates();
		}

		static Session.Client FindPlayersClient(World w, Player p)
		{
			return w.LobbyInfo.Clients.FirstOrDefault(c => c.Slot == p.PlayerReference.Name);
		}
	}
}
