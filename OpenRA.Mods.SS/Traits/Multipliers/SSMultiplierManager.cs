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

using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
	public enum SSMultiplier { Armor, Damage, Sight, Range, Reload, Speed }

	[Desc("Manages the stat modifiers for the main Sole Survivor unit.")]
	public class SSMultiplierManagerInfo : ConditionalTraitInfo
	{
		public readonly Color WackyColor = Color.Orange;

		public override object Create(ActorInitializer init) { return new SSMultiplierManager(init.Self, this); }
	}

	public class SSMultiplierManager : ConditionalTrait<SSMultiplierManagerInfo>, ITick, IDamageModifier, IFirepowerModifier, IRevealsShroudModifier, IRangeModifier, IDetectCloakedModifier, IReloadModifier, IReloadAmmoModifier, ISpeedModifier, ISelectionBar
	{
		Actor self;

		public int ArmorModifier = 100;
		public int DamageModifier = 100;
		public int SightModifier = 100;
		public int RangeModifier = 100;
		public int ReloadModifier = 100;
		public int SpeedModifier = 100;

		public int WackyTick;
		public int WackyDuration;
		public bool WackyEnabled;

		public SSMultiplierManager(Actor self, SSMultiplierManagerInfo info)
			: base(info)
		{
			this.self = self;
		}

		void ITick.Tick(Actor self)
		{
			if (!WackyEnabled)
				return;

			if (--WackyTick < 0)
			{
				WackyEnabled = false;

				ArmorModifier = 100;
				DamageModifier = 100;
				SightModifier = 100;
				RangeModifier = 100;
				ReloadModifier = 100;
				SpeedModifier = 100;
			}
		}

		int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
		{
			return IsTraitDisabled ? 100 : ArmorModifier;
		}

		int IFirepowerModifier.GetFirepowerModifier()
		{
			return IsTraitDisabled ? 100 : DamageModifier;
		}

		int IRevealsShroudModifier.GetRevealsShroudModifier()
		{
			return IsTraitDisabled ? 100 : SightModifier;
		}

		int IRangeModifier.GetRangeModifier()
		{
			return IsTraitDisabled ? 100 : RangeModifier;
		}

		int IDetectCloakedModifier.GetDetectCloakedModifier()
		{
			return IsTraitDisabled ? 100 : RangeModifier;
		}

		int IReloadModifier.GetReloadModifier()
		{
			return IsTraitDisabled ? 100 : ReloadModifier;
		}

		int IReloadAmmoModifier.GetReloadAmmoModifier()
		{
			return IsTraitDisabled ? 100 : ReloadModifier;
		}

		int ISpeedModifier.GetSpeedModifier()
		{
			return IsTraitDisabled ? 100 : SpeedModifier;
		}

		float ISelectionBar.GetValue()
		{
			if (!WackyEnabled || IsTraitDisabled)
				return 0;

			return WackyTick / (float)WackyDuration;
		}

		Color ISelectionBar.GetColor() { return Info.WackyColor; }
		bool ISelectionBar.DisplayWhenEmpty { get { return false; } }
	}
}
