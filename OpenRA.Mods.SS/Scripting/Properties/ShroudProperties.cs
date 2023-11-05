#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Scripting
{
	[ScriptPropertyGroup("General")]
	public class ShroudProperties : ScriptPlayerProperties
	{
		readonly Shroud shroud;

		public ShroudProperties(ScriptContext context, Player player)
			: base(context, player)
		{
			shroud = player.PlayerActor.TraitOrDefault<Shroud>();
		}

		[Desc("Returns true if Explore Map lobby option is enabled.")]
		public bool ExploreMapEnabled
		{
			get { return shroud.ExploreMapEnabled; }
		}

		[Desc("Returns true if Fog of War lobby option is enabled.")]
		public bool FogEnabled
		{
			get { return shroud.FogEnabled; }
		}

		[Desc("Reshrouds the player's map.")]
		public void ReshroudMap()
		{
			shroud.ResetExploration();
		}
	}
}
