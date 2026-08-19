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
using Unity.Mathematics;
using UnityEngine.Scripting;

namespace TrafficLightManager.Code.Systems.Simulation
{
    public partial class TrafficLightGroupSystem : GameSystemBase
    {
        [Preserve]
        protected override void OnUpdate()
        {
            var templateList = Mod.m_Settings?.GetCustomPhaseTemplates();
            int templateCount = templateList?.Count ?? 0;
            NativeArray<CustomPhaseTemplate.Values> customPhaseTemplates = new NativeArray<CustomPhaseTemplate.Values>(templateCount, Allocator.TempJob);
            for (int i = 0; i < templateCount; i++)
            {
                customPhaseTemplates[i] = new CustomPhaseTemplate.Values(templateList[i]);
            }

            JobHandle dependency = //JobChunkExtensions.Schedule(
            JobChunkExtensions.ScheduleParallel(
                new UpdateTrafficLightGroupJob
                {
                    m_EntityStorageInfoLookup = SystemAPI.GetEntityStorageInfoLookup(),
                    m_EntityCommandBuffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter(),
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
                    m_ExtraData = new ExtraData(this),
                    m_Settings = new Settings.Values(Mod.m_Settings),
                    m_CustomPhaseTemplates = customPhaseTemplates,
                },
                m_TrafficLightGroupQuery,
                base.Dependency
            );

            base.Dependency = customPhaseTemplates.Dispose(dependency);
            m_EndFrameBarrier.AddJobHandleForProducer(dependency);
        }

        [BurstCompile]
        public partial struct UpdateTrafficLightGroupJob : IJobChunk
        {
            public EntityStorageInfoLookup m_EntityStorageInfoLookup;

            public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

            public EntityTypeHandle m_EntityType;

            public ComponentTypeHandle<TrafficLightGroup> m_TrafficLightGroupType;

            public BufferTypeHandle<TrafficLightsMemberRef> m_TrafficLightsMemberRefType;

            public BufferTypeHandle<CustomPhaseData> m_CustomPhaseDataBufferType;

            public ComponentLookup<TrafficLights> m_TrafficLightsLookup;

            public ComponentLookup<CustomTrafficLights> m_CustomTrafficLightsLookup;

            [ReadOnly]
            public ComponentLookup<ExtraLaneSignal> m_ExtraLaneSignalLookup;

            [ReadOnly]
            public ComponentLookup<LaneFlow> m_LaneFlowLookup;

            [ReadOnly]
            public ComponentLookup<LaneFlowHistory> m_LaneFlowHistoryLookup;

            [ReadOnly]
            public ComponentLookup<MasterLane> m_MasterLaneLookup;

            [ReadOnly]
            public ComponentLookup<CarLane> m_CarLaneLookup;

            [ReadOnly]
            public ComponentLookup<TrackLane> m_TrackLaneLookup;

            [ReadOnly]
            public ComponentLookup<PedestrianLane> m_PedestrianLaneLookup;

            public ComponentLookup<LaneSignal> m_LaneSignalLookup;

            [ReadOnly]
            public BufferLookup<SubLane> m_SubLaneBufferLookup;

            public ExtraData m_ExtraData;

            public Settings.Values m_Settings;

            public NativeArray<CustomPhaseTemplate.Values> m_CustomPhaseTemplates;

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

