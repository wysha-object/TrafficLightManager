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

namespace TrafficLightManager.Code.Systems.Update
{
    public partial class TrafficLightGroupValidationSystem : GameSystemBase
    {
        [BurstCompile]
        public partial struct TrafficLightGroupValidationJob : IJobChunk
        {
            public EntityStorageInfoLookup m_EntityStorageInfoLookup;

            public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

            public EntityTypeHandle m_EntityType;

            public ComponentTypeHandle<TrafficLightGroup> m_TrafficLightGroupType;

            public BufferTypeHandle<TrafficLightsMemberRef> m_TrafficLightsMemberRefType;

            public BufferTypeHandle<CustomPhaseData> m_CustomPhaseDataBufferType;

            public ComponentLookup<TrafficLights> m_TrafficLightsLookup;

            public ComponentLookup<CustomTrafficLights> m_CustomTrafficLightsLookup;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<TrafficLightGroup> trafficLightGroupArray = chunk.GetNativeArray(ref m_TrafficLightGroupType);
                BufferAccessor<TrafficLightsMemberRef> trafficLightsMemberRefAccessor = chunk.GetBufferAccessor(ref m_TrafficLightsMemberRefType);
                BufferAccessor<CustomPhaseData> customPhaseDataBufferAccessor = chunk.GetBufferAccessor(ref m_CustomPhaseDataBufferType);
                NativeList<Entity> memberEntities = new NativeList<Entity>(Allocator.TempJob);

                for (int i = 0; i < trafficLightGroupArray.Length; i++)
                {
                    Entity entity = entityArray[i];
                    TrafficLightGroup trafficLightGroup = trafficLightGroupArray[i];
                    DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefBuffer = trafficLightsMemberRefAccessor[i];
                    DynamicBuffer<CustomPhaseData> customPhaseDataBuffer = customPhaseDataBufferAccessor[i];

                    TrafficLightGroupUtils.ValidateTrafficLightGroup(
                        unfilteredChunkIndex,
                        entity,
                        m_EntityStorageInfoLookup,
                        m_TrafficLightsLookup,
                        m_CustomTrafficLightsLookup,
                        ref trafficLightsMemberRefBuffer,
                        ref m_EntityCommandBuffer,
                        ref memberEntities
                    );
                }
                memberEntities.Dispose();
            }

            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }
        }

        private ModificationEndBarrier m_ModificationEndBarrier;

        private EntityQuery m_TrafficLightGroupQuery;

        public JobHandle ScheduleTrafficLightGroupValidationJob(in JobHandle dependsOn, EntityCommandBuffer entityCommandBuffer)
        {
            JobHandle dependency = dependsOn;
            dependency = JobChunkExtensions.ScheduleParallel(
                new TrafficLightGroupValidationJob
                {
                    m_EntityStorageInfoLookup = SystemAPI.GetEntityStorageInfoLookup(),
                    m_EntityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                    m_EntityType = SystemAPI.GetEntityTypeHandle(),
                    m_TrafficLightGroupType = SystemAPI.GetComponentTypeHandle<TrafficLightGroup>(isReadOnly: false),
                    m_TrafficLightsMemberRefType = SystemAPI.GetBufferTypeHandle<TrafficLightsMemberRef>(isReadOnly: false),
                    m_CustomPhaseDataBufferType = SystemAPI.GetBufferTypeHandle<CustomPhaseData>(isReadOnly: false),
                    m_TrafficLightsLookup = SystemAPI.GetComponentLookup<TrafficLights>(isReadOnly: false),
                    m_CustomTrafficLightsLookup = SystemAPI.GetComponentLookup<CustomTrafficLights>(isReadOnly: false),
                },
                m_TrafficLightGroupQuery,
                dependency
            );
            return dependency;
        }

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
