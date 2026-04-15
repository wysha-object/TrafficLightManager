using System;
using System.Runtime.CompilerServices;
using Game;
using Game.Common;
using Game.Net;
using Game.Tools;
using TrafficLightManager.Code.Components;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Internal;
using Unity.Jobs;
using UnityEngine.Scripting;

namespace TrafficLightManager.Code.Systems.TrafficLightSystems.Simulation;

/**
 * 同一联动组的每个路口实体更新后,
 * 在TrafficLightGroupSystem里同步
 */
public partial class TrafficLightGroupSystem : GameSystemBase
{
    //[BurstCompile]
    public partial struct UpdateTrafficLightGroupJob : IJobChunk
    {
        public EntityStorageInfoLookup m_EntityStorageInfoLookup;

        public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

        public EntityTypeHandle m_EntityType;

        public ComponentTypeHandle<TrafficLightGroup> m_TrafficLightGroupType;

        [ReadOnly]
        public BufferTypeHandle<TrafficLightsMemberRef> m_TrafficLightsMemberRefType;

        [ReadOnly]
        public BufferTypeHandle<CustomPhaseData> m_CustomPhaseDataBufferType;

        public ComponentLookup<TrafficLights> m_TrafficLightsLookup;

        public ComponentLookup<CustomTrafficLights> m_CustomTrafficLightsLookup;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> entityArray = chunk.GetNativeArray(m_EntityType);
            NativeArray<TrafficLightGroup> trafficLightGroupArray = chunk.GetNativeArray(ref m_TrafficLightGroupType);
            BufferAccessor<TrafficLightsMemberRef> trafficLightsMemberRefAccessor = chunk.GetBufferAccessor(ref m_TrafficLightsMemberRefType);
            BufferAccessor<CustomPhaseData> customPhaseDataBufferAccessor = chunk.GetBufferAccessor(ref m_CustomPhaseDataBufferType);

            for (int i = 0; i < trafficLightGroupArray.Length; i++)
            {
                Entity entity = entityArray[i];
                TrafficLightGroup trafficLightGroup = trafficLightGroupArray[i];
                DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefBuffer = trafficLightsMemberRefAccessor[i];
                DynamicBuffer<CustomPhaseData> customPhaseDataBuffer = customPhaseDataBufferAccessor[i];

                NativeList<Entity> memberEntities = new NativeList<Entity>(trafficLightsMemberRefBuffer.Length, Allocator.Temp);
                for (int j = trafficLightsMemberRefBuffer.Length - 1; j >= 0; j--)
                {
                    if (
                        !m_EntityStorageInfoLookup.Exists(trafficLightsMemberRefBuffer[j].m_Entity)
                        // 路口可能被其它方式删除
                        || !m_TrafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity)
                        || !m_CustomTrafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity)
                    )
                    {
                        if (
                            !m_TrafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity)
                            && m_CustomTrafficLightsLookup.HasComponent(trafficLightsMemberRefBuffer[j].m_Entity)
                        )
                        {
                            m_EntityCommandBuffer.RemoveComponent<CustomTrafficLights>(unfilteredChunkIndex, trafficLightsMemberRefBuffer[j].m_Entity);
                        }
                        trafficLightsMemberRefBuffer.RemoveAtSwapBack(j);
                        continue;
                    }

