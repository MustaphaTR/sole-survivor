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

using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;
using OpenRA.Scripting;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Scripting.Global
{
	[ScriptGlobal("SSUserInterface")]
	public class SSUserInterfaceGlobal : ScriptGlobal
	{
		public SSUserInterfaceGlobal(ScriptContext context)
			: base(context) { }

		[Desc("Displays a large text message at the center of the screen.")]
		public void SetCenteralText(string text, Color? color = null)
		{
			var luaLabel = Ui.Root.Get("INGAME_ROOT").Get<LabelWidget>("MISSION_CENTER_TEXT");
			luaLabel.GetText = () => text;

			var c = color ?? Color.White;
			luaLabel.GetColor = () => c;
		}
	}
}
