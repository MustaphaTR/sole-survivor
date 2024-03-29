--[[
   Copyright (c) The OpenRA Developers and Contributors
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

StartingCloakOption = Map.LobbyOption("starting-cloak")
CloakDuration =
{
	disabled = 0,
	five = DateTime.Seconds(5),
	ten = DateTime.Seconds(10),
	fifteen = DateTime.Seconds(15),
	twenty = DateTime.Seconds(20),
	thirty = DateTime.Seconds(30),
	fortyfive = DateTime.Seconds(45),
	sixty = DateTime.Minutes(1)
}

StartingInvulnOption = Map.LobbyOption("starting-invuln")
InvulnDuration =
{
	disabled = 0,
	five = DateTime.Seconds(5),
	ten = DateTime.Seconds(10),
	fifteen = DateTime.Seconds(15),
	twenty = DateTime.Seconds(20),
	thirty = DateTime.Seconds(30),
	fortyfive = DateTime.Seconds(45),
	sixty = DateTime.Minutes(1)
}

CloakInvulnTick = function()

end

CloakInvulnWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)

	Trigger.AfterDelay(0, function()
		for _,player in pairs(players) do
			if StartingCloakOption ~= "disabled" then
				player.Unit.GrantCondition("starting-cloak", CloakDuration[StartingCloakOption])
			end
			if StartingInvulnOption ~= "disabled" then
				player.Unit.GrantCondition("invulnerability", InvulnDuration[StartingInvulnOption])
			end
		end
	end)
end
