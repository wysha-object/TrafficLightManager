using Game;
using Game.Common;
using Game.Net;
using Game.Simulation;
using Game.Tools;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Update;
using TrafficLightManager.Code.Utils;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Scripting;

namespace TrafficLightManager.Code.Systems.Simulation
{
    public partial class TrafficLightGroupSystem : GameSystemBase
    {
        private EndFrameBarrier m_EndFrameBarrier;

        private SimulationSystem m_SimulationSystem;

        private TimeSystem m_TimeSystem;

        private EntityQuery m_TrafficLightGroupQuery;

        public override int GetUpdateInterval(SystemUpdatePhase phase)
        {
            return 4 * 16;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            m_EndFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
            m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>();
            m_TimeSystem = base.World.GetOrCreateSystemManaged<TimeSystem>();

            m_TrafficLightGroupQuery = GetEntityQuery(
                ComponentType.ReadWrite<TrafficLightGroup>(),
                ComponentType.Exclude<Deleted>(),
                ComponentType.Exclude<Destroyed>(),
                ComponentType.Exclude<Temp>()
            );
            RequireForUpdate(m_TrafficLightGroupQuery);
        }

        protected override void OnUpdate()
        {
            Dependency = ScheduleTrafficLightGroupValidationJob(Dependency, m_EndFrameBarrier.CreateCommandBuffer());
            Dependency = ScheduleUpdateTrafficLightGroupJob(Dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(Dependency);
        }
    }
}
