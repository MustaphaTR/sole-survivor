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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SS.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
    public class IngameUnitStatsLogic : ChromeLogic
    {

        [ObjectCreator.UseCtor]
        public IngameUnitStatsLogic(Widget widget, World world)
        {
            var health = widget.Get<LabelWidget>("STAT_HEALTH");
            var sight = widget.Get<LabelWidget>("STAT_SIGHT");
            var damage = widget.Get<LabelWidget>("STAT_DAMAGE");
            var range = widget.Get<LabelWidget>("STAT_RANGE");
            var rof = widget.Get<LabelWidget>("STAT_ROF");
            var speed = widget.Get<LabelWidget>("STAT_SPEED");
            
            health.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<UnitStatValues>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var usv = unit.Info.TraitInfo<UnitStatValuesInfo>();
                    if (usv.Health > 0)
                    {
                        var healthVaue = usv.Health;
                        foreach (var dm in unit.TraitsImplementing<DamageMultiplier>().Where(dm => !dm.IsTraitDisabled).Select(dm => dm.Info.Modifier))
                            healthVaue = healthVaue / dm * 100;

                        return health.Text + ": " + healthVaue.ToString();
                    }
                    else if (usv.Health < 0)
                    {
                        return health.Text + ": Infinite";
                    }
                    var healthTrait = unit.TraitOrDefault<Health>();
                    if (healthTrait != null)
                    {
                        var healthVaue = healthTrait.MaxHP;
                        foreach (var dm in unit.TraitsImplementing<DamageMultiplier>().Where(dm => !dm.IsTraitDisabled).Select(dm => dm.Info.Modifier))
                            healthVaue = healthVaue / dm * 100;

                        return health.Text + ": " + healthVaue.ToString();
                    }
                }

                return health.Text + ":";
            };

            sight.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<UnitStatValues>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var usv = unit.Info.TraitInfo<UnitStatValuesInfo>();
                    if (usv.Sight > 0)
                    {
                        var revealsShroudValue = usv.Sight;
                        foreach (var rsm in unit.TraitsImplementing<IRevealsShroudModifier>().Select(rsm => rsm.GetRevealsShroudModifier()))
                            revealsShroudValue = revealsShroudValue * rsm / 100;

                        return sight.Text + ": " + revealsShroudValue.ToString();
                    }
                    else if (usv.Sight < 0)
                    {
                        return sight.Text + ": Infinite";
                    }
                    var revealsShroudTrait = unit.TraitsImplementing<RevealsShroud>().MaxBy(rs => rs.Info.Range);
                    if (revealsShroudTrait != null)
                    {
                        var revealsShroudValue = revealsShroudTrait.Info.Range;
                        foreach (var rsm in unit.TraitsImplementing<IRevealsShroudModifier>().Select(rsm => rsm.GetRevealsShroudModifier()))
                            revealsShroudValue = revealsShroudValue * rsm / 100;

                        return sight.Text + ": " + revealsShroudValue.ToString();
                    }
                }

                return sight.Text + ":";
            };

            damage.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var usv = unit.Info.TraitInfo<UnitStatValuesInfo>();
                    if (usv.Damage < 0)
                        return damage.Text + ": Infinite";

                    var damageValue = usv.Damage;
                    foreach (var dm in unit.TraitsImplementing<IFirepowerModifier>().Select(fm => fm.GetFirepowerModifier()))
                        damageValue = damageValue * dm / 100;

                    return damage.Text + ": " + damageValue.ToString();
                }

                return damage.Text + ":";
            };

            rof.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var usv = unit.Info.TraitInfo<UnitStatValuesInfo>();
                    if (usv.ReloadDelay > 0)
                    {
                        var rofValue = usv.ReloadDelay;
                        foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
                            rofValue = rofValue * rm / 100;

                        return rof.Text + ": " + rofValue.ToString();
                    }
                    else if (usv.ReloadDelay < 0)
                    {
                        return rof.Text + ": Infinite";
                    }
                    var armamanets = unit.TraitsImplementing<Armament>();
                    if (armamanets.Any())
                    {
                        var rofValue = armamanets.Max(ar => ar.Weapon.ReloadDelay);
                        foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
                            rofValue = rofValue * rm / 100;

                        return rof.Text + ": " + rofValue.ToString();
                    }
                }

                return rof.Text + ":";
            };

            range.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var usv = unit.Info.TraitInfo<UnitStatValuesInfo>();
                    if (usv.Range > WDist.Zero)
                    {
                        var rangeValue = usv.Range;
                        foreach (var rm in unit.TraitsImplementing<IRangeModifier>().Select(rm => rm.GetRangeModifier()))
                            rangeValue = rangeValue * rm / 100;

                        return range.Text + ": " + rangeValue.ToString();
                    }
                    else if (usv.Range < WDist.Zero)
                    {
                        return range.Text + ": Infinite";
                    }
                    var attackBase = unit.TraitOrDefault<AttackBase>();
                    if (attackBase != null)
                    {
                        var rangeValue = attackBase.GetMaximumRange();
                        return range.Text + ": " + rangeValue.ToString();
                    }
                }

                return range.Text + ":";
            };

            speed.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var usv = unit.Info.TraitInfo<UnitStatValuesInfo>();
                    if (usv.Speed > 0)
                    {
                        var speedValue = usv.Speed;
                        foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
                            speedValue = speedValue * sm / 100;

                        return speed.Text + ": " + speedValue.ToString();
                    }
                    else if (usv.Speed < 0)
                    {
                        return speed.Text + ": Infinite";
                    }
                    var mobile = unit.Info.TraitInfoOrDefault<MobileInfo>();
                    if (mobile != null)
                    {
                        var speedValue = mobile.Speed;
                        foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
                            speedValue = speedValue * sm / 100;

                        return speed.Text + ": " + speedValue.ToString();
                    }
                    var aircraft = unit.Info.TraitInfoOrDefault<AircraftInfo>();
                    if (aircraft != null)
                    {
                        var speedValue = aircraft.Speed;
                        foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
                            speedValue = speedValue * sm / 100;

                        return speed.Text + ": " + speedValue.ToString();
                    }
                }

                return speed.Text + ":";
            };
        }
    }
}
