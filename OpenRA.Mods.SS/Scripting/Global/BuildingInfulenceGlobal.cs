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

using System.Linq;
using Eluant;
using OpenRA.Mods.Common.Scripting;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.SS.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.SS.Scripting
{
    [ScriptGlobal("BuildingInfluence")]
    public class BuildingInfluenceGlobal : ScriptGlobal
    {
        readonly World world;

        public BuildingInfluenceGlobal(ScriptContext context)
            : base(context)
        {
            world = context.World;
        }

        [Desc("Returns true if the cell contains a building.")]
        public Actor BuildingAtCell(CPos cell)
        {
            var bi = world.WorldActor.TraitOrDefault<BuildingInfluence>();

            return bi.GetBuildingAt(cell);
        }
    }
}
