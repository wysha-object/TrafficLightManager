using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Net;
using Game.Vehicles;
using TrafficLightManager.Code.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TrafficLightManager.Code.Systems.Simulation
{
    public partial class CarTrackSystem
    {
        private JobHandle ScheduleTrackedCarJob(in JobHandle dependsOn, EntityCommandBuffer entityCommandBuffer)
        {
            JobHandle dependency = dependsOn;
            var needIncrementList = new NativeList<Entity>(m_TrackedCarQuery.CalculateEntityCount(), Allocator.TempJob);
            dependency = new TrackedCarJob
            {
                m_EntityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                m_EntityType = GetEntityTypeHandle(),
                m_TrackedCarType = GetComponentTypeHandle<TrackedCar>(true),
                m_CarCurrentLaneType = GetComponentTypeHandle<CarCurrentLane>(true),
                m_NeedIncrementList = needIncrementList.AsParallelWriter(),
            }.ScheduleParallel(m_TrackedCarQuery, dependency);
            dependency = new IncrementJob { m_NeedIncrementList = needIncrementList, m_CustomTrafficLightsLookup = GetComponentLookup<CustomTrafficLights>() }.Schedule(dependency);
            needIncrementList.Dispose(dependency);
            return dependency;
        }

        private struct TrackedCarJob : IJobChunk
        {
            public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

            [ReadOnly]
            public EntityTypeHandle m_EntityType;

            [ReadOnly]
            public ComponentTypeHandle<TrackedCar> m_TrackedCarType;

            [ReadOnly]
            public ComponentTypeHandle<CarCurrentLane> m_CarCurrentLaneType;

            public NativeList<Entity>.ParallelWriter m_NeedIncrementList;

            private void Tick(int unfilteredChunkIndex, Entity entity, TrackedCar trackedCar, CarCurrentLane carCurrentLane)
            {
                if (carCurrentLane.m_Lane == trackedCar.m_SubLane)
                {
                    return;
                }

                m_NeedIncrementList.AddNoResize(trackedCar.m_CustomTrafficLights);
                m_EntityCommandBuffer.RemoveComponent<TrackedCar>(unfilteredChunkIndex, entity);
            }

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in Unity.Burst.Intrinsics.v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<TrackedCar> trackedCarArray = chunk.GetNativeArray(ref m_TrackedCarType);
                NativeArray<CarCurrentLane> carCurrentLaneArray = chunk.GetNativeArray(ref m_CarCurrentLaneType);
                for (int i = 0; i < entityArray.Length; i++)
                {
                    Tick(unfilteredChunkIndex, entityArray[i], trackedCarArray[i], carCurrentLaneArray[i]);
                }
            }
        }

        private struct IncrementJob : IJob
        {
            public NativeList<Entity> m_NeedIncrementList;
            public ComponentLookup<CustomTrafficLights> m_CustomTrafficLightsLookup;

            public void Execute()
            {
                for (int i = 0; i < m_NeedIncrementList.Length; i++)
                {
                    Entity entity = m_NeedIncrementList[i];
                    if (m_CustomTrafficLightsLookup.TryGetComponent(entity, out var customTrafficLights))
                    {
                        customTrafficLights.m_NextPassedCarCount++;
                        m_CustomTrafficLightsLookup[entity] = customTrafficLights;
                    }
                }
            }
        }
    }
}
