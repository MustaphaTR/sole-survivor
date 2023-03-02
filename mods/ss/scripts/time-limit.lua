--[[
   Copyright (c) The OpenRA Developers and Contributors
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

TeamScoreVictoryOption = Map.LobbyOption("team-score-victory")
TimeLimitOption = Map.LobbyOption("time-limit")
TimeLimit =
{
	two = DateTime.Minutes(2),
	five = DateTime.Minutes(5),
	ten = DateTime.Minutes(10),
	fifteen = DateTime.Minutes(15),
	twenty = DateTime.Minutes(20),
	thirty = DateTime.Minutes(30),
	fortyfive = DateTime.Minutes(45),
	sixty = DateTime.Minutes(60)
}

TeamMembers = { }

TimeVictory = function()
	local TeamScores = { }
	local playersStillIn = Utils.Where(players, function(p) return not p.IsObjectiveFailed(0) end)
	for _,player in pairs(players) do -- Score of dead players is still counted for the team, but only if there is someone alive in the team.
		if TeamScoreVictoryOption ~= "player" and player.Team ~= 0 and Utils.Any(TeamMembers[player.Team], function(p) return not p.IsObjectiveFailed(0) end) then
			if TeamScores[player.Team] == nil then
				TeamScores[player.Team] = player.Experience
			else
				TeamScores[player.Team] = TeamScores[player.Team] + player.Experience
			end
		end
	end
	if TeamScoreVictoryOption == "average" then
		for i,teamScore in pairs(TeamScores) do
			TeamScores[i] = teamScore / #TeamMembers[i]
		end
	end
	local highest = 0
	for _,player in pairs(playersStillIn) do
		if TeamScoreVictoryOption == "player" or player.Team == 0 then
			if player.Experience > highest then
				highest = player.Experience
			end
		else
			if TeamScores[player.Team] > highest then
				highest = TeamScores[player.Team]
			end
		end
	end

	local noTeamWinners = Utils.Where(playersStillIn, function(p) return p.Team == 0 and p.Experience == highest end)
	local teamWinners = Utils.Where(TeamScores, function(ts) return ts == highest end)
	if #noTeamWinners + #teamWinners ~= 1 then
		UserInterface.SetMissionText("Tie!", HSLColor.White)
	else
		timeout = true

		if #noTeamWinners == 1 then
			for _,player in pairs(playersStillIn) do
				if noTeamWinners[1] == player then
					player.MarkCompletedObjective(0)
				else
					player.MarkFailedObjective(0)
				end
			end
		elseif #teamWinners == 1 then 
			for _,player in pairs(playersStillIn) do
				if teamWinners[1] == TeamScores[player.Team] then
					player.MarkCompletedObjective(0)
				else
					player.MarkFailedObjective(0)
				end
			end
		end
	end
end

TimePassed = 0
TimeLimitTick = function()
	if TimeLimitOption ~= "disabled" and not timeout then
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

	for _,player in pairs(players) do
		if player.Team ~= 0 then
			if TeamMembers[player.Team] == nil then
				TeamMembers[player.Team] = { player }
			else
				TeamMembers[player.Team][#TeamMembers[player.Team] + 1] = player
			end
		end
	end
end