                    memberEntities.Add(trafficLightsMemberRefBuffer[j].m_Entity);
                }
                if (trafficLightsMemberRefBuffer.Length == 0)
                {
                    m_EntityCommandBuffer.DestroyEntity(unfilteredChunkIndex, entity);
                    memberEntities.Dispose();
                    continue;
                }

                Entity target = Entity.Null;
                foreach (var item in memberEntities)
                {
                    TrafficLights trafficLights = m_TrafficLightsLookup[item];
                    CustomTrafficLights customTrafficLights = m_CustomTrafficLightsLookup[item];
                    customTrafficLights.m_ManualSignalGroup = trafficLightGroup.m_ManualSignalGroup;
                    m_CustomTrafficLightsLookup[item] = customTrafficLights;

                    if (
                        trafficLights.m_State != trafficLightGroup.m_State
                        || trafficLights.m_CurrentSignalGroup != trafficLightGroup.m_CurrentSignalGroup
                        || trafficLights.m_NextSignalGroup != trafficLightGroup.m_NextSignalGroup
                        || customTrafficLights.m_Timer != trafficLightGroup.m_Timer
                    )
                    {
                        if (target == Entity.Null)
                        {
                            target = item;
                        }
                        else
                        {
                            TrafficLights targetTrafficLights = m_TrafficLightsLookup[target];
                            CustomTrafficLights targetCustomTrafficLights = m_CustomTrafficLightsLookup[target];
                            if (
                                isAheadSignalGroup(trafficLights.m_CurrentSignalGroup, targetTrafficLights.m_CurrentSignalGroup, customPhaseDataBuffer.Length)
                                || customTrafficLights.m_Timer > targetCustomTrafficLights.m_Timer
                            )
                            {
                                target = item;
                            }
                        }
                    }
                }

                if (target != Entity.Null)
                {
                    TrafficLights targetTrafficLights = m_TrafficLightsLookup[target];
                    CustomTrafficLights targetCustomTrafficLights = m_CustomTrafficLightsLookup[target];
                    trafficLightGroup.m_State = targetTrafficLights.m_State;
                    trafficLightGroup.m_CurrentSignalGroup = targetTrafficLights.m_CurrentSignalGroup;
                    trafficLightGroup.m_NextSignalGroup = targetTrafficLights.m_NextSignalGroup;
                    trafficLightGroup.m_Timer = targetCustomTrafficLights.m_Timer;

                    foreach (var item in memberEntities)
                    {
                        TrafficLights trafficLights = m_TrafficLightsLookup[item];
                        CustomTrafficLights customTrafficLights = m_CustomTrafficLightsLookup[item];

                        trafficLights.m_State = trafficLightGroup.m_State;
                        trafficLights.m_CurrentSignalGroup = trafficLightGroup.m_CurrentSignalGroup;
                        trafficLights.m_NextSignalGroup = trafficLightGroup.m_NextSignalGroup;
                        customTrafficLights.m_Timer = trafficLightGroup.m_Timer;

                        m_TrafficLightsLookup[item] = trafficLights;
                        m_CustomTrafficLightsLookup[item] = customTrafficLights;
                    }
                }

                trafficLightGroupArray[i] = trafficLightGroup;
                memberEntities.Dispose();
            }
        }

        public bool isAheadSignalGroup(int signalGroup1, int signalGroup2, int signalGroupCount)
        {
            int linearDiff = signalGroup1 - signalGroup2;
            int circularDiff = signalGroup1 + signalGroupCount - signalGroup2;
            if (Math.Abs(linearDiff) < Math.Abs(circularDiff))
            {
                return linearDiff > 0;
            }
            else
            {
                return circularDiff > 0;
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
        }
    }

    private struct TypeHandle
    {
        public EntityTypeHandle __Entity_TypeHandle;

        public ComponentTypeHandle<TrafficLightGroup> __TrafficLightGroup_RW_ComponentTypeHandle;

        public BufferTypeHandle<TrafficLightsMemberRef> __TrafficLightsMemberRef_RW_BufferTypeHandle;

        public BufferTypeHandle<CustomPhaseData> __CustomPhaseData_RO_BufferTypeHandle;

        public ComponentLookup<TrafficLights> __TrafficLights_RW_ComponentLookup;

        public ComponentLookup<CustomTrafficLights> __CustomTrafficLights_RW_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Entity_TypeHandle = state.GetEntityTypeHandle();
            __TrafficLightGroup_RW_ComponentTypeHandle = state.GetComponentTypeHandle<TrafficLightGroup>(isReadOnly: false);
            __TrafficLightsMemberRef_RW_BufferTypeHandle = state.GetBufferTypeHandle<TrafficLightsMemberRef>(isReadOnly: false);
            __CustomPhaseData_RO_BufferTypeHandle = state.GetBufferTypeHandle<CustomPhaseData>(isReadOnly: true);
            __TrafficLights_RW_ComponentLookup = state.GetComponentLookup<TrafficLights>(isReadOnly: false);
            __CustomTrafficLights_RW_ComponentLookup = state.GetComponentLookup<CustomTrafficLights>(isReadOnly: false);
        }
    }

    private EntityQuery m_TrafficLightGroupQuery;

    private TypeHandle __TypeHandle;

    private EndFrameBarrier m_EndFrameBarrier;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();

        m_EndFrameBarrier = base.World.GetOrCreateSystemManaged<EndFrameBarrier>();
        m_TrafficLightGroupQuery = GetEntityQuery(
            ComponentType.ReadWrite<TrafficLightGroup>(),
            ComponentType.Exclude<Deleted>(),
            ComponentType.Exclude<Destroyed>(),
            ComponentType.Exclude<Temp>()
        );
        RequireForUpdate(m_TrafficLightGroupQuery);
    }

    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        __TypeHandle.__AssignHandles(ref base.CheckedStateRef);
    }

    [Preserve]
    protected override void OnUpdate()
    {
        JobHandle dependency = JobChunkExtensions.Schedule(
            //JobChunkExtensions.ScheduleParallel(
            new UpdateTrafficLightGroupJob
            {
                m_EntityStorageInfoLookup = GetEntityStorageInfoLookup(),
                m_EntityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
                m_EntityType = InternalCompilerInterface.GetEntityTypeHandle(ref __TypeHandle.__Entity_TypeHandle, ref base.CheckedStateRef),
                m_TrafficLightGroupType = InternalCompilerInterface.GetComponentTypeHandle(ref __TypeHandle.__TrafficLightGroup_RW_ComponentTypeHandle, ref base.CheckedStateRef),
                m_TrafficLightsMemberRefType = InternalCompilerInterface.GetBufferTypeHandle(
                    ref __TypeHandle.__TrafficLightsMemberRef_RW_BufferTypeHandle,
                    ref base.CheckedStateRef
                ),
                m_CustomPhaseDataBufferType = InternalCompilerInterface.GetBufferTypeHandle(ref __TypeHandle.__CustomPhaseData_RO_BufferTypeHandle, ref base.CheckedStateRef),
                m_TrafficLightsLookup = InternalCompilerInterface.GetComponentLookup(ref __TypeHandle.__TrafficLights_RW_ComponentLookup, ref base.CheckedStateRef),
                m_CustomTrafficLightsLookup = InternalCompilerInterface.GetComponentLookup(ref __TypeHandle.__CustomTrafficLights_RW_ComponentLookup, ref base.CheckedStateRef),
            },
            m_TrafficLightGroupQuery,
            base.Dependency
        );
        base.Dependency = dependency;
        m_EndFrameBarrier.AddJobHandleForProducer(base.Dependency);
    }
}
