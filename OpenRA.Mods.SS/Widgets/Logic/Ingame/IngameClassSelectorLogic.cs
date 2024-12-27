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
using OpenRA.Mods.SS.Traits;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	sealed class IngameClassSelectorLogic : ChromeLogic
	{
		public void ShowFactionDropDown(World world, SpawnSSUnit spawner, DropDownButtonWidget dropdown, Dictionary<string, LobbyFaction> factions)
		{
			var player = world.LocalPlayer;

			ScrollItemWidget SetupItem(string factionId, ScrollItemWidget itemTemplate)
			{
				var item = ScrollItemWidget.Setup(itemTemplate,
					() => spawner.Classes[player] == factionId,
					() =>
					{
						if (spawner.Classes[player] != factionId)
						{
							spawner.Classes[player] = factionId;

							if (spawner.QuickClassChange)
							{
								var unit = spawner.Units[player];
								if (unit != null && !unit.IsDead)
									unit.Kill(unit);
							}
						}
					});
				var faction = factions[factionId];
				item.Get<LabelWidget>("LABEL").GetText = () => FluentProvider.GetMessage(faction.Name);
				var flag = item.Get<ImageWidget>("FLAG");
				flag.GetImageCollection = () => "flags";
				flag.GetImageName = () => factionId;

				var description = faction.Description != null ? FluentProvider.GetMessage(faction.Description) : null;
				var (text, desc) = LobbyUtils.SplitOnFirstToken(description);
				item.GetTooltipText = () => text;
				item.GetTooltipDesc = () => desc;

				return item;
			}

			var options = factions.Where(f => f.Value.Selectable && f.Value.Side != "Random").GroupBy(f => f.Value.Side)
				.ToDictionary(g => g.Key ?? "", g => g.Select(f => f.Key));

			dropdown.ShowDropDown("FACTION_DROPDOWN_TEMPLATE", 154, options, SetupItem);
		}

		[ObjectCreator.UseCtor]
		public IngameClassSelectorLogic(Widget widget, World world)
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
			classDropdown.IsDisabled = () => !spawner.ClassChanging || spawner.ClassChangingPaused;
			classDropdown.OnMouseDown = _ => ShowFactionDropDown(world, spawner, classDropdown, factions);

			classDropdown.GetTooltipText = () => LobbyUtils.SplitOnFirstToken(FluentProvider.GetMessage(factions[spawner.Classes[player]].Description)).First;
			classDropdown.GetTooltipDesc = () => LobbyUtils.SplitOnFirstToken(FluentProvider.GetMessage(factions[spawner.Classes[player]].Description)).Second;

			var factionName = classDropdown.Get<LabelWidget>("FACTIONNAME");
			factionName.GetText = () => FluentProvider.GetMessage(factions[spawner.Classes[player]].Name);
			var factionFlag = classDropdown.Get<ImageWidget>("FACTIONFLAG");
			factionFlag.GetImageName = () => spawner.Classes[player];
			factionFlag.GetImageCollection = () => "flags";
		}
	}
}
