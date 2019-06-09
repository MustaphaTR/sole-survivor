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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	public class SSCrateInfo : CrateInfo
	{
		public new object Create(ActorInitializer init) { return new SSCrate(init, this); }
	}

	public class SSCrate : Crate, INotifyAddedToWorld, INotifyRemovedFromWorld
    {
		public SSCrate(ActorInitializer init, SSCrateInfo info)
		    : base(init, info) { }

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
	}
}
