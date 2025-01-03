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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Changes the remap color of the player to one of the team leader.")]
	public class TeamLeaderColorModifierInfo : TraitInfo
	{
		[PaletteReference(true)]
		public readonly string Palette = "player";

		public override object Create(ActorInitializer init) { return new TeamLeaderColorModifier(init.Self, this); }
	}

	public class TeamLeaderColorModifier : IRenderModifier, IRadarColorModifier
	{
		readonly TeamLeaderColorModifierInfo info;
		readonly SpawnSSUnit spawner;
		readonly Player leader;

		public TeamLeaderColorModifier(Actor self, TeamLeaderColorModifierInfo info)
		{
			this.info = info;
			spawner = self.World.WorldActor.Trait<SpawnSSUnit>();
			spawner.TeamLeaders.TryGetValue(self.Owner, out leader);
		}

		IEnumerable<IRenderable> IRenderModifier.ModifyRender(Actor self, WorldRenderer wr, IEnumerable<IRenderable> r)
		{
			if (leader == null || leader == self.Owner)
				return r;

			var palette = wr.Palette(info.Palette + leader.InternalName);
			return r.Select(a => !a.IsDecoration && a is IPalettedRenderable renderable ? renderable.WithPalette(palette) : a);
		}

		IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self, WorldRenderer wr, IEnumerable<Rectangle> bounds)
		{
			return bounds;
		}

		Color IRadarColorModifier.RadarColorOverride(Actor self, Color color)
		{
			if (leader == null || leader == self.Owner)
				return color;

			return leader.Color;
		}
	}
}