                    if (
                        TrafficLightGroupUtils.ValidateTrafficLightGroup(
                            unfilteredChunkIndex,
                            entity,
                            m_EntityStorageInfoLookup,
                            m_TrafficLightsLookup,
                            m_CustomTrafficLightsLookup,
                            ref trafficLightsMemberRefBuffer,
                            ref m_EntityCommandBuffer,
                            ref memberEntities
                        )
                    )
                    {
                        // 更新status
                        var firstItem = memberEntities[0];
                        byte newStatus = m_CustomTrafficLightsLookup[firstItem].m_Status;
                        uint diff1 = (byte)(newStatus - trafficLightGroup.m_Status);
                        uint diff2 = (byte)(trafficLightGroup.m_Status - newStatus);
                        byte diff = (byte)math.min(diff1, diff2);

                        // 从template同步
                        for (int j = 0; j < customPhaseDataBuffer.Length; j++)
                        {
                            CustomPhaseData customPhaseData = customPhaseDataBuffer[j];
                            if ((customPhaseData.m_Options & CustomPhaseData.Options.BindWithTemplate) != 0)
                            {
                                int templateIndex = -1;
                                for (int k = 0; k < m_CustomPhaseTemplates.Length; k++)
                                {
                                    var template = m_CustomPhaseTemplates[k];
                                    if (template.m_Name == customPhaseData.m_BindTemplate)
                                    {
                                        templateIndex = k;
                                        break;
                                    }
                                }
                                if (templateIndex >= 0)
                                {
                                    var template = m_CustomPhaseTemplates[templateIndex];
                                    if (template.m_IsPrioritiseTrack)
                                        customPhaseData.m_Options |= CustomPhaseData.Options.PrioritiseTrack;
                                    else
                                        customPhaseData.m_Options &= ~CustomPhaseData.Options.PrioritiseTrack;
                                    if (template.m_IsPrioritisePublicCar)
                                        customPhaseData.m_Options |= CustomPhaseData.Options.PrioritisePublicCar;
                                    else
                                        customPhaseData.m_Options &= ~CustomPhaseData.Options.PrioritisePublicCar;
                                    if (template.m_IsPrioritisePedestrian)
                                        customPhaseData.m_Options |= CustomPhaseData.Options.PrioritisePedestrian;
                                    else
                                        customPhaseData.m_Options &= ~CustomPhaseData.Options.PrioritisePedestrian;
                                    customPhaseData.m_MinimumDuration = template.m_MinimumDuration;
                                    customPhaseData.m_MaximumDuration = template.m_MaximumDuration;
                                    customPhaseData.m_TargetDurationMultiplier = template.m_TargetDurationMultiplier;
                                    customPhaseData.m_LaneOccupiedMultiplier = template.m_LaneOccupiedMultiplier;
                                    customPhaseData.m_IntervalExponent = template.m_IntervalExponent;
                                    customPhaseDataBuffer[j] = customPhaseData;
                                }
                            }
                        }

                        // 更新相位数据
                        if (diff > 0)
                        {
                            NativeList<Entity> laneSignals = new NativeList<Entity>(30, Allocator.Temp);
                            NativeList<SubLane> subLanes = new NativeList<SubLane>(Allocator.Temp);
                            foreach (var item in memberEntities)
                            {
                                DynamicBuffer<SubLane> itemSubLaneBuffer = m_SubLaneBufferLookup[item];
                                subLanes.AddRange(itemSubLaneBuffer.AsNativeArray());
                            }
                            FillLaneSignals(subLanes, laneSignals);

                            CalculatePriority(subLanes, customPhaseDataBuffer);
                            CalculateFlow(unfilteredChunkIndex, subLanes, trafficLightGroup.m_CurrentSignalGroup, customPhaseDataBuffer);
                            UpdateTrafficLightState(ref trafficLightGroup, customPhaseDataBuffer);
                            UpdateLaneSignals(laneSignals, trafficLightGroup);
                            laneSignals.Dispose();
                            subLanes.Dispose();
                        }

                        // 同步
                        trafficLightGroup.m_Status = newStatus;
                        foreach (var item in memberEntities)
                        {
                            TrafficLights trafficLights = m_TrafficLightsLookup[item];
                            CustomTrafficLights customTrafficLights = m_CustomTrafficLightsLookup[item];

                            trafficLights.m_State = trafficLightGroup.m_State;
                            trafficLights.m_CurrentSignalGroup = trafficLightGroup.m_CurrentSignalGroup;
                            trafficLights.m_NextSignalGroup = trafficLightGroup.m_NextSignalGroup;
                            customTrafficLights.m_Timer = trafficLightGroup.m_Timer;
                            customTrafficLights.m_TargetDuration = trafficLightGroup.m_TargetDuration;
                            customTrafficLights.m_Status = trafficLightGroup.m_Status;

                            m_TrafficLightsLookup[item] = trafficLights;
                            m_CustomTrafficLightsLookup[item] = customTrafficLights;
                        }

                        // 结束
                        trafficLightGroupArray[i] = trafficLightGroup;
                    }
                }
                memberEntities.Dispose();
            }

