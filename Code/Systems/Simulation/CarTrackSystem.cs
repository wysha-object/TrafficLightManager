using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Common;
using Game.Net;
using Game.Tools;
using Game.Vehicles;
using TrafficLightManager.Code.Components;
using Unity.Entities;

namespace TrafficLightManager.Code.Systems.Simulation
{
    public partial class CarTrackSystem : GameSystemBase
    {
        private EndFrameBarrier m_EndFrameBarrier;
        private EntityQuery m_CustomTrafficLightsQuery;
        private EntityQuery m_TrackedCarQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EndFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_CustomTrafficLightsQuery = GetEntityQuery(
                new EntityQueryDesc { All = [ComponentType.ReadOnly<CustomTrafficLights>()], None = [ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Temp>()] }
            );
            m_TrackedCarQuery = GetEntityQuery(
                new EntityQueryDesc
                {
                    All = [ComponentType.ReadOnly<TrackedCar>(), ComponentType.ReadOnly<CarCurrentLane>()],
                    None = [ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Temp>()],
                }
            );
        }

        protected override void OnUpdate()
        {
            Dependency = ScheduleCustomTrafficLightsJob(Dependency, m_EndFrameBarrier.CreateCommandBuffer());
            Dependency = ScheduleTrackedCarJob(Dependency, m_EndFrameBarrier.CreateCommandBuffer());
        }
    }
}
