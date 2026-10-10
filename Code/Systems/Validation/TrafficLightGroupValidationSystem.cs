using Game;
using Game.Common;
using Game.Net;
using Game.Simulation;
using Game.Tools;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Utils;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace TrafficLightManager.Code.Systems.Validation
{
    public partial class TrafficLightGroupValidationSystem : GameSystemBase
    {
        private ModificationEndBarrier m_ModificationEndBarrier;

        private EntityQuery m_TrafficLightGroupQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_ModificationEndBarrier = base.World.GetOrCreateSystemManaged<ModificationEndBarrier>();
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
            Dependency = ScheduleTrafficLightGroupValidationJob(Dependency, m_ModificationEndBarrier.CreateCommandBuffer());
            m_ModificationEndBarrier.AddJobHandleForProducer(Dependency);
        }
    }
}
