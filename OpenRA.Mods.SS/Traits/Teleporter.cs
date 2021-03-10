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
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	public class TeleporterInfo : TraitInfo, IPositionableInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Image containing the effect animation sequence.")]
		public readonly string Image = "crate-effects";

		[SequenceReference("Image")]
		[Desc("Animation sequence played when teleported. Leave empty for no effect.")]
		public readonly string Sequence = null;

		[PaletteReference]
		[Desc("Palette to draw the animation in.")]
		public readonly string Palette = "effect";

		[Desc("Audio clip to play when an actor is teleported.")]
		public readonly string Sound = null;

		[NotificationReference("Speech")]
		[Desc("Notification to play when an actor is teleported.")]
		public readonly string Notification = null;

		[Desc("The maxiumum distance unit can be teleported to.")]
		public readonly int MaxDistance = 50;

		[Desc("The maxiumum distance unit can be teleported to.")]
		public readonly int MinDistance = 0;

		[Desc("Set camera position to where unit get teleported to.")]
		public readonly bool SetCameraPosition = false;

		[Desc("Define actors that can collect crates by setting this into the Crushes field from the Mobile trait.")]
		public readonly string CrushClass = "teleporter";

		public override object Create(ActorInitializer init) { return new Teleporter(init, this); }

		public IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any)
		{
			var occupied = new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } };
			return new ReadOnlyDictionary<CPos, SubCell>(occupied);
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

	public class Teleporter : IPositionable, ICrushable, ISync, IRender, INotifyAddedToWorld, INotifyRemovedFromWorld, INotifyCrushed
	{
		readonly Actor self;
		readonly TeleporterInfo info;
		WorldRenderer wr;

		[Sync]
		public CPos Location;

		public Teleporter(ActorInitializer init, TeleporterInfo info)
		{
			self = init.Self;
			this.info = info;

			var locationInit = init.GetOrDefault<LocationInit>(info);
			if (locationInit != null)
				SetPosition(self, locationInit.Value);
		}

		void INotifyCrushed.WarnCrush(Actor self, Actor crusher, BitSet<CrushClass> crushClasses) { }

		void INotifyCrushed.OnCrush(Actor self, Actor crusher, BitSet<CrushClass> crushClasses)
		{
			// Crate can only be crushed if it is not in the air.
			if (!self.IsAtGroundLevel() || !crushClasses.Contains(info.CrushClass))
				return;

			OnCrushInner(crusher);
		}

		void OnCrushInner(Actor crusher)
		{
			Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);

			if (!string.IsNullOrEmpty(info.Notification))
				Game.Sound.PlayNotification(self.World.Map.Rules, crusher.Owner, "Speech",
					info.Notification, crusher.Owner.Faction.InternalName);

			if (info.Image != null && info.Sequence != null)
				crusher.World.AddFrameEndTask(w => w.Add(new SpriteEffect(crusher, w, info.Image, info.Sequence, info.Palette)));

			var recipient = crusher;  // loop variable in closure hazard
			recipient.CancelActivity();

			recipient.World.AddFrameEndTask(w =>
			{
				var positionable = recipient.TraitOrDefault<IPositionable>();
				var locations = recipient.World.Map.FindTilesInAnnulus(recipient.Location, info.MinDistance, info.MaxDistance).Where(c => positionable.CanEnterCell(c));
				if (positionable != null && locations.Any())
				{
					recipient.CancelActivity();

					var loc = locations.Random(recipient.World.SharedRandom);
					positionable.SetPosition(recipient, recipient.World.Map.CenterOfCell(loc));

					if (info.SetCameraPosition && recipient.Owner == recipient.World.RenderPlayer)
					{
						wr.Viewport.Center(new Actor[] { recipient });
					}
				}
			});
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
			SetLocation(self, cell, subCell);
			SetCenterPosition(self, self.World.Map.CenterOfCell(cell));
		}

		// Sets only the visual position (CenterPosition)
		public void SetCenterPosition(Actor self, WPos pos)
		{
			CenterPosition = pos;
			self.World.UpdateMaps(self, this);
		}

		// Sets only the location (Location)
		void SetLocation(Actor self, CPos cell, SubCell subCell = SubCell.Any)
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
			// Crate can only be crushed if it is not in the air.
			return self.IsAtGroundLevel() && crushClasses.Contains(info.CrushClass);
		}

		LongBitSet<PlayerBitMask> ICrushable.CrushableBy(Actor self, BitSet<CrushClass> crushClasses)
		{
			return self.IsAtGroundLevel() && crushClasses.Contains(info.CrushClass) ? self.World.AllPlayersMask : self.World.NoPlayersMask;
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			self.World.AddToMaps(self, this);

			var cs = self.World.WorldActor.TraitOrDefault<SSCrateSpawner>();
			if (cs != null)
				cs.IncrementCrates();
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.RemoveFromMaps(self, this);

			var cs = self.World.WorldActor.TraitOrDefault<SSCrateSpawner>();
			if (cs != null)
				cs.DecrementCrates();
		}

		public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
		{
			return new IRenderable[] { };
		}

		IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
		{
			this.wr = wr;

			yield break;
		}
	}
}
