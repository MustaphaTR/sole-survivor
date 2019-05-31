--[[
   Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

TimeLimitOption = Map.LobbyOption("time-limit")
TimeLimit =
{
	two = DateTime.Minutes(2),
	five = DateTime.Minutes(5),
	ten = DateTime.Minutes(10),
	fifteen = DateTime.Minutes(15),
	twenty = DateTime.Minutes(20),
	thirty = DateTime.Minutes(30)
}

TimeVictory = function()
	local highest = 0
	for _,player in pairs(players) do
		if not player.IsObjectiveFailed(0) and player.Experience > highest then
			highest = player.Experience
		end
	end

	local winners = Utils.Where(players, function(p) return p.Experience == highest end)
	if #winners ~= 1 then
		UserInterface.SetMissionText("Tie!", HSLColor.White)
	else
		for _,player in pairs(players) do
			if winners[1] == player or (player.Team ~= 0 and player.Team == winners[1].Team) then
				player.MarkCompletedObjective(0)
			else
				player.MarkFailedObjective(0)
			end
		end
	end
end

TimePassed = 0
TimeLimitTick = function()
	if TimeLimitOption ~= "disabled" then
		TimePassed = TimePassed + 1

		if TimePassed > TimeLimit[TimeLimitOption] then
			TimeVictory()
		else
			UserInterface.SetMissionText("Time remaining: " .. Utils.FormatTime(TimeLimit[TimeLimitOption] - TimePassed), HSLColor.White)
		end
	end
end

TimeLimitWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)
end
