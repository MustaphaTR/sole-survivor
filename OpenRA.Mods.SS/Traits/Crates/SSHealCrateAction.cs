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
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Heals the collector if it is damaged, fires a weapon otherwise.")]
	class SSHealCrateActionInfo : CrateActionInfo
	{
		[WeaponReference]
		[FieldLoader.Require]
		[Desc("The weapon to fire if collector has full health.")]
		public string Weapon = null;

		[NotificationReference("Speech")]
		[Desc("Notification to play when the crate is collected and weapon fires.")]
		public readonly string WeaponNotification = null;

		public override object Create(ActorInitializer init) { return new SSHealCrateAction(init.Self, this); }
	}

	class SSHealCrateAction : CrateAction
	{
		readonly Actor self;
		readonly SSHealCrateActionInfo info;

		public SSHealCrateAction(Actor self, SSHealCrateActionInfo info)
			: base(self, info)
		{
			this.info = info;
			this.self = self;
		}

		public override void Activate(Actor collector)
		{
			var health = collector.TraitOrDefault<IHealth>();
			if (health != null && health.DamageState == DamageState.Undamaged)
			{
				var weapon = collector.World.Map.Rules.Weapons[info.Weapon.ToLowerInvariant()];
				health.InflictDamage(collector, collector, new Damage(-(health.MaxHP - health.HP)), true);

				if (!string.IsNullOrEmpty(info.WeaponNotification))
					Game.Sound.PlayNotification(self.World.Map.Rules, collector.Owner, "Speech",
						info.WeaponNotification, collector.Owner.Faction.InternalName);
			}
			else
			{
				health.InflictDamage(collector, collector, new Damage(-(health.MaxHP - health.HP)), true);

				if (!string.IsNullOrEmpty(Info.Notification))
					Game.Sound.PlayNotification(self.World.Map.Rules, collector.Owner, "Speech",
						Info.Notification, collector.Owner.Faction.InternalName);

				Game.Sound.Play(SoundType.World, info.Sound, self.CenterPosition);
				if (Info.Image != null && Info.Sequence != null)
					collector.World.AddFrameEndTask(w => w.Add(new SpriteEffect(collector, w, Info.Image, Info.Sequence, Info.Palette)));
			}
		}
	}
}
