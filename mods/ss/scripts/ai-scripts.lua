--[[
   Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

Bots = { { }, { }, { }, { }, { } }
CheckTimers = {
	Health = { 25, 25, 25, 25, 25 },
	Demolish = { 25, 25, 25, 25, 25 }
}

MHQActor = { }

IdleHunt = function(actor)
	if actor.HasProperty("Hunt") and not actor.IsDead then
		Trigger.OnIdle(actor, actor.Hunt)
	end
end

BuildEngiStuff = function(engi)
	if engi.IsDead then
		return
	end

	local engiLoc = engi.Location
	local locations =
	{
		engiLoc + CVec.New(-1,-1),
		engiLoc + CVec.New(-1, 0),
		engiLoc + CVec.New(-1, 1),
		engiLoc + CVec.New( 0,-1),
		engiLoc + CVec.New( 0, 0),
		engiLoc + CVec.New( 0, 1),
		engiLoc + CVec.New( 1,-1),
		engiLoc + CVec.New( 1, 0),
		engiLoc + CVec.New( 1, 1)
	}

	local buildableLocations = Utils.Where(locations, function(l) return (Map.TerrainType(l) == "Clear" or Map.TerrainType(l) == "Road") and #Map.ActorsInBox(Map.CenterOfCell(l) + WVec.New(-512, -512, 0), Map.CenterOfCell(l) + WVec.New(512, 512, 0)) == 0 end)

	if #buildableLocations > 0 then
		local towerLocation = Utils.Random(buildableLocations)

		if #engi.Owner.GetActorsByType("gtwr") == 0 then
			Actor.Create("gtwr", true, { Owner = engi.Owner, Location = towerLocation })
		end

		if #buildableLocations > 1 then
			local turretLocation = Utils.Random(Utils.Where(buildableLocations, function(l) return l ~= towerLocation end))

			if #engi.Owner.GetActorsByType("gun") == 0 then
				Actor.Create("gun",  true, { Owner = engi.Owner, Location = turretLocation })
			end
		end
	end
end

MHQSpyPlane = function(mhq)
	MHQActor[mhq.Owner.InternalName] = mhq

	Trigger.AfterDelay(550, function()
		if mhq.IsDead then
			return
		end

		local cell = Map.RandomCell()
		mhq.TargetAirstrike(WPos.New(cell.X * 1024, cell.Y * 1024, 0))

		MHQSpyPlane(mhq)
	end)
end

GetNearbyCrates = function(actor, distance)
	return Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type == "crate" or a.Type == "upgradecrate" or a.Type == "experiencecrate" or a.Type == "wackycrate" end)
end

GetNearbyEnemies = function(actor, distance)
	return Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type ~= "e1" and a.Type ~= "e2" and a.Type ~= "e3" and a.Type ~= "e4" and a.Type ~= "e5" and a.Type ~= "e6" and a.Type ~= "rmbo" and a.Type ~= "tran" and a.Type ~= "heli" and a.Type ~= "orca" and a.Type ~= "a10" and a.Type ~= "u2" and not a.Owner.IsNonCombatant and (a.Owner.Team ~= actor.Owner.Team or a.Owner.Team == 0) end)
end

GetNearbyHealCrate = function(actor, distance)
	local crates = Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type == "healcrate" end)

	if #crates == 0 then
		return nil
	end

	return Utils.Random(crates)
end

TickAI = function(bots, i)
	CheckTimers["Health"][i] = CheckTimers["Health"][i] - 1
	CheckTimers["Demolish"][i] = CheckTimers["Demolish"][i] - 1

	for _,bot in pairs(bots) do
		local unit = bot.Unit
		if unit ~= nil and not unit.IsDead then
			if unit.Type == "mhq" then
				if (not Lobby.ExploredMap() or Lobby.FogOfWar()) and MHQActor[bot.InternalName] ~= unit then
					MHQSpyPlane(unit)
				end
			end

			if unit.IsIdle or unit.IsIdleAircraft then
				local crates = GetNearbyCrates(unit, 8)
				for _,crate in pairs(crates) do
					unit.Move(crate.Location)
					if unit.HasProperty("Land") then
						unit.Land(crate)
					end
				end

				if unit.Flag ~= nil then
					unit.Move(FlagCircles[bot.TeamLeader.InternalName].Location)
				elseif unit.Type == "e6" then
					Trigger.AfterDelay(5, function()
						if unit.IsIdle then
							BuildEngiStuff(unit)
						end
					end)
				elseif unit.Type == "tran" then
					unit.Move(unit.Location) -- Take off.
				else
					IdleHunt(unit)
				end
			end

			if unit.Health <= unit.MaxHealth * 25 / 100 then
				local healcrate = GetNearbyHealCrate(unit, 10)

				if healcrate ~= nil then
					if not unit.HasProperty("Land") or CheckTimers["Health"][i] <= 0 then
						unit.Stop()
						unit.Move(healcrate.Location)
						if unit.HasProperty("Land") then
							unit.Land(healcrate)
						end
					end
				end
			elseif unit.Type == "rmbo" and CheckTimers["Demolish"][i] <= 0 then
				local enemies = GetNearbyEnemies(unit, 2)

				if #enemies > 0 then
					unit.Stop()
					Utils.Do(enemies, function(enemy)
						unit.Demolish(enemy)
					end)
				end
			end
		end
	end

	if CheckTimers["Health"][i] <= 0 then
		CheckTimers["Health"][i] = 25
	end

	if CheckTimers["Demolish"][i] <= 0 then
		CheckTimers["Demolish"][i] = 25
	end

	Trigger.AfterDelay(5, function()
		TickAI(bots, i)
	end)
end

AITick = function()

end

DivideBots = function(bots)
	for i = 1, #bots, 1 do
		Bots[i % 5 + 1][#Bots[i % 5 + 1] + 1] = bots[i]
	end
end

AIWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)
	local bots = Utils.Where(players, function(p) return p.IsBot end)

	DivideBots(bots)
	for i = 1, #Bots, 1 do
		Trigger.AfterDelay(i, function()
			TickAI(Bots[i], i)
		end)
	end

	for _,bot in pairs(bots) do
		MHQActor[bot.InternalName] = nil
	end
end
