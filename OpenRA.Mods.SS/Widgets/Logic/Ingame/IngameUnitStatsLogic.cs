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
using OpenRA.Traits;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
    public class IngameUnitStatsLogic : ChromeLogic
    {

        [ObjectCreator.UseCtor]
        public IngameUnitStatsLogic(Widget widget, World world)
        {
            var health = widget.Get<LabelWidget>("STAT_HEALTH");
            // var damage = widget.Get<LabelWidget>("STAT_DAMAGE");
            var range = widget.Get<LabelWidget>("STAT_RANGE");
            var rof = widget.Get<LabelWidget>("STAT_ROF");
            var speed = widget.Get<LabelWidget>("STAT_SPEED");
            
            health.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var healthVaue = unit.Trait<Health>().MaxHP;
                    foreach (var dm in unit.TraitsImplementing<DamageMultiplier>().Where(dm => !dm.IsTraitDisabled).Select(dm => dm.Info.Modifier))
                        healthVaue = healthVaue / dm * 100;

                    return health.Text + ": " + healthVaue.ToString();
                }

                return "";
            };

            rof.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var rofVaue = unit.TraitsImplementing<Armament>().Max(ar => ar.Weapon.ReloadDelay);
                    foreach (var rm in unit.TraitsImplementing<IReloadModifier>().Select(sm => sm.GetReloadModifier()))
                        rofVaue = rofVaue * rm / 100;

                    return rof.Text + ": " + rofVaue.ToString();
                }

                return "";
            };

            range.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    return range.Text + ": " + (unit.IsDead ? "" : unit.TraitOrDefault<AttackBase>().GetMaximumRange().ToString());
                }

                return "";
            };

            speed.GetText = () =>
            {
                var units = world.Actors.Where(a => a.Owner == world.LocalPlayer && !a.IsDead && a.TraitOrDefault<Mobile>() != null);

                if (units.Any())
                {
                    var unit = units.First();
                    var speedValue = unit.Trait<Mobile>().Info.Speed;
                    foreach (var sm in unit.TraitsImplementing<ISpeedModifier>().Select(sm => sm.GetSpeedModifier()))
                        speedValue = speedValue * sm / 100;

                    return speed.Text + ": " + speedValue.ToString();
                }

                return "";
            };
        }
    }
}
