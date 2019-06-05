--[[
   Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

RespawnOption = Map.LobbyOption("respawn-delay")
RespawnDelay =
{
	zero = 0;
	one = DateTime.Seconds(1),
	two = DateTime.Seconds(2),
	three = DateTime.Seconds(3),
	five = DateTime.Seconds(5),
	ten = DateTime.Seconds(10)
}

RespawnTimers = { }
SetupRespawnTimers = function()
	for _,player in pairs(players) do
		RespawnTimers[player.InternalName] = RespawnDelay[RespawnOption]
	end
end

RespawnTick = function()
	for _,player in pairs(players) do
		if Units[player.InternalName] ~= nil then
			if Units[player.InternalName].IsDead then
				if RespawnOption ~= "disabled" then
					if RespawnTimers[player.InternalName] == RespawnDelay[RespawnOption] then
						player.Experience = player.Experience - 20
					end

					RespawnTimers[player.InternalName] = RespawnTimers[player.InternalName] - 1

					if RespawnTimers[player.InternalName] < 0 then
						local unitType = Units[player.InternalName].Type

						Units[player.InternalName] = Actor.Create(unitType, true, { Owner = player, Location = player.SpawnCellPosition })
						RespawnTimers[player.InternalName] = RespawnDelay[RespawnOption]
						if player.IsLocalPlayer then
							Camera.Position = player.SpawnWorldPosition
						end
					end
				else
					player.MarkFailedObjective(0)
						
					local buildings = player.GetActorsByTypes( { "gtwr", "gun" } )
					Utils.Do(buildings, function(building)
						building.Kill()
					end)

					local husks = player.GetActorsByTypes( { "gtwr.husk", "gun.husk" } )
					Utils.Do(husks, function(husk)
						husk.Owner = neutral
					end)
				end
			end
		end

		local win = true
		for _,enemy in pairs(Utils.Where(players, function(p) return p ~= player and (p.Team == 0 or p.Team ~= player.Team) end)) do
			if not enemy.IsObjectiveFailed(0) then
				win = false
			end
		end
		if win then
			player.MarkCompletedObjective(0)
		end
	end
end

SetupObjectives = function()
	local warningShown
	for _,player in pairs(players) do
		if RespawnOption ~= "disabled" then
			if TimeLimitOption ~= "disabled" then
				if CtFOption == "victory" then
					player.AddPrimaryObjective("Have the highest score when timer expires\nor capture all enemy flags.")
				else
					player.AddPrimaryObjective("Have the highest score when timer expires.")
				end
			else
				if CtFOption == "victory" then
					player.AddPrimaryObjective("Capture all enemy flags.")
				elseif not warningShown then
					Media.DisplayMessage("You are playing without any victory conditions. The game cannot end!")
					warningShown = true
				end
			end
		else
			if TimeLimitOption ~= "disabled" then
				if CtFOption == "victory" then
					player.AddPrimaryObjective("Be the last survivor, have the highest score when timer expires\nor capture all enemy flags.")
				else
					player.AddPrimaryObjective("Be the last survivor or have the highest score when timer expires.")
				end
			else
				if CtFOption == "victory" then
					player.AddPrimaryObjective("Be the last survivor or capture all enemy flags.")
				else
					player.AddPrimaryObjective("Be the last survivor.")
				end
			end
		end

		Trigger.OnPlayerLost(player, function()
			Trigger.AfterDelay(DateTime.Seconds(1), function()
				Media.PlaySpeechNotification(player, "Lose")
			end)
		end)
		Trigger.OnPlayerWon(player, function()
			Trigger.AfterDelay(DateTime.Seconds(1), function()
				Media.PlaySpeechNotification(player, "Win")
			end)
		end)

		Trigger.OnPlayerLost(player, function()
			local units = Utils.Where(player.GetActors(), function(a) return a.HasProperty("Kill") end)
			Utils.Do(units, function(unit)
				unit.Kill()
			end)
		end)
	end
end

RespawnWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)
	neutral = Player.GetPlayer("Neutral")

	SetupRespawnTimers()
	SetupObjectives()
end