            private void FillLaneSignals(NativeList<SubLane> subLanes, NativeList<Entity> laneSignals)
            {
                for (int i = 0; i < subLanes.Length; i++)
                {
                    Entity value = subLanes[i].m_SubLane;
                    if (m_LaneSignalLookup.HasComponent(value))
                    {
                        laneSignals.Add(in value);
                    }
                }
            }

            public static bool UpdateTrafficLightState(ref TrafficLightGroup trafficLightGroup, DynamicBuffer<CustomPhaseData> customPhaseDataBuffer)
            {
                if (
                    trafficLightGroup.m_State == Game.Net.TrafficLightState.None
                    || trafficLightGroup.m_State == Game.Net.TrafficLightState.Extending
                    || trafficLightGroup.m_State == Game.Net.TrafficLightState.Extended
                )
                {
                    trafficLightGroup.m_State = Game.Net.TrafficLightState.Beginning;
                    trafficLightGroup.m_CurrentSignalGroup = 0;
                    trafficLightGroup.m_NextSignalGroup = GetNextSignalGroup(trafficLightGroup.m_CurrentSignalGroup, customPhaseDataBuffer, trafficLightGroup, out _);
                    trafficLightGroup.m_Timer = 0;
                    return true;
                }
                else if (trafficLightGroup.m_State == Game.Net.TrafficLightState.Beginning)
                {
                    if (trafficLightGroup.m_NextSignalGroup <= 0)
                    {
                        trafficLightGroup.m_State = Game.Net.TrafficLightState.None; // roll a new group
                        return true;
                    }
                    trafficLightGroup.m_State = Game.Net.TrafficLightState.Ongoing;
                    trafficLightGroup.m_CurrentSignalGroup = trafficLightGroup.m_NextSignalGroup;
                    trafficLightGroup.m_NextSignalGroup = 0;
                    trafficLightGroup.m_Timer = 0;
                    for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                    {
                        CustomPhaseData phase = customPhaseDataBuffer[i];
                        if (trafficLightGroup.m_CurrentSignalGroup == i + 1)
                        {
                            phase.m_TurnsSinceLastRun = 0;
                            phase.m_LowFlowTimer = 0;
                            phase.m_LowPriorityTimer = 0;
                        }
                        else
                        {
                            phase.m_TurnsSinceLastRun++;
                        }
                        phase.m_Options &= ~CustomPhaseData.Options.EndPhasePrematurely;
                        customPhaseDataBuffer[i] = phase;
                    }
                    return true;
                }
                else if (trafficLightGroup.m_State == Game.Net.TrafficLightState.Ongoing)
                {
                    int currentSignalIndex = trafficLightGroup.m_CurrentSignalGroup - 1;
                    if (currentSignalIndex < 0 || currentSignalIndex >= customPhaseDataBuffer.Length)
                    {
                        trafficLightGroup.m_State = Game.Net.TrafficLightState.None; // roll a new group
                        return true;
                    }
                    trafficLightGroup.m_Timer++;
                    CustomPhaseData phase = customPhaseDataBuffer[currentSignalIndex];
                    float targetDuration = 10f * (phase.AverageCarFlow() + (float)(phase.m_TrackLaneOccupied)) * phase.m_TargetDurationMultiplier;
                    bool preferChange = false;
                    trafficLightGroup.m_TargetDuration = targetDuration;
                    if (trafficLightGroup.m_Timer <= phase.m_MinimumDuration)
                    {
                        phase.m_LowFlowTimer = 0;
                        phase.m_LowPriorityTimer = 0;
                    }
                    else if (phase.m_Priority > 0 && phase.m_Priority >= MaxPriority(customPhaseDataBuffer))
                    {
                        if (trafficLightGroup.m_Timer >= phase.m_MaximumDuration)
                        {
                            preferChange = true;
                        }
                        else if (trafficLightGroup.m_Timer <= targetDuration)
                        {
                            phase.m_LowFlowTimer = 0;
                        }
                        else if (phase.m_LowFlowTimer < 3)
                        {
                            phase.m_LowFlowTimer++;
                        }
                        else
                        {
                            preferChange = true;
                        }
                        phase.m_LowPriorityTimer = 0;
                    }
                    else if (phase.m_Priority < MaxPriority(customPhaseDataBuffer))
                    {
                        if (phase.m_LowPriorityTimer >= 1)
                        {
                            preferChange = true;
                        }
                        phase.m_LowPriorityTimer++;
                    }
                    else
                    {
                        preferChange = true;
                    }
                    if ((phase.m_Options & CustomPhaseData.Options.EndPhasePrematurely) != 0)
                    {
                        preferChange = true;
                    }
                    if (trafficLightGroup.m_ManualSignalGroup > 0 && trafficLightGroup.m_ManualSignalGroup != trafficLightGroup.m_CurrentSignalGroup)
                    {
                        preferChange = true;
                    }
                    customPhaseDataBuffer[currentSignalIndex] = phase;
                    byte nextGroup = GetNextSignalGroup(trafficLightGroup.m_CurrentSignalGroup, customPhaseDataBuffer, trafficLightGroup, out var linked);
                    if (preferChange && nextGroup != trafficLightGroup.m_CurrentSignalGroup)
                    {
                        trafficLightGroup.m_State = Game.Net.TrafficLightState.Ending;
                        trafficLightGroup.m_NextSignalGroup = nextGroup;
                        if (linked)
                        {
                            for (int i = trafficLightGroup.m_CurrentSignalGroup; i < trafficLightGroup.m_NextSignalGroup - 1; i++)
                            {
                                CustomPhaseData nextPhase = customPhaseDataBuffer[i];
                                if (nextPhase.m_Priority <= 0)
                                {
                                    nextPhase.m_TurnsSinceLastRun = 0;
                                    customPhaseDataBuffer[i] = nextPhase;
                                }
                            }
                        }
                        return true;
                    }
                    return false;
                }
                else if (trafficLightGroup.m_State == Game.Net.TrafficLightState.Ending)
                {
                    trafficLightGroup.m_State = Game.Net.TrafficLightState.Changing;
                    return true;
                }
                else if (trafficLightGroup.m_State == Game.Net.TrafficLightState.Changing)
                {
                    trafficLightGroup.m_State = Game.Net.TrafficLightState.Beginning;
                    return true;
                }
                return false;
            }

