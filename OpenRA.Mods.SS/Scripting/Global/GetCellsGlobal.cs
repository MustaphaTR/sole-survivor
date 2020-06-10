﻿#region Copyright & License Information
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
using OpenRA.Scripting;

namespace OpenRA.Mods.SS.Scripting
{
    [ScriptGlobal("GetCells")]
    public class GetCellsGlobal : ScriptGlobal
    {
        readonly World world;

        public GetCellsGlobal(ScriptContext context)
            : base(context)
        {
            world = context.World;
        }

        [Desc("Gets cells in an annulus around a cell.")]
        public CPos[] FindTilesInAnnulus(CPos cell, int inner, int outer)
        {
            return world.Map.FindTilesInAnnulus(cell, inner, outer).ToArray();
        }
    }
}