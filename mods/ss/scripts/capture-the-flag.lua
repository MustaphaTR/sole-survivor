--[[
   Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

CtFOption = Map.LobbyOption("ctf-mode")
FlagCircles = { }
Flags = { }

CtFTick = function()

end

OnCircle = function(player)
	Trigger.AfterDelay(0, function()
		if not player.Unit.IsDead then
			local circle = FlagCircles[player.TeamLeader.InternalName]
			if player.Unit.Location == circle.Location then
				local flag = player.Unit.DropFlag()
				if flag ~= nil then
					if flag.Owner ~= player.TeamLeader then
						player.Experience = player.Experience + 40
						if flag.Owner.Team == 0 then
							Media.DisplaySystemMessage(player.Name .. " has captured flag of " .. flag.Owner.Name, "Battlefield Control")
						else
							Media.DisplaySystemMessage(player.Name .. " has captured flag of Team " .. flag.Owner.Team, "Battlefield Control")
						end
						if CtFOption == "score" then
							flag.Teleport(FlagCircles[flag.Owner.InternalName].Location)
						else
							flag.Destroy()
							for _,loser in pairs(Utils.Where(players, function(p) return p.TeamLeader == flag.Owner end)) do
								loser.MarkFailedObjective(0)
							end
						end
					end
				end
			end
		end
	end)
end

CtFWorldLoaded = function()
	if CtFOption ~= "disabled" then
		players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)

		Trigger.AfterDelay(0, function()
			for _,player in pairs(players) do
				if player == player.TeamLeader then
					FlagCircles[player.InternalName] = Actor.Create("flagcircle", true, { Owner = player, Location = player.SpawnCellPosition })
					Flags[player.InternalName] = Actor.Create("flag", true, { Owner = player, Location = player.SpawnCellPosition })
				end

				Trigger.OnEnteredFootprint({ FlagCircles[player.TeamLeader.InternalName].Location }, function()
					OnCircle(player)
				end)
			end
		end)
	end
end
