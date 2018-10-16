--[[
   Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
   This file is part of OpenRA, which is free software. It is made
   available to you under the terms of the GNU General Public License
   as published by the Free Software Foundation, either version 3 of
   the License, or (at your option) any later version. For more
   information, see COPYING.
]]

AIUnits = { }

IdleHunt = function(actor)
	if actor.HasProperty("Hunt") and not actor.IsDead then
		Trigger.OnIdle(actor, actor.Hunt)
	end
end

GetNearbyCrates = function(actor, distance)
	return Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type == "crate" or a.Type == "upgradecrate" end)
end

GetNearbyHealCrate = function(actor, distance)
	local crates = Utils.Where(Map.ActorsInCircle(actor.CenterPosition, WDist.FromCells(distance)), function(a) return a.Type == "healcrate" end)

	if #crates == 0 then
		return nil
	end

	return Utils.Random(crates)
end

SetupAIUnits = function(bot)
	AIUnits[bot.InternalName] = bot.GetGroundAttackers()[1]
end

Tick = function()
	Trigger.AfterDelay(5, function()
		for _,bot in pairs(bots) do
			local unit = AIUnits[bot.InternalName]
			
			if unit ~= nil and not unit.IsDead then
				if unit.IsIdle then
					local crates = GetNearbyCrates(unit, 8)
					for _,crate in pairs(crates) do
						unit.Move(crate.Location)
					end

					IdleHunt(unit)
				end
				if unit.Health <= unit.MaxHealth * 25 / 100 then
					local healcrate = GetNearbyHealCrate(unit, 10)

					if healcrate ~= nil then
						unit.Stop()
						unit.Move(healcrate.Location)
					end
				end
			end
		end
	end)
end

WorldLoaded = function()
	bots = Player.GetPlayers(function(p) return p.IsBot and not p.IsNonCombatant end)

	for _,bot in pairs(bots) do
		SetupAIUnits(bot)
	end
end