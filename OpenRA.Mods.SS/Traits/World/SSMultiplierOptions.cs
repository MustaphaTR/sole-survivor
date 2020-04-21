#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
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

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Controls the game speed, tech level, and short game lobby options.")]
	public class SSMultiplierOptionsInfo : ITraitInfo, ILobbyOptions
	{
		[Translate]
		[Desc("Descriptive label for the maximum multiplier option in the lobby.")]
		public readonly string MaxMultiplierDropdownLabel = "Max. Bonus M.";

		[Translate]
		[Desc("Tooltip description for the maximum multiplier option in the lobby.")]
		public readonly string MaxMultiplierDropdownDescription = "Maximum percentage of stat bonus you can get by collecting crates";

		[Desc("Default maximum multiplier.")]
		public readonly int MaxMultiplier = 200;

		[Desc("Starting maximum multiplier options that are available in the lobby options.")]
		public readonly int[] SelectableMaxMultiplier = { 150, 200, 250, 300, 500, 1000, 2500, 5000, 10000 };

		[Desc("Prevent the maximum multiplier from being changed in the lobby.")]
		public readonly bool MaxMultiplierDropdownLocked = false;

		[Desc("Display the maximum multiplier option in the lobby.")]
		public readonly bool MaxMultiplierDropdownVisible = true;

		[Desc("Display order for the maximum multiplier option in the lobby.")]
		public readonly int MaxMultiplierDropdownDisplayOrder = 0;

		[Translate]
		[Desc("Tooltip description for the game speed option in the lobby.")]
		public readonly string StandardMultiplierDropdownLabel = "Def. Bonus M.";

		[Translate]
		[Desc("Description of the game speed option in the lobby.")]
		public readonly string StandardMultiplierDropdownDescription = "Percentage amount of bonus you get from each upgrade from brown crates";

		[Desc("Default game speed.")]
		public readonly int StandardMultiplier = 10;

		[Desc("Starting cash options that are available in the lobby options.")]
		public readonly int[] SelectableStandardMultiplier = { 10, 20, 25, 30, 50, 100 };

		[Desc("Prevent the game speed from being changed in the lobby.")]
		public readonly bool StandardMultiplierDropdownLocked = false;

		[Desc("Display the game speed option in the lobby.")]
		public readonly bool StandardMultiplierDropdownVisible = true;

		[Desc("Display order for the game speed option in the lobby.")]
		public readonly int StandardMultiplierDropdownDisplayOrder = 0;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
		{
			var maxMultipliers = SelectableMaxMultiplier.ToDictionary(c => c.ToString(), c => c.ToString() + "%");

			if (maxMultipliers.Any())
				yield return new LobbyOption("maxmultiplier", MaxMultiplierDropdownLabel, MaxMultiplierDropdownDescription, MaxMultiplierDropdownVisible, MaxMultiplierDropdownDisplayOrder,
					new ReadOnlyDictionary<string, string>(maxMultipliers),	MaxMultiplier.ToString(), MaxMultiplierDropdownLocked);

			var standardMultipliers = SelectableStandardMultiplier.ToDictionary(c => c.ToString(), c => c.ToString() + "%");

			if (standardMultipliers.Any())
				yield return new LobbyOption("standardmultiplier", StandardMultiplierDropdownLabel, StandardMultiplierDropdownDescription, StandardMultiplierDropdownVisible, StandardMultiplierDropdownDisplayOrder,
					new ReadOnlyDictionary<string, string>(standardMultipliers), StandardMultiplier.ToString(), StandardMultiplierDropdownLocked);
		}

		public object Create(ActorInitializer init) { return new SSMultiplierOptions(this); }
	}

	public class SSMultiplierOptions : INotifyCreated
	{
		readonly SSMultiplierOptionsInfo info;

		public int MaxMultiplier;
		public int StandardMultiplier;

		public SSMultiplierOptions(SSMultiplierOptionsInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			var max = self.World.LobbyInfo.GlobalSettings
				.OptionOrDefault("maxmultiplier", info.MaxMultiplier.ToString());

			if (!int.TryParse(max, out MaxMultiplier))
				MaxMultiplier = info.MaxMultiplier;

			var standard = self.World.LobbyInfo.GlobalSettings
				.OptionOrDefault("standardmultiplier", info.StandardMultiplier.ToString());

			if (!int.TryParse(standard, out StandardMultiplier))
				StandardMultiplier = info.StandardMultiplier;
		}
	}
}
