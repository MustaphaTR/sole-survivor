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

using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SS.Traits;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class IngameUnitStatsLogic : ChromeLogic
	{
		[FluentReference]
		const string Infinite = "label-actor-stats-infinite";

		[ObjectCreator.UseCtor]
		public IngameUnitStatsLogic(Widget widget, World world)
		{
			var spawner = world.WorldActor.Trait<SpawnSSUnit>();
			var selection = world.WorldActor.Trait<ISelection>();

			var options = world.WorldActor.TraitOrDefault<SSMultiplierOptions>();
			var max = options != null ? 100f / (options.MaxMultiplier - 100) : 1;

			var health = widget.Get<LabelWidget>("STAT_HEALTH");
			var sight = widget.Get<LabelWidget>("STAT_SIGHT");
			var damage = widget.Get<LabelWidget>("STAT_DAMAGE");
			var range = widget.Get<LabelWidget>("STAT_RANGE");
			var rof = widget.Get<LabelWidget>("STAT_ROF");
			var speed = widget.Get<LabelWidget>("STAT_SPEED");

			var healthBar = widget.Get<ProgressBarWidget>("STAT_HEALTH_BAR");
			var sightBar = widget.Get<ProgressBarWidget>("STAT_SIGHT_BAR");
			var damageBar = widget.Get<ProgressBarWidget>("STAT_DAMAGE_BAR");
			var rangeBar = widget.Get<ProgressBarWidget>("STAT_RANGE_BAR");
			var rofBar = widget.Get<ProgressBarWidget>("STAT_ROF_BAR");
			var speedBar = widget.Get<ProgressBarWidget>("STAT_SPEED_BAR");

			var healthText = FluentProvider.GetMessage(health.Text);
			var sightText = FluentProvider.GetMessage(sight.Text);
			var damageText = FluentProvider.GetMessage(damage.Text);
			var rangeText = FluentProvider.GetMessage(range.Text);
			var rofText = FluentProvider.GetMessage(rof.Text);
			var speedText = FluentProvider.GetMessage(speed.Text);
			var infiniteText = FluentProvider.GetMessage(Infinite);

			health.GetText = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Health > 0)
						{
							var healthValue = usv.Health;
							foreach (var dm in unit.TraitsImplementing<IDamageModifier>().Select(dm => dm.GetDamageModifier(unit, new Damage(usv.Health))).Where(d => d != 0))
								healthValue = healthValue / dm * 100;

							return healthText + ": " + healthValue.ToString();
						}
						else if (usv.Health < 0)
						{
							return healthText + ": " + infiniteText;
						}
					}

					var healthTrait = unit.TraitOrDefault<Health>();
					if (healthTrait != null)
					{
						var healthValue = healthTrait.MaxHP;
						foreach (var dm in unit.TraitsImplementing<IDamageModifier>().Select(dm => dm.GetDamageModifier(unit, new Damage(healthTrait.MaxHP))).Where(d => d != 0))
							healthValue = healthValue / dm * 100;

						return healthText + ": " + healthValue.ToString();
					}
				}

				return healthText + ":";
			};
			healthBar.GetPercentage = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Health > 0)
						{
							var healthValue = usv.Health;
							foreach (var dm in unit.TraitsImplementing<IDamageModifier>().Select(dm => dm.GetDamageModifier(unit, new Damage(usv.Health))).Where(d => d != 0))
								healthValue = healthValue / dm * 100;

							return (int)(((float)(healthValue - usv.Health) / (float)usv.Health) * 100 * max);
						}
						else if (usv.Health < 0)
						{
							return 100;
						}
					}

					var healthTrait = unit.TraitOrDefault<Health>();
					if (healthTrait != null)
					{
						var healthValue = healthTrait.MaxHP;
						foreach (var dm in unit.TraitsImplementing<IDamageModifier>().Select(dm => dm.GetDamageModifier(unit, new Damage(healthTrait.MaxHP))).Where(d => d != 0))
							healthValue = healthValue / dm * 100;

						return (int)(((float)(healthValue - healthTrait.MaxHP) / (float)healthTrait.MaxHP) * 100 * max);
					}
				}

				return 0;
			};

			sight.GetText = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Sight > WDist.Zero)
						{
							var revealsShroudValue = usv.Sight;
							foreach (var rsm in unit.TraitsImplementing<IRevealsShroudModifier>().Select(rsm => rsm.GetRevealsShroudModifier()))
								revealsShroudValue = revealsShroudValue * rsm / 100;

							return sightText + ": " + Math.Round((float)revealsShroudValue.Length / 1024, 2).ToString();
						}
						else if (usv.Sight < WDist.Zero)
						{
							return sightText + ": Infinite";
						}
					}

					var revealsShroudTrait = unit.TraitsImplementing<RevealsShroud>().MaxBy(rs => rs.Info.Range);
					if (revealsShroudTrait != null)
					{
						var revealsShroudValue = revealsShroudTrait.Info.Range;
						foreach (var rsm in unit.TraitsImplementing<IRevealsShroudModifier>().Select(rsm => rsm.GetRevealsShroudModifier()))
							revealsShroudValue = revealsShroudValue * rsm / 100;

						return sightText + ": " + Math.Round((float)revealsShroudValue.Length / 1024, 2).ToString();
					}
				}

				return sightText + ":";
			};
			sightBar.GetPercentage = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Sight > WDist.Zero)
						{
							var revealsShroudValue = usv.Sight;
							foreach (var rsm in unit.TraitsImplementing<IRevealsShroudModifier>().Select(rsm => rsm.GetRevealsShroudModifier()))
								revealsShroudValue = revealsShroudValue * rsm / 100;

							return (int)(((float)(revealsShroudValue.Length - usv.Sight.Length) / (float)usv.Sight.Length) * 100 * max);
						}
						else if (usv.Sight < WDist.Zero)
						{
							return 100;
						}
					}

					var revealsShroudTrait = unit.TraitsImplementing<RevealsShroud>().MaxBy(rs => rs.Info.Range);
					if (revealsShroudTrait != null)
					{
						var revealsShroudValue = revealsShroudTrait.Info.Range;
						foreach (var rsm in unit.TraitsImplementing<IRevealsShroudModifier>().Select(rsm => rsm.GetRevealsShroudModifier()))
							revealsShroudValue = revealsShroudValue * rsm / 100;

						return (int)(((float)(revealsShroudValue.Length - revealsShroudTrait.Info.Range.Length) / (float)revealsShroudTrait.Info.Range.Length) * 100 * max);
					}
				}

				return 0;
			};

			damage.GetText = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Damage < 0)
							return damageText + ": Infinite";

						var damageValue = usv.Damage;
						foreach (var dm in unit.TraitsImplementing<IFirepowerModifier>().Select(fm => fm.GetFirepowerModifier()))
							damageValue = damageValue * dm / 100;

						return damageText + ": " + damageValue.ToString();
					}
				}

				return damageText + ":";
			};
			damageBar.GetPercentage = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Damage < 0)
							return 100;

						var damageValue = usv.Damage;
						foreach (var dm in unit.TraitsImplementing<IFirepowerModifier>().Select(fm => fm.GetFirepowerModifier()))
							damageValue = damageValue * dm / 100;

						return (int)(((float)(damageValue - usv.Damage) / (float)usv.Damage) * 100 * max);
					}
				}

				return 0;
			};

			rof.GetText = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.ReloadDelay > 0)
						{
							var rofValue = usv.ReloadDelay;
							foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
								rofValue = rofValue * rm / 100;

							return rofText + ": " + rofValue.ToString();
						}
						else if (usv.ReloadDelay < 0)
						{
							return rofText + ": Infinite";
						}
					}

					var armamanets = unit.TraitsImplementing<Armament>();
					if (armamanets.Any())
					{
						var rofValue = armamanets.Max(ar => ar.Weapon.ReloadDelay);
						foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
							rofValue = rofValue * rm / 100;

						return rofText + ": " + rofValue.ToString();
					}
				}

				return rofText + ":";
			};
			rofBar.GetPercentage = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.ReloadDelay > 0)
						{
							var rofValue = usv.ReloadDelay;
							foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
								rofValue = rofValue * rm / 100;

							return (int)(((float)(usv.ReloadDelay - rofValue) / (float)usv.ReloadDelay) * 100 * (max + 1));
						}
						else if (usv.ReloadDelay < 0)
						{
							return 100;
						}
					}

					var armamanets = unit.TraitsImplementing<Armament>();
					if (armamanets.Any())
					{
						var rofValue = armamanets.Max(ar => ar.Weapon.ReloadDelay);
						foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
							rofValue = rofValue * rm / 100;

						return (int)(((float)(armamanets.Max(ar => ar.Weapon.ReloadDelay - rofValue)) / (float)armamanets.Max(ar => ar.Weapon.ReloadDelay)) * 100 * (max + 1));
					}
				}

				return 0;
			};

			range.GetText = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Range > WDist.Zero)
						{
							var rangeValue = usv.Range;
							foreach (var rm in unit.TraitsImplementing<IRangeModifier>().Select(rm => rm.GetRangeModifier()))
								rangeValue = rangeValue * rm / 100;

							return rangeText + ": " + Math.Round((float)rangeValue.Length / 1024, 2).ToString();
						}
						else if (usv.Range < WDist.Zero)
						{
							return rangeText + ": Infinite";
						}
					}

					var attackBase = unit.TraitsImplementing<AttackBase>();
					if (attackBase.Any())
					{
						var rangeValue = attackBase.Max(ab => ab.GetMaximumRange());
						return rangeText + ": " + Math.Round((float)rangeValue.Length / 1024, 2).ToString();
					}
				}

				return rangeText + ":";
			};
			rangeBar.GetPercentage = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Range > WDist.Zero)
						{
							var rangeValue = usv.Range;
							foreach (var rm in unit.TraitsImplementing<IRangeModifier>().Select(rm => rm.GetRangeModifier()))
								rangeValue = rangeValue * rm / 100;

							return (int)(((float)(rangeValue.Length - usv.Range.Length) / (float)usv.Range.Length) * 100 * max);
						}
						else if (usv.Range < WDist.Zero)
						{
							return 100;
						}
					}

					var armamanets = unit.TraitsImplementing<Armament>();
					if (armamanets.Any())
					{
						var rangeValue = armamanets.Max(ar => ar.Weapon.Range);
						foreach (var rm in unit.TraitsImplementing<IRangeModifier>().Select(rm => rm.GetRangeModifier()))
							rangeValue = rangeValue * rm / 100;

						return (int)(((float)(rangeValue.Length - armamanets.Max(ar => ar.Weapon.Range.Length)) / (float)armamanets.Max(ar => ar.Weapon.Range.Length)) * 100 * max);
					}
				}

				return 0;
			};

			speed.GetText = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Speed > 0)
						{
							var speedValue = usv.Speed;
							foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
								speedValue = speedValue * sm / 100;

							return speedText + ": " + speedValue.ToString();
						}
						else if (usv.Speed < 0)
						{
							return speedText + ": Infinite";
						}
					}

					var mobile = unit.Info.TraitInfoOrDefault<MobileInfo>();
					if (mobile != null)
					{
						var speedValue = mobile.Speed;
						foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
							speedValue = speedValue * sm / 100;

						return speedText + ": " + speedValue.ToString();
					}

					var aircraft = unit.Info.TraitInfoOrDefault<AircraftInfo>();
					if (aircraft != null)
					{
						var speedValue = aircraft.Speed;
						foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
							speedValue = speedValue * sm / 100;

						return speedText + ": " + speedValue.ToString();
					}
				}

				return speedText + ":";
			};
			speedBar.GetPercentage = () =>
			{
				var renderPlayer = world.RenderPlayer;
				var unit = renderPlayer != null && renderPlayer.InternalName != "Everyone" ? spawner.Units[renderPlayer] : selection.Actors.FirstOrDefault(a => a.Info.HasTraitInfo<UnitStatValuesInfo>());
				if (unit != null && !unit.IsDead)
				{
					var usv = unit.Info.TraitInfoOrDefault<UnitStatValuesInfo>();
					if (usv != null)
					{
						if (usv.Speed > 0)
						{
							var speedValue = usv.Speed;
							foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
								speedValue = speedValue * sm / 100;

							return (int)(((float)(speedValue - usv.Speed) / (float)usv.Speed) * 100 * max);
						}
						else if (usv.Speed < 0)
						{
							return 100;
						}
					}

					var mobile = unit.Info.TraitInfoOrDefault<MobileInfo>();
					if (mobile != null)
					{
						var speedValue = mobile.Speed;
						foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
							speedValue = speedValue * sm / 100;

						return (int)(((float)(speedValue - mobile.Speed) / (float)mobile.Speed) * 100 * max);
					}

					var aircraft = unit.Info.TraitInfoOrDefault<AircraftInfo>();
					if (aircraft != null)
					{
						var speedValue = aircraft.Speed;
						foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
							speedValue = speedValue * sm / 100;

						return (int)(((float)(speedValue - aircraft.Speed) / (float)aircraft.Speed) * 100 * max);
					}
				}

				return 0;
			};
		}
	}
}
