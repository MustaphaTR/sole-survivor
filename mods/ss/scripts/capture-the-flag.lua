--[[
   Copyright (c) The OpenRA Developers and Contributors
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

CtFOption = Map.LobbyOption("ctf-mode")
EmptyDrop = Player.GetPlayer("Neutral").HasPrerequisites({ "global-emptydrop" })
FlagCircles = { }
Flags = { }

CtFTick = function()

end

OnCircle = function(player, circle)
	Trigger.AfterDelay(0, function()
		if not player.Unit.IsDead and player.Unit.Flag ~= nil and not player.Unit.Flag.IsDead then
			if circle.Owner == player or (circle.Owner.Team ~= 0 and circle.Owner.Team == player.Team) then
				if player.Unit.Location == circle.Location then
					local flagOnMap = true
					if not EmptyDrop then
						for _,other in pairs(players) do
							if not other.Unit.IsDead and other.Unit.Flag == Flags[circle.Owner.InternalName] then
								flagOnMap = false
								break
							end
						end
					end

					if player.Unit.Flag.Owner.Team == 0 or player.Unit.Flag.Owner.Team ~= circle.Owner.Team or player.Unit.Flag.Owner == circle.Owner then
						if EmptyDrop or (flagOnMap and not Flags[circle.Owner.InternalName].IsDead and Flags[circle.Owner.InternalName].Location == FlagCircles[circle.Owner.InternalName].Location) or player.Unit.Flag == Flags[circle.Owner.InternalName] then
							local flag = player.Unit.DropFlag()
							if flag.Owner ~= circle.Owner then
								player.Experience = player.Experience + 40
								if flag.Owner.Team == 0 or not Lobby.TeamSpawns() then
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

					local cells = GetCells.FindTilesInAnnulus(player.SpawnCellPosition, 3, 4)
					local cell = Utils.Random(Utils.Where(cells, function(c) return player.Unit.CanEnterCell(c) end))
					player.Unit.Teleport(cell)
					SpawnPoints[player.InternalName] = cell
				end

				Trigger.OnEnteredFootprint({ FlagCircles[player.TeamLeader.InternalName].Location }, function(a, id)
					if not a.Owner.IsNonCombatant then
						OnCircle(a.Owner, FlagCircles[player.TeamLeader.InternalName])
					end
				end)
			end
		end)
	end
end
