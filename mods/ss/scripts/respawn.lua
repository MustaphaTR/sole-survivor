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
	ten = DateTime.Seconds(10),
	fifteen = DateTime.Seconds(15),
	thirty = DateTime.Seconds(30),
	forthfive = DateTime.Seconds(45),
	sixty = DateTime.Seconds(60)
}

PenaltyOption = Map.LobbyOption("death-penalty")
Penalty =
{
	disabled = 0;
	five = 5,
	ten = 10,
	twenty = 20
}

SpawnPoints = { }

Respawn = function(player)
	Trigger.OnKilled(player.Unit, function()
		if not player.IsObjectiveFailed(0) and not player.IsObjectiveCompleted(0) then
			player.Experience = player.Experience - Penalty[PenaltyOption]

			TickTimer(player, RespawnDelay[RespawnOption])

			if player.Class ~= "e6" then
				local others = player.GetActorsByTypes( { "gtwr", "gun" } )

				Utils.Do(others, function(other)
					other.Destroy()
				end)
			elseif player.Class ~= "mhq" then
				local others = player.GetActorsByTypes( { "u2", "a10" } )

				Utils.Do(others, function(other)
					other.Destroy()
				end)
			end

			Trigger.AfterDelay(RespawnDelay[RespawnOption], function()
				if not player.IsObjectiveFailed(0) and not player.IsObjectiveCompleted(0) then
					if ReshroudOnDeath and not player.ExploreMapEnabled then
						player.ReshroudMap()
					end

					local unitType = player.Class

					local location = SpawnPoints[player.InternalName]
					if RandomRespawn then
						location = Utils.Random(SpawnPoints)
					end

					player.Unit = Actor.Create(unitType, true, { Owner = player, Location = location })
					if player.IsLocalPlayer then
						Camera.Position = Map.CenterOfCell(location)
					end
					Respawn(player)
				end
			end)
		end
	end)
end

TickTimer = function(player, timer)
	if player.IsLocalPlayer then
		if timer <= 0 or player.IsObjectiveFailed(0) or player.IsObjectiveCompleted(0) then
			SSUserInterface.SetCenteralText("")

			return
		end

		SSUserInterface.SetCenteralText("Respawn in " .. Utils.FormatTime(timer))

		Trigger.AfterDelay(DateTime.Seconds(1), function()
			TickTimer(player, timer - DateTime.Seconds(1))
		end)
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
				else
					player.AddPrimaryObjective("Why are we still here? Just to suffer?")

					if not warningShown then
						Media.DisplaySystemMessage("You are playing without any victory conditions. The game cannot end!", "Battlefield Control")
						warningShown = true
					end
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
			Media.DisplaySystemMessage(player.Name .. " has been defeated!", "Battlefield Control")
			Trigger.AfterDelay(DateTime.Seconds(1), function()
				Media.PlaySpeechNotification(player, "Lose")
			end)

			local units = Utils.Where(player.GetActors(), function(a) return a.HasProperty("Kill") end)
			Utils.Do(units, function(unit)
				unit.Kill()
			end)

			local husks = player.GetActorsByTypes( { "gtwr.husk", "gun.husk" } )
			Utils.Do(husks, function(husk)
				husk.Owner = neutral
			end)

			if not timeout then
				for _,other in pairs(players) do
					if not other.IsObjectiveFailed(0) and not other.IsObjectiveCompleted(0) then
						local win = true
						for _,enemy in pairs(Utils.Where(players, function(p) return p ~= other and (p.Team == 0 or p.Team ~= other.Team) end)) do
							if not enemy.IsObjectiveFailed(0) then
								win = false
							end
						end
						if win then
							other.MarkCompletedObjective(0)
						end
					end
				end
			end
		end)
		Trigger.OnPlayerWon(player, function()
			Media.DisplaySystemMessage(player.Name .. " is victorious!", "Battlefield Control")
			Trigger.AfterDelay(DateTime.Seconds(1), function()
				Media.PlaySpeechNotification(player, "Win")
			end)
		end)
	end
end

RespawnTick = function()

end

RespawnWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)
	neutral = Player.GetPlayer("Neutral")

	RandomRespawn = neutral.HasPrerequisites({ "global-randomrespawn" })
	ReshroudOnDeath = neutral.HasPrerequisites({ "global-reshroudondeath" })

	SetupObjectives()
	for _,player in pairs(players) do
		if RespawnOption ~= "disabled" then
			Respawn(player)

			SpawnPoints[player.InternalName] = player.SpawnCellPosition
		else
			player.ClassChanging = false
			Trigger.OnKilled(player.Unit, function()
				player.MarkFailedObjective(0)
			end)
		end
	end
end
