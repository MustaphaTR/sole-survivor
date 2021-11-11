--[[
   Copyright 2007-2021 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

RespawnOption = Map.LobbyOption("respawn-delay")
ClassChangingOption = Map.LobbyOption("class-changing")
ClassChangingInterval =
{
	fifteen = DateTime.Seconds(15),
	thirty = DateTime.Seconds(30),
	forthfive = DateTime.Seconds(45),
	sixty = DateTime.Seconds(60),
	hundredtwenty = DateTime.Seconds(120),
	threehundred = DateTime.Seconds(300)
}

TickClassChangingTimer = function(player, timer)
	if timer <= 0 then
		player.ClassChangingPaused = false

		return
	end

	Trigger.AfterDelay(DateTime.Seconds(1), function()
		TickClassChangingTimer(player, timer - DateTime.Seconds(1))
	end)
end

SetupTimers = function()
	for _,player in pairs(humanPlayers) do -- AI doesn't know how to change classes, let's not waste performance checking them.
		player.ClassChangingPaused = true

		TickClassChangingTimer(player, ClassChangingInterval[ClassChangingOption])
	end
end

ClassChangingTick = function()

end

ClassChangingWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)
	humanPlayers = Player.GetPlayers(function(p) return not p.IsNonCombatant and not p.IsBot end)

	if ClassChangingOption ~= "disabled" and RespawnOption ~= "disabled" then
		SetupTimers()
	else
		for _,player in pairs(players) do
			player.ClassChanging = false
		end
	end
end
