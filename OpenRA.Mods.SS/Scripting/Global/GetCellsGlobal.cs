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

		[Desc("Gets if a cell is in the playable are of the map.")]
		public bool TileIsInMap(CPos cell)
		{
			return world.Map.Contains(cell);
		}
	}
}
