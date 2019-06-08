--[[
   Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

WackyTick = function()

end

WackyWorldLoaded = function()
	if Lobby.Wacky() then
		players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)

		for _,player in pairs(players) do
			Actor.Create("proxy.wacky", true, { Owner = player })
		end
	end
end