            public static byte GetNextSignalGroup(byte currentGroup, DynamicBuffer<CustomPhaseData> customPhaseDataBuffer, TrafficLightGroup trafficLightGroup, out bool linked)
            {
                linked = false;
                byte nextGroup = 0;
                int maxPriority = -1;
                float maxWaiting = -1;
                if (trafficLightGroup.m_ManualSignalGroup > 0 && trafficLightGroup.m_ManualSignalGroup - 1 < customPhaseDataBuffer.Length)
                {
                    return trafficLightGroup.m_ManualSignalGroup;
                }
                for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                {
                    CustomPhaseData phase = customPhaseDataBuffer[i];
                    float weightedWaiting =
                        ((float)phase.TotalLaneOccupied())
                        * phase.m_LaneOccupiedMultiplier
                        * math.pow((float)phase.m_TurnsSinceLastRun / (float)customPhaseDataBuffer.Length, phase.m_IntervalExponent);
                    if (phase.m_Priority > maxPriority)
                    {
                        nextGroup = (byte)(i + 1);
                        maxPriority = phase.m_Priority;
                        maxWaiting = weightedWaiting;
                    }
                    else if (phase.m_Priority == maxPriority && weightedWaiting > maxWaiting)
                    {
                        nextGroup = (byte)(i + 1);
                        maxWaiting = weightedWaiting;
                    }
                    phase.m_WeightedWaiting = weightedWaiting;
                    customPhaseDataBuffer[i] = phase;
                }

                int linkedPriority = -1;
                byte linkedNextGroup = 0;
                for (int i = currentGroup - 1; i >= 0 && i < customPhaseDataBuffer.Length - 1; i++)
                {
                    CustomPhaseData phase = customPhaseDataBuffer[i];
                    if ((phase.m_Options & CustomPhaseData.Options.LinkedWithNextPhase) == 0)
                    {
                        break;
                    }

                    CustomPhaseData nextPhase = customPhaseDataBuffer[i + 1];
                    if (linkedNextGroup == 0 && nextPhase.m_Priority > 0)
                    {
                        linkedNextGroup = (byte)(i + 2);
                    }
                    linkedPriority = math.max(linkedPriority, nextPhase.m_Priority);
                }
                if (linkedNextGroup > 0 && linkedPriority >= maxPriority)
                {
                    linked = true;
                    return linkedNextGroup;
                }

                for (int i = nextGroup - 2; i >= 0; i--)
                {
                    CustomPhaseData phase = customPhaseDataBuffer[i];
                    if ((phase.m_Options & CustomPhaseData.Options.LinkedWithNextPhase) == 0)
                    {
                        break;
                    }
                    if (phase.m_Priority > 0)
                    {
                        nextGroup = (byte)(i + 1);
                    }
                }
                return nextGroup;
            }

