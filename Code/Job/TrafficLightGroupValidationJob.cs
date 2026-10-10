using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Net;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Utils;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TrafficLightManager.Code.Job
{
    [BurstCompile]
    public struct TrafficLightGroupValidationJob : IJobChunk
    {
        public EntityStorageInfoLookup m_EntityStorageInfoLookup;

        public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

        public EntityTypeHandle m_EntityType;

        public ComponentTypeHandle<TrafficLightGroup> m_TrafficLightGroupType;

        public BufferTypeHandle<TrafficLightsMemberRef> m_TrafficLightsMemberRefType;

        public BufferTypeHandle<CustomPhaseData> m_CustomPhaseDataBufferType;

        [ReadOnly]
        public ComponentLookup<TrafficLights> m_TrafficLightsLookup;

        [ReadOnly]
        public ComponentLookup<CustomTrafficLights> m_CustomTrafficLightsLookup;

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
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
    }
}
