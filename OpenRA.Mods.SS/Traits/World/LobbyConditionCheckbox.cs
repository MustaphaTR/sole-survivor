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
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Grants defined conditions at game start for all players if the checkbox is enabled.")]
	public class LobbyConditionCheckboxInfo : ITraitInfo, ILobbyOptions
	{
		[FieldLoader.Require]
		[Desc("Internal id for this checkbox.")]
		public readonly string ID = null;

		[FieldLoader.Require]
		[Desc("Display name for this checkbox.")]
		public readonly string Label = null;

		[Desc("Description name for this checkbox.")]
		public readonly string Description = null;

		[Desc("Default value of the checkbox in the lobby.")]
		public readonly bool Enabled = false;

		[Desc("Prevent the checkbox from being changed from its default value.")]
		public readonly bool Locked = false;

		[Desc("Display the checkbox in the lobby.")]
		public readonly bool Visible = true;

		[Desc("Display order for the checkbox in the lobby.")]
		public readonly int DisplayOrder = 0;

		[FieldLoader.Require]
		[Desc("Prerequisites to grant when this checkbox is enabled.")]
		public readonly string Condition = null;

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(Ruleset rules)
		{
			yield return new LobbyBooleanOption(ID, Label, Description,
				Visible, DisplayOrder, Enabled, Locked);
		}

		public object Create(ActorInitializer init) { return new LobbyConditionCheckbox(this); }
	}

	public class LobbyConditionCheckbox : INotifyCreated
	{
		readonly LobbyConditionCheckboxInfo info;
		public int Token = Actor.InvalidConditionToken;

		public LobbyConditionCheckbox(LobbyConditionCheckboxInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			var enabled = self.World.LobbyInfo.GlobalSettings.OptionOrDefault(info.ID, info.Enabled);
			if (enabled)
                Token = self.GrantCondition(info.Condition);
        }
	}
}
