#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
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
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Grants a condition on the collector if it is not, revokes it if it is.")]
	class SwitchConditionCrateActionInfo : CrateActionInfo
	{
		[FieldLoader.Require]
		[Desc("The condition to grant or revoke. Must be included in the target actor's ExternalConditions list.")]
		public readonly string Condition = null;

		[Desc("The range to search for extra collectors in.", "Extra collectors will also be granted the crate action.")]
		public readonly WDist Range = new WDist(3);

		[Desc("The maximum number of extra collectors to grant the crate action to.", "-1 = no limit")]
		public readonly int MaxExtraCollectors = 4;

		[Desc("Effect to draw when the condition is revoked.")]
		public readonly string RevokeSequence = null;

		[NotificationReference("Speech")]
		[Desc("Notification to play when the condition is revoked.")]
		public readonly string RevokeNotification = null;

		public override object Create(ActorInitializer init) { return new SwitchConditionCrateAction(init.Self, this); }
	}

	class SwitchConditionCrateAction : CrateAction
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
			if (a.TraitOrDefault<ConditionManager>() == null)
				return false;

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

					if (a.TraitOrDefault<ConditionManager>() == null)
						return;

					var switchCondition = a.TraitsImplementing<SwitchCondition>()
						.FirstOrDefault(t => t.Info.Condition == info.Condition);

					if (switchCondition != null)
					{
						var granted = switchCondition.Token != ConditionManager.InvalidConditionToken;
						if (!granted)
						{
							switchCondition.GrantCondition(a);

							if (!string.IsNullOrEmpty(Info.Notification))
								Game.Sound.PlayNotification(self.World.Map.Rules, collector.Owner, "Speech",
									Info.Notification, collector.Owner.Faction.InternalName);

							Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);
							if (Info.Image != null && Info.Sequence != null)
								collector.World.AddFrameEndTask(world => world.Add(new SpriteEffect(collector, world, Info.Image, Info.Sequence, Info.Palette)));
						}
						else
						{
							switchCondition.RevokeCondition(a);

							if (!string.IsNullOrEmpty(info.RevokeNotification))
								Game.Sound.PlayNotification(self.World.Map.Rules, collector.Owner, "Speech",
									info.RevokeNotification, collector.Owner.Faction.InternalName);

							Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);
							if (Info.Image != null && info.RevokeSequence != null)
								collector.World.AddFrameEndTask(world => world.Add(new SpriteEffect(collector, world, Info.Image, info.RevokeSequence, info.Palette)));
						}
					}
				}
			});
		}
	}
}
