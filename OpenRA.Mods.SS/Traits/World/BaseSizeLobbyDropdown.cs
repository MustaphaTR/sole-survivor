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
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Controls the base size lobby options.")]
	[TraitLocation(SystemActors.World)]
	public class BaseSizeLobbyDropdownInfo : TraitInfo, ILobbyOptions
	{
		[Desc("Internal id for this option.")]
		public readonly string ID = "base-size";

		[TranslationReference]
		[Desc("Descriptive label for this option.")]
		public readonly string Label = "dropdown-base-size.label";

		[TranslationReference]
		[Desc("Tooltip description for this option.")]
		public readonly string Description = "dropdown-base-size.description";

		[Desc("Default base size.")]
		public readonly int Default = 0;

		[Desc("Possible base sizes to select.")]
		public readonly int[] Values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

		[Desc("Base buildings to spawn.")]
		public readonly string[] BaseBuildings = { "atwr", "obli" };

		[Desc("Prevent the option from being changed from its default value.")]
		public readonly bool Locked = false;

		[Desc("Whether to display the option in the lobby.")]
		public readonly bool Visible = true;

		[Desc("Display order for the option in the lobby.")]
		public readonly int DisplayOrder = 0;

		[Desc("Inner radius for spawning base buildings")]
		public readonly int InnerBaseRadius = 1;

		[Desc("Outer radius for spawning base buildings")]
		public readonly int OuterBaseRadius = 3;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(MapPreview map)
		{
			var values = Values.ToDictionary(bs => bs.ToString(), bs => bs.ToString());

			yield return new LobbyOption(map, ID, Label, Description, Visible, DisplayOrder,
				values, Default.ToString(), Locked);
		}

		public override object Create(ActorInitializer init) { return new BaseSizeLobbyDropdown(this); }
	}

	public class BaseSizeLobbyDropdown
	{
		public BaseSizeLobbyDropdown(BaseSizeLobbyDropdownInfo info) { }
	}
}