            private static int MaxPriority(DynamicBuffer<CustomPhaseData> customPhaseDataBuffer)
            {
                int max = int.MinValue;
                foreach (var phase in customPhaseDataBuffer)
                {
                    max = math.max(max, phase.m_Priority);
                }
                return max;
            }

            public void CalculatePriority(NativeList<SubLane> subLaneBuffer, DynamicBuffer<CustomPhaseData> customPhaseDataBuffer)
            {
                for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                {
                    CustomPhaseData customPhaseData = customPhaseDataBuffer[i];
                    customPhaseData.m_CarLaneOccupied = 0;
                    customPhaseData.m_PublicCarLaneOccupied = 0;
                    customPhaseData.m_TrackLaneOccupied = 0;
                    customPhaseData.m_PedestrianLaneOccupied = 0;
                    customPhaseData.m_Priority = 0;
                    customPhaseDataBuffer[i] = customPhaseData;
                }
                foreach (var subLane in subLaneBuffer)
                {
                    Entity subLaneEntity = subLane.m_SubLane;

                    if (!m_LaneSignalLookup.TryGetComponent(subLaneEntity, out var laneSignal))
                    {
                        continue;
                    }

                    Entity lanePetitioner = laneSignal.m_Petitioner;
                    int lanePriority = laneSignal.m_Priority;

                    laneSignal.m_Petitioner = Entity.Null;
                    laneSignal.m_Priority = laneSignal.m_Default;
                    m_LaneSignalLookup[subLaneEntity] = laneSignal;

                    if (m_MasterLaneLookup.HasComponent(subLaneEntity))
                    {
                        continue;
                    }
                    if (lanePetitioner == Entity.Null)
                    {
                        continue;
                    }

                    for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                    {
                        if ((laneSignal.m_GroupMask & (1 << i)) == 0)
                        {
                            continue;
                        }

                        CustomPhaseData customPhaseData = customPhaseDataBuffer[i];

                        if (m_CarLaneLookup.HasComponent(subLaneEntity))
                        {
                            customPhaseData.m_CarLaneOccupied++;
                            if (m_ExtraLaneSignalLookup.TryGetComponent(subLaneEntity, out var extraLaneSignal))
                            {
                                if (extraLaneSignal.m_SourceSubLane != Entity.Null && m_CarLaneLookup.TryGetComponent(extraLaneSignal.m_SourceSubLane, out var sourceCarLane))
                                {
                                    if ((sourceCarLane.m_Flags & CarLaneFlags.PublicOnly) != 0)
                                    {
                                        customPhaseData.m_PublicCarLaneOccupied++;
                                        if ((customPhaseData.m_Options & CustomPhaseData.Options.PrioritisePublicCar) != 0)
                                        {
                                            lanePriority = math.max(lanePriority, 104); // 104 is the priority for trams
                                        }
                                        else
                                        {
                                            lanePriority = math.min(lanePriority, 100); // 100 is the default priority
                                        }
                                    }
                                }
                            }
                        }
                        if (m_TrackLaneLookup.HasComponent(subLaneEntity))
                        {
                            customPhaseData.m_TrackLaneOccupied++;
                            if ((customPhaseData.m_Options & CustomPhaseData.Options.PrioritiseTrack) == 0)
                            {
                                // Do not lower priority for trains, as they do not stop for signals
                                // 110 is the priority for trains
                                if (lanePriority < 110)
                                {
                                    lanePriority = math.min(lanePriority, 100); // 100 is the default priority
                                }
                            }
                        }
                        if (m_PedestrianLaneLookup.TryGetComponent(subLaneEntity, out var pedestrianLane))
                        {
                            if ((pedestrianLane.m_Flags & PedestrianLaneFlags.Crosswalk) != 0)
                            {
                                customPhaseData.m_PedestrianLaneOccupied++;
                                if ((customPhaseData.m_Options & CustomPhaseData.Options.PrioritisePedestrian) != 0)
                                {
                                    lanePriority = math.max(lanePriority, 104); // 104 is the priority for trams
                                }
                            }
                        }

                        customPhaseData.m_Priority = math.max(customPhaseData.m_Priority, lanePriority);

                        customPhaseDataBuffer[i] = customPhaseData;
                    }
                }
            }

