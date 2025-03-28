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

using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Grants a condition on the collector if it is not, revokes it if it is.")]
	sealed class SwitchConditionCrateActionInfo : CrateActionInfo
	{
		[FieldLoader.Require]
		[Desc("The condition to grant or revoke. Must be included in the target actor's ExternalConditions list.")]
		public readonly string Condition = null;

		[Desc("The range to search for extra collectors in.", "Extra collectors will also be granted the crate action.")]
		public readonly WDist Range = new(3);

		[Desc("The maximum number of extra collectors to grant the crate action to.", "-1 = no limit")]
		public readonly int MaxExtraCollectors = 4;

		[Desc("Effect to draw when the condition is revoked.")]
		public readonly string RevokeSequence = null;

		[NotificationReference("Speech")]
		[Desc("Notification to play when the condition is revoked.")]
		public readonly string RevokeNotification = null;

		[FluentReference]
		[Desc("Text notification to display when the condition is revoked.")]
		public readonly string RevokeTextNotification = null;

		public override object Create(ActorInitializer init) { return new SwitchConditionCrateAction(init.Self, this); }
	}

	sealed class SwitchConditionCrateAction : CrateAction
	{
		readonly Actor self;
		readonly SwitchConditionCrateActionInfo info;

		public SwitchConditionCrateAction(Actor self, SwitchConditionCrateActionInfo info)
			: base(self, info)
		{
			this.self = self;
			this.info = info;
		}

		bool AcceptsCondition(Actor a)
		{
			return a.TraitsImplementing<SwitchCondition>()
				.Any(t => t.Info.Condition == info.Condition);
		}

		public override int GetSelectionShares(Actor collector)
		{
			return AcceptsCondition(collector) ? info.SelectionShares : 0;
		}

		public override void Activate(Actor collector)
		{
			var actorsInRange = self.World.FindActorsInCircle(self.CenterPosition, info.Range)
				.Where(a => a != self && a != collector && a.Owner == collector.Owner && AcceptsCondition(a));

			if (info.MaxExtraCollectors > -1)
				actorsInRange = actorsInRange.Take(info.MaxExtraCollectors);

			collector.World.AddFrameEndTask(w =>
			{
				foreach (var a in actorsInRange.Append(collector))
				{
					if (!a.IsInWorld || a.IsDead)
						continue;

					var switchCondition = a.TraitsImplementing<SwitchCondition>()
						.FirstOrDefault(t => t.Info.Condition == info.Condition);

					if (switchCondition != null)
					{
						var granted = switchCondition.Token != Actor.InvalidConditionToken;
						if (!granted)
						{
							switchCondition.GrantCondition(a);

							base.Activate(collector);
						}
						else
						{
							switchCondition.RevokeCondition(a);

							Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);

							if (!string.IsNullOrEmpty(info.RevokeNotification))
								Game.Sound.PlayNotification(self.World.Map.Rules, collector.Owner, "Speech",
									info.RevokeNotification, collector.Owner.Faction.InternalName);

							TextNotificationsManager.AddTransientLine(collector.Owner, info.RevokeTextNotification);

							if (Info.Image != null && info.RevokeSequence != null)
								collector.World.AddFrameEndTask(world => world.Add(new SpriteEffect(collector, world, Info.Image, info.RevokeSequence, info.Palette)));
						}
					}
				}
			});
		}
	}
}
