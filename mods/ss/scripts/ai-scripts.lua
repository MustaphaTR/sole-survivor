--[[
   Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

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

	local buildableLocations = Utils.Where(locations, function(l) return (Map.TerrainType(l) == "Clear" or Map.TerrainType(l) == "Road") and BuildingInfluence.BuildingAtCell(l) == nil and #Map.ActorsInBox(Map.CenterOfCell(l) + WVec.New(-512, -512, 0), Map.CenterOfCell(l) + WVec.New(512, 512, 0)) == 0 end)

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

GetNearbyCrates = function(actor, distance)
	return Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type == "crate" or a.Type == "upgradecrate" or a.Type == "experiencecrate" or a.Type == "wackycrate" end)
end

GetNearbyHealCrate = function(actor, distance)
	local crates = Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type == "healcrate" end)

	if #crates == 0 then
		return nil
	end

	return Utils.Random(crates)
end

TickAI = function(bot)
	if bot.IsObjectiveFailed(0) then
		return
	end

	local unit = bot.Unit
	if unit ~= nil and not unit.IsDead then
		if unit.IsIdle then
			local crates = GetNearbyCrates(unit, 8)
			for _,crate in pairs(crates) do
				unit.Move(crate.Location)
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
				-- Do nothing!
			else
				IdleHunt(unit)
			end
		end

		if unit.Health <= unit.MaxHealth * 25 / 100 then
			local healcrate = GetNearbyHealCrate(unit, 10)

			if healcrate ~= nil then
				unit.Stop()
				unit.Move(healcrate.Location)
			end
		end
	end

	Trigger.AfterDelay(5, function()
		TickAI(bot)
	end)
end

AITick = function()

end

AIWorldLoaded = function()
	players = Player.GetPlayers(function(p) return not p.IsNonCombatant end)
	bots = Utils.Where(players, function(p) return p.IsBot end)


	for i = 1, #bots, 1 do
		Trigger.AfterDelay(i % 5, function()
			TickAI(bots[i])
		end)
	end
end