            public void CalculateFlow(int unfilteredChunkIndex, NativeList<SubLane> subLaneBuffer, byte currentSignalGroup, DynamicBuffer<CustomPhaseData> customPhaseDataBuffer)
            {
                float4 timeFactors = m_ExtraData.m_TimeFactors * 0.125f;
                for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                {
                    CustomPhaseData customPhaseData = customPhaseDataBuffer[i];
                    customPhaseData.m_CarFlow.z = customPhaseData.m_CarFlow.y;
                    customPhaseData.m_CarFlow.y = customPhaseData.m_CarFlow.x;
                    customPhaseData.m_CarFlow.x = 0f;
                    customPhaseDataBuffer[i] = customPhaseData;
                }
                foreach (var subLane in subLaneBuffer)
                {
                    Entity subLaneEntity = subLane.m_SubLane;
                    float4 newDistance = 0f;
                    float4 newDuration = 0f;
                    float4 oldDistance = 0f;
                    float4 oldDuration = 0f;
                    float4 diffDistance = 0f;
                    float4 diffDuration = 0f;
                    uint newFrame = m_ExtraData.m_Frame;
                    uint oldFrame = 0;
                    uint diffFrame = 0;

                    if (!m_LaneSignalLookup.TryGetComponent(subLaneEntity, out var laneSignal))
                    {
                        continue;
                    }
                    if (!m_LaneFlowLookup.TryGetComponent(subLaneEntity, out var laneFlow))
                    {
                        continue;
                    }
                    if ((laneSignal.m_GroupMask & (1 << currentSignalGroup - 1)) == 0)
                    {
                        continue;
                    }

                    newDistance = math.lerp(laneFlow.m_Distance, laneFlow.m_Next.y, timeFactors);
                    newDuration = math.lerp(laneFlow.m_Duration, laneFlow.m_Next.x, timeFactors);

                    LaneFlowHistory laneFlowHistory = new LaneFlowHistory();
                    if (m_LaneFlowHistoryLookup.TryGetComponent(subLaneEntity, out laneFlowHistory))
                    {
                        oldDistance = laneFlowHistory.m_Distance;
                        oldDuration = laneFlowHistory.m_Duration;
                        oldFrame = laneFlowHistory.m_Frame;
                    }
                    else
                    {
                        m_EntityCommandBuffer.AddComponent(unfilteredChunkIndex, subLaneEntity, laneFlowHistory);
                    }

                    diffDistance = newDistance - oldDistance;
                    diffDuration = newDuration - oldDuration;
                    diffFrame = newFrame - oldFrame;

                    laneFlowHistory.m_Distance = newDistance;
                    laneFlowHistory.m_Duration = newDuration;
                    laneFlowHistory.m_Frame = newFrame;

                    m_EntityCommandBuffer.SetComponent(unfilteredChunkIndex, subLaneEntity, laneFlowHistory);

                    int group = currentSignalGroup - 1;
                    if (group < customPhaseDataBuffer.Length && diffFrame > 0)
                    {
                        CustomPhaseData customPhaseData = customPhaseDataBuffer[group];
                        float totalDiff = math.abs(Max(diffDistance)) + math.abs(Max(diffDuration));
                        customPhaseData.m_CarFlow.x += totalDiff * (64f / (float)diffFrame); // 64 frames per traffic light tick
                        customPhaseDataBuffer[group] = customPhaseData;
                    }
                }
            }

