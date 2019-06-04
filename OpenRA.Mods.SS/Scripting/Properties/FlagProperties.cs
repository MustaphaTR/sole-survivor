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

using OpenRA.Mods.SS.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Scripting
{
    [ScriptPropertyGroup("General")]
    public class CarriesFlagProperties : ScriptActorProperties, Requires<CarriesFlagInfo>
    {
        readonly CarriesFlag carriesFlag;
        readonly Actor actor;

        public CarriesFlagProperties(ScriptContext context, Actor actor)
            : base(context, actor)
        {
            this.actor = actor;
            carriesFlag = actor.Trait<CarriesFlag>();
        }

        [Desc("Returns the flag actor is carrying.")]
        public Actor Flag
        {
            get { return carriesFlag.Flag; }
        }

        [Desc("Drops the Flag actor is carrying and returns to the dropped Flag.")]
        public Actor DropFlag()
        {
            var flag = carriesFlag.Flag;
            if (flag != null)
                carriesFlag.DropFlag(actor);

            return flag;
        }
    }
}
