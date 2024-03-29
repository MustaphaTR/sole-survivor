﻿#region Copyright & License Information
/*
 * Copyright 2007-2022 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Scripting
{
    [ScriptPropertyGroup("Positionable")]
    public class PositionableProperties : ScriptActorProperties, Requires<IPositionableInfo>
    {
        readonly IPositionableInfo ip;
        readonly Actor actor;

        public PositionableProperties(ScriptContext context, Actor actor)
            : base(context, actor)
        {
            this.actor = actor;
            ip = actor.Info.TraitInfo<IPositionableInfo>();
        }

        [Desc("Returns true if the actor can enter the specified cell.")]
        public bool CanEnterCell(CPos cell)
        {
            return ip.CanEnterCell(actor.World, null, cell);
        }
    }
}
