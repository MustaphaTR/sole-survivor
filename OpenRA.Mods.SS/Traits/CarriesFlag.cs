#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.SS.Traits
{
    [Desc("This actor can carry a CtF flag.")]
    public class CarriesFlagInfo : ITraitInfo
    {
        [GrantedConditionReference]
        [Desc("Condition to grant when the actor is carrying a flag.")]
        public readonly string Condition = null;

        public object Create(ActorInitializer init) { return new CarriesFlag(init.Self, this); }
    }

    public class CarriesFlag : INotifyCreated, INotifyKilled, ITick, IRender
    {
        readonly CarriesFlagInfo info;
        readonly SpawnSSUnit spawner;
        readonly BuildingInfluence bi;

        public Actor Flag;
        IActorPreview[] flagPreviews;

        public int Token = ConditionManager.InvalidConditionToken;
        ConditionManager conditionManager;

        public CarriesFlag(Actor self, CarriesFlagInfo info)
        {
            this.info = info;
            Flag = null;

            spawner = self.World.WorldActor.Trait<SpawnSSUnit>();
            bi = self.World.WorldActor.Trait<BuildingInfluence>();
        }

        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.Trait<ConditionManager>();
        }

        void ITick.Tick(Actor self)
        {
            if (flagPreviews != null)
                foreach (var preview in flagPreviews)
                    preview.Tick();
        }

        public void GrantCondition(Actor self)
        {
            if (conditionManager == null || string.IsNullOrEmpty(info.Condition))
                return;

            Token = conditionManager.GrantCondition(self, info.Condition);
        }

        public void RevokeCondition(Actor self)
        {
            if (conditionManager == null)
                return;

            if (conditionManager.TokenValid(self, Token))
                Token = conditionManager.RevokeCondition(self, Token);
        }

        public void TakeFlag(Actor self, Actor flag)
        {
            if (Flag != null)
                DropFlag(self);

            Flag = flag;
        }

        public void DropFlag(Actor self)
        {
            self.World.Add(Flag);
            var positionable = Flag.Trait<IPositionable>();
            if (positionable.CanExistInCell(self.Location) && bi.GetBuildingAt(self.Location) == null)
                positionable.SetPosition(Flag, self.Location);
            else
            {
                var sp = spawner.PlayerSpawnPoints[spawner.TeamLeaders[Flag.Owner]];
                positionable.SetPosition(Flag, sp);
            }
            Flag.Trait<Flag>().Collected = false;

            Flag = null;
            flagPreviews = null;
            RevokeCondition(self);
        }

        IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
        {
            if (Flag != null && flagPreviews == null)
            {
                var flagInits = new TypeDictionary()
                {
                    new OwnerInit(Flag.Owner)
                };

                foreach (var api in Flag.TraitsImplementing<IActorPreviewInitModifier>())
                    api.ModifyActorPreviewInit(Flag, flagInits);

                var init = new ActorPreviewInitializer(Flag.Info, wr, flagInits);
                flagPreviews = Flag.Info.TraitInfos<IRenderActorPreviewInfo>()
                    .SelectMany(rpi => rpi.RenderPreview(init))
                    .ToArray();
            }

            var pos = self.CenterPosition;
            if (flagPreviews != null)
                foreach (var preview in flagPreviews)
                    foreach (var pp in preview.Render(wr, pos))
                        yield return pp.WithZOffset(1).AsDecoration();
        }

        IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
        {
            var pos = self.CenterPosition;
            if (flagPreviews != null)
                foreach (var preview in flagPreviews)
                    foreach (var b in preview.ScreenBounds(wr, pos))
                        yield return b;
        }

        void INotifyKilled.Killed(Actor self, AttackInfo e)
        {
            if (Flag != null)
                self.World.AddFrameEndTask(w => DropFlag(self));
        }
    }
}
