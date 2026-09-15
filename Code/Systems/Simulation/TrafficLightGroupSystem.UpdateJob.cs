using Game.Net;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Utils;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficLightManager.Code.Systems.Simulation
{
    public partial class TrafficLightGroupSystem
    {
        [BurstCompile]
        public struct UpdateTrafficLightGroupJob : IJobChunk
        {
            public uint m_FrameIndex;

            public Settings.Values m_Settings;

            public NativeArray<CustomPhaseTemplate.Values> m_CustomPhaseTemplates;

            public EntityCommandBuffer.ParallelWriter m_EntityCommandBuffer;

            public EntityStorageInfoLookup m_EntityStorageInfoLookup;

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

            private void Tick(
                int unfilteredChunkIndex,
                Entity entity,
                ref TrafficLightGroup trafficLightGroup,
                DynamicBuffer<CustomPhaseData> customPhaseDataBuffer,
                DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefBuffer
            )
            {
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
                            customPhaseData.m_IntervalFactor = template.m_IntervalFactor;
                            customPhaseDataBuffer[j] = customPhaseData;
                        }
                    }
                }

                NativeList<SubLane> allSubLanes = new NativeList<SubLane>(Allocator.Temp);
                NativeList<Entity> allLaneSignalEntities = new NativeList<Entity>(30, Allocator.Temp);
                foreach (var item in trafficLightsMemberRefBuffer)
                {
                    DynamicBuffer<SubLane> itemSubLaneBuffer = m_SubLaneBufferLookup[item.m_Entity];
                    allSubLanes.AddRange(itemSubLaneBuffer.AsNativeArray());
                }
                foreach (var subLane in allSubLanes)
                {
                    Entity value = subLane.m_SubLane;
                    if (m_LaneSignalLookup.HasComponent(value))
                    {
                        allLaneSignalEntities.Add(in value);
                    }
                }

                CalculatePassedCarCount(ref trafficLightGroup, trafficLightsMemberRefBuffer);
                CalculatePriority(allSubLanes, customPhaseDataBuffer);
                UpdateTrafficLightState(ref trafficLightGroup, customPhaseDataBuffer);
                UpdateLaneSignals(allLaneSignalEntities, trafficLightGroup);

                allLaneSignalEntities.Dispose();
                allSubLanes.Dispose();

                foreach (var item in trafficLightsMemberRefBuffer)
                {
                    TrafficLights trafficLights = m_TrafficLightsLookup[item.m_Entity];
                    CustomTrafficLights customTrafficLights = m_CustomTrafficLightsLookup[item.m_Entity];

                    trafficLights.m_State = trafficLightGroup.m_State;
                    trafficLights.m_CurrentSignalGroup = trafficLightGroup.m_CurrentSignalGroup;
                    trafficLights.m_NextSignalGroup = trafficLightGroup.m_NextSignalGroup;
                    customTrafficLights.m_Timer = trafficLightGroup.m_Timer;

                    m_CustomTrafficLightsLookup[item.m_Entity] = customTrafficLights;
                    m_TrafficLightsLookup[item.m_Entity] = trafficLights;
                }
            }

            private void CalculatePassedCarCount(ref TrafficLightGroup trafficLightGroup, DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefs)
            {
                var nextPassedCarCount = 0;
                foreach (var item in trafficLightsMemberRefs)
                {
                    if (m_CustomTrafficLightsLookup.TryGetComponent(item.m_Entity, out var customTrafficLights))
                    {
                        nextPassedCarCount += customTrafficLights.m_NextPassedCarCount;
                        customTrafficLights.m_NextPassedCarCount = 0;
                        m_CustomTrafficLightsLookup[item.m_Entity] = customTrafficLights;
                    }
                }

                trafficLightGroup.m_PassedCarCount = new int4(
                    nextPassedCarCount,
                    trafficLightGroup.m_PassedCarCount.x,
                    trafficLightGroup.m_PassedCarCount.y,
                    trafficLightGroup.m_PassedCarCount.z
                );
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
                    float targetDuration = ((float)math.csum(trafficLightGroup.m_PassedCarCount) / 4) * phase.m_TargetDurationMultiplier;
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
                        * ((float)phase.m_TurnsSinceLastRun / customPhaseDataBuffer.Length)
                        * phase.m_IntervalFactor;
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

                switch (trafficLightGroup.m_State)
                {
                    case Game.Net.TrafficLightState.Beginning:
                        if ((laneSignal.m_GroupMask & num2) != 0)
                        {
                            if (laneSignal.m_Signal != LaneSignalType.Go)
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
                            laneSignal.m_Signal = LaneSignalType.Go;
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
                                laneSignal.m_Signal = LaneSignalType.Go;
                            }
                            else
                            {
                                laneSignal.m_Signal = LaneSignalType.Stop;
                            }
                        }
                        else if (laneSignal.m_Signal == LaneSignalType.Go)
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
                            laneSignal.m_Signal = LaneSignalType.Go;
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    case Game.Net.TrafficLightState.Ending:
                        if (laneSignal.m_Signal == LaneSignalType.Go)
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
                        if (laneSignal.m_Signal != LaneSignalType.Go || (laneSignal.m_GroupMask & num2) == 0)
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }

                        break;
                    default:
                        laneSignal.m_Signal = LaneSignalType.None;
                        break;
                }
            }

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entityArray = chunk.GetNativeArray(m_EntityType);
                NativeArray<TrafficLightGroup> trafficLightGroupArray = chunk.GetNativeArray(ref m_TrafficLightGroupType);
                BufferAccessor<TrafficLightsMemberRef> trafficLightsMemberRefAccessor = chunk.GetBufferAccessor(ref m_TrafficLightsMemberRefType);
                BufferAccessor<CustomPhaseData> customPhaseDataBufferAccessor = chunk.GetBufferAccessor(ref m_CustomPhaseDataBufferType);
                NativeList<Entity> memberEntities = new NativeList<Entity>(Allocator.Temp);

                for (int i = 0; i < entityArray.Length; i++)
                {
                    Entity entity = entityArray[i];
                    TrafficLightGroup trafficLightGroup = trafficLightGroupArray[i];
                    DynamicBuffer<CustomPhaseData> customPhaseDataBuffer = customPhaseDataBufferAccessor[i];
                    DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefBuffer = trafficLightsMemberRefAccessor[i];
                    Tick(unfilteredChunkIndex, entity, ref trafficLightGroup, customPhaseDataBuffer, trafficLightsMemberRefBuffer);
                    trafficLightGroupArray[i] = trafficLightGroup;
                }
                memberEntities.Dispose();
            }
        }
    }
}
