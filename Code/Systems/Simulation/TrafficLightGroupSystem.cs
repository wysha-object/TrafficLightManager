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

        private TrafficLightGroupValidationSystem m_TrafficLightGroupValidationSystem;

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
            m_TrafficLightGroupValidationSystem = base.World.GetOrCreateSystemManaged<TrafficLightGroupValidationSystem>();

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
            var templateList = Mod.m_Settings?.GetCustomPhaseTemplates();
            int templateCount = templateList?.Count ?? 0;
            NativeArray<CustomPhaseTemplate.Values> customPhaseTemplates = new NativeArray<CustomPhaseTemplate.Values>(templateCount, Allocator.TempJob);
            for (int i = 0; i < templateCount; i++)
            {
                customPhaseTemplates[i] = new CustomPhaseTemplate.Values(templateList[i]);
            }

            Dependency = m_TrafficLightGroupValidationSystem.ScheduleTrafficLightGroupValidationJob(Dependency, m_EndFrameBarrier.CreateCommandBuffer());
            Dependency = JobChunkExtensions.ScheduleParallel(
                new UpdateTrafficLightGroupJob
                {
                    m_FrameIndex = m_SimulationSystem.frameIndex,
                    m_Settings = new Settings.Values(Mod.m_Settings),
                    m_CustomPhaseTemplates = customPhaseTemplates,
                    m_EntityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                    m_EntityStorageInfoLookup = SystemAPI.GetEntityStorageInfoLookup(),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_TrafficLightGroupType = SystemAPI.GetComponentTypeHandle<TrafficLightGroup>(isReadOnly: false),
                    m_TrafficLightsMemberRefType = SystemAPI.GetBufferTypeHandle<TrafficLightsMemberRef>(isReadOnly: false),
                    m_CustomPhaseDataBufferType = SystemAPI.GetBufferTypeHandle<CustomPhaseData>(isReadOnly: false),
                    m_TrafficLightsLookup = SystemAPI.GetComponentLookup<TrafficLights>(isReadOnly: false),
                    m_CustomTrafficLightsLookup = SystemAPI.GetComponentLookup<CustomTrafficLights>(isReadOnly: false),
                    m_ExtraLaneSignalLookup = SystemAPI.GetComponentLookup<ExtraLaneSignal>(isReadOnly: true),
                    m_LaneFlowLookup = SystemAPI.GetComponentLookup<LaneFlow>(isReadOnly: true),
                    m_LaneFlowHistoryLookup = SystemAPI.GetComponentLookup<LaneFlowHistory>(isReadOnly: true),
                    m_MasterLaneLookup = SystemAPI.GetComponentLookup<MasterLane>(isReadOnly: true),
                    m_CarLaneLookup = SystemAPI.GetComponentLookup<CarLane>(isReadOnly: true),
                    m_TrackLaneLookup = SystemAPI.GetComponentLookup<TrackLane>(isReadOnly: true),
                    m_PedestrianLaneLookup = SystemAPI.GetComponentLookup<PedestrianLane>(isReadOnly: true),
                    m_LaneSignalLookup = SystemAPI.GetComponentLookup<LaneSignal>(isReadOnly: false),
                    m_SubLaneBufferLookup = SystemAPI.GetBufferLookup<SubLane>(isReadOnly: true),
                },
                m_TrafficLightGroupQuery,
                Dependency
            );

            Dependency = customPhaseTemplates.Dispose(Dependency);

            m_EndFrameBarrier.AddJobHandleForProducer(Dependency);
        }
    }
}
