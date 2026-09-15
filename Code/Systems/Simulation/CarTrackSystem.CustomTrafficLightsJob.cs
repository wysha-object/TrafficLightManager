using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Net;
using Game.Vehicles;
using TrafficLightManager.Code.Components;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TrafficLightManager.Code.Systems.Simulation
{
    public partial class CarTrackSystem
    {
        private JobHandle ScheduleCustomTrafficLightsJob(in JobHandle dependsOn, EntityCommandBuffer entityCommandBuffer)
        {
            JobHandle dependency = dependsOn;
            dependency = new CustomTrafficLightsJob
            {
                m_EntityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                m_EntityType = GetEntityTypeHandle(),
                m_SubLaneType = GetBufferTypeHandle<SubLane>(true),
                m_CarCurrentLaneLookup = GetComponentLookup<CarCurrentLane>(true),
                m_LaneObjectLookup = GetBufferLookup<LaneObject>(true),
            }.ScheduleParallel(m_CustomTrafficLightsQuery, dependency);
            return dependency;
        }

        private struct CustomTrafficLightsJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            [ReadOnly]
            public BufferTypeHandle<SubLane> m_SubLaneType;

            [ReadOnly]
            public ComponentLookup<CarCurrentLane> m_CarCurrentLaneLookup;

            [ReadOnly]
            public BufferLookup<LaneObject> m_LaneObjectLookup;

            private void Tick(int unfilteredChunkIndex, Entity entity, DynamicBuffer<SubLane> subLaneBuffer)
            {
                foreach (var subLane in subLaneBuffer)
                {
                    if (m_LaneObjectLookup.TryGetBuffer(subLane.m_SubLane, out var laneObjectBuffer))
                    {
                        foreach (var laneObject in laneObjectBuffer)
                        {
                            var carEntity = laneObject.m_LaneObject;
                            if (m_CarCurrentLaneLookup.HasComponent(carEntity))
                            {
                                m_EntityCommandBuffer.AddComponent(
                                    unfilteredChunkIndex,
                                    carEntity,
                                    new TrackedCar { m_CustomTrafficLights = entity, m_SubLane = subLane.m_SubLane }
                                );
                            }
                        }
                    }
                }
            }

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityArray = chunk.GetNativeArray(m_EntityType);
                BufferAccessor<SubLane> subLaneAccessor = chunk.GetBufferAccessor(ref m_SubLaneType);
                for (int i = 0; i < entityArray.Length; i++)
                {
                    Tick(unfilteredChunkIndex, entityArray[i], subLaneAccessor[i]);
                }
            }
        }
    }
}
