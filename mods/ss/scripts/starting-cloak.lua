--[[
   Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

StartingCloakOption = Map.LobbyOption("starting-cloak")
CloakLimit =
{
	disabled = 0,
	ten = DateTime.Seconds(10),
	fifteen = DateTime.Seconds(15),
	twenty = DateTime.Seconds(20),
	thirty = DateTime.Seconds(30),
	fortyfive = DateTime.Seconds(45),
	sixty = DateTime.Minutes(1)
}

cloakticks = 0
CloakTick = function()
	if cloakticks < CloakLimit[StartingCloakOption] then
		cloakticks = cloakticks + 1
	end
end

CloakWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)

	for _,player in pairs(players) do
		if StartingCloakOption ~= "disabled" then
			local condition = Units[player.InternalName].GrantCondition("starting-cloak")

			Trigger.AfterDelay(CloakLimit[StartingCloakOption], function()
				if not Units[player.InternalName].IsDead then
					Units[player.InternalName].RevokeCondition(condition)
				end
			end)
		end
	end
end