            private static float Max(float4 f)
            {
                return math.max(f.w, math.max(f.x, math.max(f.y, f.z)));
            }

            private void UpdateLaneSignals(NativeList<Entity> laneSignals, TrafficLightGroup trafficLightGroup)
            {
                for (int i = 0; i < laneSignals.Length; i++)
                {
                    Entity entity = laneSignals[i];
                    LaneSignal laneSignal = m_LaneSignalLookup[entity];
                    ExtraLaneSignal extraLaneSignal = new ExtraLaneSignal();
                    if (m_ExtraLaneSignalLookup.HasComponent(entity))
                    {
                        extraLaneSignal = m_ExtraLaneSignalLookup[entity];
                    }
                    UpdateLaneSignal(trafficLightGroup, ref laneSignal, ref extraLaneSignal);
                    laneSignal.m_Petitioner = Entity.Null;
                    laneSignal.m_Priority = laneSignal.m_Default;
                    m_LaneSignalLookup[entity] = laneSignal;
                }
            }

            public static void UpdateLaneSignal(TrafficLightGroup trafficLightGroup, ref LaneSignal laneSignal, ref ExtraLaneSignal extraLaneSignal)
            {
                int num = 0;
                int num2 = 0;
                if (trafficLightGroup.m_CurrentSignalGroup > 0)
                {
                    num |= 1 << trafficLightGroup.m_CurrentSignalGroup - 1;
                }

                if (trafficLightGroup.m_NextSignalGroup > 0)
                {
                    num2 |= 1 << trafficLightGroup.m_NextSignalGroup - 1;
                }

                LaneSignalType goSignalType = LaneSignalType.Go;

                if ((extraLaneSignal.m_YieldGroupMask & (1 << trafficLightGroup.m_CurrentSignalGroup - 1)) != 0)
                {
                    goSignalType = LaneSignalType.Yield;
                }

                switch (trafficLightGroup.m_State)
                {
                    case Game.Net.TrafficLightState.Beginning:
                        if ((laneSignal.m_GroupMask & num2) != 0)
                        {
                            if (laneSignal.m_Signal != goSignalType)
                            {
                                laneSignal.m_Signal = LaneSignalType.Yield;
                            }
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    case Game.Net.TrafficLightState.Ongoing:
                        if ((laneSignal.m_GroupMask & num) != 0)
                        {
                            laneSignal.m_Signal = goSignalType;
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    case Game.Net.TrafficLightState.Extending:
                        if ((laneSignal.m_Flags & LaneSignalFlags.CanExtend) != 0)
                        {
                            if ((laneSignal.m_GroupMask & num) != 0)
                            {
                                laneSignal.m_Signal = goSignalType;
                            }
                            else
                            {
                                laneSignal.m_Signal = LaneSignalType.Stop;
                            }
                        }
                        else if (laneSignal.m_Signal == goSignalType)
                        {
                            if ((laneSignal.m_GroupMask & num2) == 0)
                            {
                                laneSignal.m_Signal = LaneSignalType.SafeStop;
                            }
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    case Game.Net.TrafficLightState.Extended:
                        if ((laneSignal.m_Flags & LaneSignalFlags.CanExtend) != 0 && (laneSignal.m_GroupMask & num) != 0)
                        {
                            laneSignal.m_Signal = goSignalType;
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    case Game.Net.TrafficLightState.Ending:
                        if (laneSignal.m_Signal == goSignalType)
                        {
                            if ((laneSignal.m_GroupMask & num2) == 0)
                            {
                                laneSignal.m_Signal = LaneSignalType.SafeStop;
                            }
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    case Game.Net.TrafficLightState.Changing:
                        if (laneSignal.m_Signal != goSignalType || (laneSignal.m_GroupMask & num2) == 0)
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    default:
                        laneSignal.m_Signal = LaneSignalType.None;
                        break;
                }
            }

            void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Execute(chunk, unfilteredChunkIndex, useEnabledMask, chunkEnabledMask);
            }
        }

        private EndFrameBarrier m_EndFrameBarrier;

        private EntityQuery m_TrafficLightGroupQuery;

        public SimulationSystem m_SimulationSystem;

        public TimeSystem m_TimeSystem;

        [Preserve]
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
    }
}
