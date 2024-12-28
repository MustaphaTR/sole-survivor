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

using Eluant;
using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptGlobal("Positionable")]
	public class PositionableGlobal : ScriptGlobal
	{
		public PositionableGlobal(ScriptContext context)
			: base(context) { }

		[Desc("Returns true if the speicified actor type can enter the specified cell.")]
		public bool CanEnterCell(string type, CPos cell)
		{
			if (!Context.World.Map.Rules.Actors.TryGetValue(type, out var ai))
				throw new LuaException($"Unknown actor type '{type}'");

			var ip = ai.TraitInfoOrDefault<IPositionableInfo>();
			if (ip == null)
				return false;

			return ip.CanEnterCell(Context.World, null, cell);
		}
	}
}
