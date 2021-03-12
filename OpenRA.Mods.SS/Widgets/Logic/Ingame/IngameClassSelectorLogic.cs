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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SS.Traits;
using OpenRA.Network;
using OpenRA.Primitives;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	class IngameClassSelectorLogic : ChromeLogic
	{
		public (string First, string Second) SplitOnFirstToken(string input, string token = "\\n")
		{
			if (string.IsNullOrEmpty(input))
				return (null, null);

			var split = input.IndexOf(token, StringComparison.Ordinal);
			var first = split > 0 ? input.Substring(0, split) : input;
			var second = split > 0 ? input.Substring(split + token.Length) : null;
			return (first, second);
		}

		public void ShowFactionDropDown(World world, SpawnSSUnit spawner, DropDownButtonWidget dropdown, Dictionary<string, LobbyFaction> factions)
		{
			var player = world.LocalPlayer;

			Func<string, ScrollItemWidget, ScrollItemWidget> setupItem = (factionId, itemTemplate) =>
			{
				var item = ScrollItemWidget.Setup(itemTemplate,
					() => spawner.Classes[player] == factionId,
					() => {
						if (spawner.Classes[player] != factionId)
						{
							spawner.Classes[player] = factionId;

							if (spawner.QuickClassChange)
							{
								var unit = spawner.Units[player];
								if (unit != null && !unit.IsDead)
									unit.Kill(unit);

								var others = world.ActorsWithTrait<SSExternalMultiplierManager>().Where(a => a.Actor.Owner == player);
								foreach (var a in others)
									a.Actor.Dispose();
							}
						}
					});
				var faction = factions[factionId];
				item.Get<LabelWidget>("LABEL").GetText = () => faction.Name;
				var flag = item.Get<ImageWidget>("FLAG");
				flag.GetImageCollection = () => "flags";
				flag.GetImageName = () => factionId;

				var tooltip = SplitOnFirstToken(faction.Description);
				item.GetTooltipText = () => tooltip.First;
				item.GetTooltipDesc = () => tooltip.Second;

				return item;
			};

			var options = factions.Where(f => f.Value.Selectable && f.Value.Side != "Random").GroupBy(f => f.Value.Side)
				.ToDictionary(g => g.Key ?? "", g => g.Select(f => f.Key));

			dropdown.ShowDropDown("FACTION_DROPDOWN_TEMPLATE", 154, options, setupItem);
		}

		[ObjectCreator.UseCtor]
		public IngameClassSelectorLogic(Widget widget, World world, OrderManager orderManager, WorldRenderer worldRenderer)
		{
			var player = world.LocalPlayer;
			var spawner = world.WorldActor.Trait<SpawnSSUnit>();
			var factions = new Dictionary<string, LobbyFaction>();
			foreach (var f in world.WorldActor.Info.TraitInfos<FactionInfo>())
				factions.Add(f.InternalName, new LobbyFaction { Selectable = f.Selectable, Name = f.Name, Side = f.Side, Description = f.Description });

			var closeButton = widget.Get<ButtonWidget>("CLOSE_CLASS");
			var openButton = widget.Get<ButtonWidget>("OPEN_CLASS");
			var classDropdown = widget.Get<DropDownButtonWidget>("CLASS_DROPDOWN");

			closeButton.OnClick = () =>
			{
				classDropdown.Visible = false;
				closeButton.Visible = false;
				openButton.Visible = true;
			};

			openButton.OnClick = () =>
			{
				classDropdown.Visible = true;
				closeButton.Visible = true;
				openButton.Visible = false;
			};

			openButton.IsDisabled = () => !spawner.ClassChanging;
			classDropdown.IsDisabled = () => !spawner.ClassChanging;
			classDropdown.OnMouseDown = _ => ShowFactionDropDown(world, spawner, classDropdown, factions);

			classDropdown.GetTooltipText = () => SplitOnFirstToken(factions[spawner.Classes[player]].Description).First;
			classDropdown.GetTooltipDesc = () => SplitOnFirstToken(factions[spawner.Classes[player]].Description).Second;

			var factionName = classDropdown.Get<LabelWidget>("FACTIONNAME");
			factionName.GetText = () => factions[spawner.Classes[player]].Name;
			var factionFlag = classDropdown.Get<ImageWidget>("FACTIONFLAG");
			factionFlag.GetImageName = () => spawner.Classes[player];
			factionFlag.GetImageCollection = () => "flags";
		}
	}
}
