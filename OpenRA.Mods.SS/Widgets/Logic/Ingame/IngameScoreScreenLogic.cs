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

using System.Globalization;
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
	sealed class IngameScoreScreenLogic : ChromeLogic
	{
		[TranslationReference("team")]
		const string TeamNumber = "label-team-name";

		[TranslationReference]
		const string NoTeam = "label-no-team";

		[TranslationReference]
		const string Spectators = "label-spectators";

		[ObjectCreator.UseCtor]
		public IngameScoreScreenLogic(Widget widget, World world, OrderManager orderManager, WorldRenderer worldRenderer)
		{
			var spawner = world.WorldActor.Trait<SpawnSSUnit>();

			var closeButton = widget.Get<ButtonWidget>("CLOSE_STATS");
			var openButton = widget.Get<ButtonWidget>("OPEN_STATS");
			var stats = widget.Get<ContainerWidget>("STATS_HEADERS");

			var player = world.RenderPlayer ?? world.LocalPlayer;
			var playerPanel = widget.Get<ScrollPanelWidget>("PLAYER_LIST");

			var teamTemplate = playerPanel.Get<ScrollItemWidget>("TEAM_TEMPLATE");
			var playerTemplate = playerPanel.Get("PLAYER_TEMPLATE");
			playerPanel.RemoveChildren();

			closeButton.OnClick = () =>
			{
				stats.Visible = false;
				playerPanel.Visible = false;
				teamTemplate.Visible = false;
				playerTemplate.Visible = false;
				closeButton.Visible = false;
				openButton.Visible = true;
			};

			openButton.OnClick = () =>
			{
				stats.Visible = true;
				playerPanel.Visible = true;
				teamTemplate.Visible = true;
				playerTemplate.Visible = true;
				closeButton.Visible = true;
				openButton.Visible = false;
			};

			var teams = world.Players.Where(p => !p.NonCombatant && p.Playable)
				.Select(p => (Player: p, PlayerStatistics: p.PlayerActor.TraitOrDefault<PlayerStatistics>()))
				.OrderByDescending(p => p.PlayerStatistics?.Experience ?? 0)
				.GroupBy(p => (world.LobbyInfo.ClientWithIndex(p.Player.ClientIndex) ?? new Session.Client()).Team)
				.OrderByDescending(g => g.Sum(gg => gg.PlayerStatistics?.Experience ?? 0))
				.ToList();

			foreach (var t in teams)
			{
				if (teams.Count > 1)
				{
					var teamHeader = ScrollItemWidget.Setup(teamTemplate, () => false, () => { });
					var team = t.Key > 0
						? TranslationProvider.GetString(TeamNumber, Translation.Arguments("team", t.Key))
						: TranslationProvider.GetString(NoTeam);
					teamHeader.Get<LabelWidget>("TEAM").GetText = () => team;
					var teamRating = teamHeader.Get<LabelWidget>("TEAM_SCORE");
					var scoreCache = new CachedTransform<int, string>(s => s.ToString(NumberFormatInfo.CurrentInfo));
					var teamMemberScores = t.Select(tt => tt.PlayerStatistics).Where(s => s != null).ToArray().Select(s => s.Experience);
					teamRating.GetText = () => scoreCache.Update(teamMemberScores.Sum());

					playerPanel.AddChild(teamHeader);
				}

				foreach (var p in t.ToList())
				{
					var pp = p.Player;
					var client = world.LobbyInfo.ClientWithIndex(pp.ClientIndex);
					var item = playerTemplate.Clone();
					LobbyUtils.SetupProfileWidget(item, client, orderManager, worldRenderer);

					var nameLabel = item.Get<LabelWidget>("NAME");
					var nameFont = Game.Renderer.Fonts[nameLabel.Font];

					var suffixLength = new CachedTransform<string, int>(s => nameFont.Measure(s).X);
					var name = new CachedTransform<(string Name, WinState WinState, Session.ClientState ClientState), string>(c =>
					{
						var suffix = "";
						if (pp.WinState == WinState.Lost)
							suffix = " (L)";
						else if (pp.WinState == WinState.Won)
							suffix = " (W)";
						if (client != null && client.State == Session.ClientState.Disconnected)
							suffix = " (G)";

						return WidgetUtils.TruncateText(c.Name, nameLabel.Bounds.Width - nameFont.Measure(suffix).X, nameFont) + suffix;
					});

					nameLabel.GetText = () =>
					{
						var clientState = client != null ? client.State : Session.ClientState.Ready;
						return name.Update((pp.PlayerName, pp.WinState, clientState));
					};

					// nameLabel.GetColor = () => pp.Color;
					nameLabel.GetColor = () => Color.White;

					var flag = item.Get<ImageWidget>("FACTIONFLAG");
					flag.GetImageCollection = () => "flags";
					if (player == null || player.RelationshipWith(pp) == PlayerRelationship.Ally || player.WinState != WinState.Undefined)
					{
						flag.GetImageName = () => spawner.Classes[pp];
					}
					else
					{
						flag.GetImageName = () => pp.DisplayFaction.InternalName;
					}

					var scoreCache = new CachedTransform<int, string>(s => s.ToString());
					item.Get<LabelWidget>("SCORE").GetText = () => scoreCache.Update(p.PlayerStatistics?.Experience ?? 0);

					playerPanel.AddChild(item);
				}
			}

			var spectators = orderManager.LobbyInfo.Clients.Where(c => c.IsObserver).ToList();
			if (spectators.Count > 0)
			{
				var spectatorHeader = ScrollItemWidget.Setup(teamTemplate, () => false, () => { });
				var spectatorTeam = TranslationProvider.GetString(Spectators);
				spectatorHeader.Get<LabelWidget>("TEAM").GetText = () => spectatorTeam;

				playerPanel.AddChild(spectatorHeader);

				foreach (var client in spectators)
				{
					var item = playerTemplate.Clone();
					LobbyUtils.SetupProfileWidget(item, client, orderManager, worldRenderer);

					var nameLabel = item.Get<LabelWidget>("NAME");
					var nameFont = Game.Renderer.Fonts[nameLabel.Font];

					var suffixLength = new CachedTransform<string, int>(s => nameFont.Measure(s).X);
					var name = new CachedTransform<(string Name, string Suffix), string>(c =>
						WidgetUtils.TruncateText(c.Name, nameLabel.Bounds.Width - suffixLength.Update(c.Suffix), nameFont) + c.Suffix);

					nameLabel.GetText = () =>
					{
						var suffix = client.State == Session.ClientState.Disconnected ? " (G)" : "";
						return name.Update((client.Name, suffix));
					};

					item.Get<ImageWidget>("FACTIONFLAG").IsVisible = () => false;
					playerPanel.AddChild(item);
				}
			}
		}
	}
}
