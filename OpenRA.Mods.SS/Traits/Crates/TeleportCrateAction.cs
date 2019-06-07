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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Teleports the unit to a random location.")]
	class TeleportCrateActionInfo : CrateActionInfo
	{
		[Desc("The maxiumum distance unit can be teleported to.")]
		public readonly int MaxDistance = 50;

        [Desc("The maxiumum distance unit can be teleported to.")]
        public readonly int MinDistance = 0;

        [Desc("Set camera position to where unit get teleported to.")]
        public readonly bool SetCameraPosition = false;

        [Desc("The range to search for extra collectors in.", "Extra collectors will also be granted the crate action.")]
		public readonly WDist Range = new WDist(1);

		[Desc("The maximum number of extra collectors to grant the crate action to.")]
		public readonly int MaxExtraCollectors = 4;

		public override object Create(ActorInitializer init) { return new TeleportCrateAction(init.Self, this); }
	}

	class TeleportCrateAction : CrateAction, IRender
    {
		readonly Actor self;
        readonly TeleportCrateActionInfo info;
        WorldRenderer wr;

        public TeleportCrateAction(Actor self, TeleportCrateActionInfo info)
			: base(self, info)
		{
			this.self = self;
			this.info = info;
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

        public override int GetSelectionShares(Actor collector)
		{
			var mobile = collector.TraitOrDefault<Mobile>();
			return mobile != null ? info.SelectionShares : 0;
		}

		public override void Activate(Actor collector)
		{
			var inRange = self.World.FindActorsInCircle(self.CenterPosition, info.Range).Where(a =>
			{
				// Don't touch the same unit twice
				if (a == collector)
					return false;

				// Only affect the collecting player's units
				// TODO: Also apply to allied units?
				if (a.Owner != collector.Owner)
					return false;

				// Ignore units that can't level up
				var ge = a.TraitOrDefault<GainsExperience>();
				return ge != null && ge.CanGainLevel;
			});

			if (info.MaxExtraCollectors > -1)
				inRange = inRange.Take(info.MaxExtraCollectors);

			foreach (var actor in inRange.Append(collector))
			{
				var recipient = actor;	// loop variable in closure hazard
				recipient.World.AddFrameEndTask(w =>
				{
                    var mobile = recipient.TraitOrDefault<Mobile>();
                    var locations = recipient.World.Map.FindTilesInAnnulus(recipient.Location, info.MinDistance, info.MaxDistance).Where(c => mobile.CanEnterCell(c));
                    if (mobile != null && locations.Any())
                    {
                        recipient.CancelActivity();

                        var loc = locations.Random(recipient.World.SharedRandom);
                        mobile.SetPosition(recipient, recipient.World.Map.CenterOfCell(loc));

                        if (info.SetCameraPosition && recipient.Owner == recipient.World.RenderPlayer)
                        {
                            wr.Viewport.Center(new Actor[] { recipient });
                        }
                    }
                });
			}

			base.Activate(collector);
		}
	}
}
