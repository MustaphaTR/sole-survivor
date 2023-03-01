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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	[Desc("Manages the stat modifiers for the other Sole Survivor units, namely Engineer's buildings and MHQ's planes.")]
	public class SSExternalMultiplierManagerInfo : ConditionalTraitInfo
	{
		public override object Create(ActorInitializer init) { return new SSExternalMultiplierManager(init.Self, this); }
	}

	public class SSExternalMultiplierManager : ConditionalTrait<SSExternalMultiplierManagerInfo>, IDamageModifier, IFirepowerModifier, IRevealsShroudModifier, IRangeModifier, IDetectCloakedModifier, IReloadModifier, IReloadAmmoModifier, ISpeedModifier
	{
		Actor self;
		Actor main;
		SSMultiplierManager mainManager;

		public SSExternalMultiplierManager(Actor self, SSExternalMultiplierManagerInfo info)
			: base(info)
		{
			this.self = self;

			var mainPair = self.World.ActorsWithTrait<SSMultiplierManager>().Where(a => a.Actor.Owner == self.Owner).FirstOrDefault();
			if (mainPair.Actor != null)
			{
				main = mainPair.Actor;
				mainManager = mainPair.Trait;
			}

			self.World.ActorAdded += ActorAdded;
			self.World.ActorRemoved += ActorRemoved;
		}

		int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.ArmorModifier;
		}

		int IFirepowerModifier.GetFirepowerModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.DamageModifier;
		}

		int IRevealsShroudModifier.GetRevealsShroudModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.SightModifier;
		}

		int IRangeModifier.GetRangeModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.RangeModifier;
		}

		int IDetectCloakedModifier.GetDetectCloakedModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.RangeModifier;
		}

		int IReloadModifier.GetReloadModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.ReloadModifier;
		}

		int IReloadAmmoModifier.GetReloadAmmoModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.ReloadModifier;
		}

		int ISpeedModifier.GetSpeedModifier()
		{
			return mainManager == null || IsTraitDisabled ? 100 : mainManager.SpeedModifier;
		}

		void ActorAdded(Actor a)
		{
			if (main != null)
				return;

			if (a.Owner != self.Owner)
				return;

			var manager = a.TraitOrDefault<SSMultiplierManager>();
			if (manager == null)
				return;

			main = a;
			mainManager = manager;
		}

		void ActorRemoved(Actor a)
		{
			if (a == main)
			{
				main = null;
				mainManager = null;
			}
		}
	}
}
