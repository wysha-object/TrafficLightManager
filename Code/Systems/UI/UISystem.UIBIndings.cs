using System.Collections.Generic;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.UI;
using Newtonsoft.Json;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Utils;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    private static GetterValueBinding<string> m_LocalisationBinding;

    private GetterValueBinding<string> m_CityConfigurationBinding;

    private GetterValueBinding<Dictionary<string, UITypes.ScreenPoint>> m_ScreenPointBinding;

    private GetterValueBinding<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>> m_EdgeInfoBinding;

    private GetterValueBinding<string> m_CustomPhaseItemsBinding;

    private GetterValueBinding<uint> m_TimerBinding;

    private GetterValueBinding<int> m_CurrentSignalGroupBinding;

    private GetterValueBinding<int> m_ManualSignalGroupBinding;

    private GetterValueBinding<string> m_TrafficLightsMembersBinding;

    private ValueBinding<UITypes.ToolTooltipMessage[]> m_ToolTooltipMessageBinding;

    private ValueBinding<int> m_ToolStateBinding;

    private ValueBinding<int> m_DisplayPhaseIndexBinding;

    private ValueBinding<int> m_EditingPhaseIndexBinding;

    private void AddUIBindings()
    {
        AddBinding(
            m_LocalisationBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetLocalisation",
                () =>
                {
                    var result = LocalisationUtils.GetActiveDictionary();

                    return JsonConvert.SerializeObject(result);
                }
            )
        );
        AddBinding(
            m_CityConfigurationBinding = new GetterValueBinding<string>(
                "C2VM.TLE",
                "GetCityConfiguration",
                () =>
                {
                    var result = new { leftHandTraffic = m_CityConfigurationSystem.leftHandTraffic };

                    return JsonConvert.SerializeObject(result);
                }
            )
        );
        AddBinding(
            m_ScreenPointBinding = new GetterValueBinding<Dictionary<string, UITypes.ScreenPoint>>(
                "C2VM.TLE",
                "GetScreenPoint",
                () =>
                {
                    Dictionary<string, UITypes.ScreenPoint> screenPointDictionary = [];
                    m_Camera = Camera.main;
                    m_ScreenHeight = Screen.height;
                    foreach (var wp in m_WorldPositionList)
                    {
                        if (!screenPointDictionary.ContainsKey(wp))
                        {
                            screenPointDictionary[wp] = new UITypes.ScreenPoint(m_Camera.WorldToScreenPoint(wp), m_ScreenHeight);
                        }
                    }
                    return screenPointDictionary;
                },
                new DictionaryWriter<string, UITypes.ScreenPoint>(null, new ValueWriter<UITypes.ScreenPoint>()),
                new JsonWriter.FalseEqualityComparer<Dictionary<string, UITypes.ScreenPoint>>()
            )
        );
        AddBinding(
            m_EdgeInfoBinding = new GetterValueBinding<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>>(
                "C2VM.TLE",
                "GetEdgeInfo",
                () =>
                {
                    return m_EdgeInfoDictionary;
                },
                new JsonWriter.EdgeInfoWriter(),
                new JsonWriter.FalseEqualityComparer<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>>()
            )
        );
        AddBinding(
            m_CustomPhaseItemsBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetCustomPhaseItems",
                () =>
                {
                    UITypes.CustomPhaseItem[] customPhaseItems;
                    if (
                        EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup)
                        && EntityManager.TryGetBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity, true, out var customPhaseDataBuffer)
                    )
                    {
                        customPhaseItems = new UITypes.CustomPhaseItem[customPhaseDataBuffer.Length];
                        for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                        {
                            CustomPhaseData item = customPhaseDataBuffer[i];
                            customPhaseItems[i] = new UITypes.CustomPhaseItem
                            {
                                turnsSinceLastRun = item.m_TurnsSinceLastRun,
                                lowFlowTimer = item.m_LowFlowTimer,
                                carFlow = item.AverageCarFlow(),
                                carLaneOccupied = item.m_CarLaneOccupied,
                                publicCarLaneOccupied = item.m_PublicCarLaneOccupied,
                                trackLaneOccupied = item.m_TrackLaneOccupied,
                                pedestrianLaneOccupied = item.m_PedestrianLaneOccupied,
                                weightedWaiting = item.m_WeightedWaiting,
                                targetDuration = item.m_TargetDuration,
                                priority = item.m_Priority,
                                minimumDuration = item.m_MinimumDuration,
                                maximumDuration = item.m_MaximumDuration,
                                targetDurationMultiplier = item.m_TargetDurationMultiplier,
                                laneOccupiedMultiplier = item.m_LaneOccupiedMultiplier,
                                intervalExponent = item.m_IntervalExponent,
                                prioritiseTrack = (item.m_Options & CustomPhaseData.Options.PrioritiseTrack) != 0,
                                prioritisePublicCar = (item.m_Options & CustomPhaseData.Options.PrioritisePublicCar) != 0,
                                prioritisePedestrian = (item.m_Options & CustomPhaseData.Options.PrioritisePedestrian) != 0,
                                linkedWithNextPhase = (item.m_Options & CustomPhaseData.Options.LinkedWithNextPhase) != 0,
                                endPhasePrematurely = (item.m_Options & CustomPhaseData.Options.EndPhasePrematurely) != 0,
                            };
                        }
                    }
                    else
                    {
                        customPhaseItems = [];
                    }
                    return JsonConvert.SerializeObject(customPhaseItems);
                }
            )
        );
        AddBinding(
            m_TimerBinding = new GetterValueBinding<uint>(
                "TrafficLightManager",
                "GetTimer",
                () =>
                {
                    if (EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup))
                    {
                        return trafficLightGroup.m_Timer;
                    }
                    return 0;
                }
            )
        );
        AddBinding(
            m_CurrentSignalGroupBinding = new GetterValueBinding<int>(
                "TrafficLightManager",
                "GetCurrentPhaseIndex",
                () =>
                {
                    if (EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup))
                    {
                        return trafficLightGroup.m_CurrentSignalGroup - 1;
                    }
                    return 0;
                }
            )
        );
        AddBinding(
            m_ManualSignalGroupBinding = new GetterValueBinding<int>(
                "TrafficLightManager",
                "GetManualPhaseIndex",
                () =>
                {
                    if (EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup))
                    {
                        return trafficLightGroup.m_ManualSignalGroup - 1;
                    }
                    return 0;
                }
            )
        );
        AddBinding(
            m_TrafficLightsMembersBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetTrafficLightsMembers",
                () =>
                {
                    List<object> members = [];
                    if (EntityManager.TryGetBuffer<TrafficLightsMemberRef>(m_SelectedTrafficLightGroupEntity, true, out var trafficLightsMembers))
                    {
                        foreach (var trafficLight in trafficLightsMembers)
                        {
                            members.Add(new { entityIndex = trafficLight.m_Entity.Index });
                        }
                    }
                    return JsonConvert.SerializeObject(members);
                }
            )
        );

        AddBinding(
            m_ToolTooltipMessageBinding = new ValueBinding<UITypes.ToolTooltipMessage[]>(
                "C2VM.TLE",
                "GetToolTooltipMessage",
                [],
                new ListWriter<UITypes.ToolTooltipMessage>(new ValueWriter<UITypes.ToolTooltipMessage>())
            )
        );
        AddBinding(m_ToolStateBinding = new ValueBinding<int>("TrafficLightManager", "GetToolState", (int)ToolState.Disabled));
        AddBinding(m_DisplayPhaseIndexBinding = new ValueBinding<int>("TrafficLightManager", "GetDisplayPhaseIndex", -1));
        AddBinding(m_EditingPhaseIndexBinding = new ValueBinding<int>("TrafficLightManager", "GetEditingPhaseIndex", -1));

        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallAddCustomPhase",
                (_) =>
                {
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }
                        customPhaseDataBuffer.Add(new CustomPhaseData());
                        ForEachTrafficLight(UpdateEdgeInfo);
                        m_CustomPhaseItemsBinding.Update();
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallRemoveCustomPhase",
                (input) =>
                {
                    var definition = new { index = 0 };
                    var value = JsonConvert.DeserializeAnonymousType(input, definition);
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }
                        customPhaseDataBuffer.RemoveAt(value.index);

                        ForEachTrafficLight(
                            (e) =>
                            {
                                DynamicBuffer<EdgeGroupMask> edgeGroupMaskBuffer;
                                DynamicBuffer<SubLaneGroupMask> subLaneGroupMaskBuffer;
                                if (!EntityManager.TryGetBuffer(e, false, out edgeGroupMaskBuffer))
                                {
                                    edgeGroupMaskBuffer = EntityManager.AddBuffer<EdgeGroupMask>(e);
                                }
                                if (!EntityManager.TryGetBuffer(e, false, out subLaneGroupMaskBuffer))
                                {
                                    subLaneGroupMaskBuffer = EntityManager.AddBuffer<SubLaneGroupMask>(e);
                                }

                                for (int i = value.index; i < 16; i++)
                                {
                                    CustomPhaseUtils.SwapBit(subLaneGroupMaskBuffer, i, i + 1);
                                    CustomPhaseUtils.SwapBit(edgeGroupMaskBuffer, i, i + 1);
                                }

                                UpdateEdgeInfo(e);
                            }
                        );
                        m_CustomPhaseItemsBinding.Update();
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallSwapCustomPhase",
                (input) =>
                {
                    var definition = new { index1 = 0, index2 = 0 };
                    var value = JsonConvert.DeserializeAnonymousType(input, definition);
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }
                        (customPhaseDataBuffer[value.index2], customPhaseDataBuffer[value.index1]) = (customPhaseDataBuffer[value.index1], customPhaseDataBuffer[value.index2]);

                        ForEachTrafficLight(
                            (e) =>
                            {
                                DynamicBuffer<EdgeGroupMask> edgeGroupMaskBuffer;
                                if (!EntityManager.TryGetBuffer(e, false, out edgeGroupMaskBuffer))
                                {
                                    edgeGroupMaskBuffer = EntityManager.AddBuffer<EdgeGroupMask>(e);
                                }
                                DynamicBuffer<SubLaneGroupMask> subLaneGroupMaskBuffer;
                                if (!EntityManager.TryGetBuffer(e, false, out subLaneGroupMaskBuffer))
                                {
                                    subLaneGroupMaskBuffer = EntityManager.AddBuffer<SubLaneGroupMask>(e);
                                }

                                CustomPhaseUtils.SwapBit(edgeGroupMaskBuffer, value.index1, value.index2);
                                CustomPhaseUtils.SwapBit(subLaneGroupMaskBuffer, value.index1, value.index2);
                                UpdateEdgeInfo(e);
                            }
                        );
                        m_CustomPhaseItemsBinding.Update();
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallUpdateEdgeGroupMask",
                (input) =>
                {
                    if (m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        return "";
                    }

                    var definition = new { groupMaskArray = new EdgeGroupMask[0], entity = Entity.Null };
                    var value = JsonConvert.DeserializeAnonymousType(input, definition);
                    EdgeGroupMask[] groupMaskArray = value.groupMaskArray;
                    Entity entity = value.entity;

                    ForEachTrafficLight(
                        (e) =>
                        {
                            if (e == entity)
                            {
                                DynamicBuffer<EdgeGroupMask> groupMaskBuffer;
                                if (EntityManager.HasBuffer<EdgeGroupMask>(e))
                                {
                                    groupMaskBuffer = EntityManager.GetBuffer<EdgeGroupMask>(e, false);
                                }
                                else
                                {
                                    groupMaskBuffer = EntityManager.AddBuffer<EdgeGroupMask>(e);
                                }
                                foreach (var newValue in groupMaskArray)
                                {
                                    int index = CustomPhaseUtils.TryGet(groupMaskBuffer, newValue, out EdgeGroupMask oldValue);
                                    if (index >= 0)
                                    {
                                        groupMaskBuffer[index] = new EdgeGroupMask(oldValue, newValue);
                                    }
                                    else
                                    {
                                        groupMaskBuffer.Add(new EdgeGroupMask(oldValue, newValue));
                                    }
                                }
                                UpdateEdgeInfo(e);
                            }
                        }
                    );
                    AddUpdate();

                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallUpdateSubLaneGroupMask",
                (input) =>
                {
                    if (m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        return "";
                    }

                    var definition = new { groupMaskArray = new SubLaneGroupMask[0], entity = Entity.Null };
                    var value = JsonConvert.DeserializeAnonymousType(input, definition);
                    SubLaneGroupMask[] groupMaskArray = value.groupMaskArray;
                    Entity entity = value.entity;

                    ForEachTrafficLight(
                        (e) =>
                        {
                            if (e == entity)
                            {
                                DynamicBuffer<SubLaneGroupMask> groupMaskBuffer;
                                if (EntityManager.HasBuffer<SubLaneGroupMask>(e))
                                {
                                    groupMaskBuffer = EntityManager.GetBuffer<SubLaneGroupMask>(e, false);
                                }
                                else
                                {
                                    groupMaskBuffer = EntityManager.AddBuffer<SubLaneGroupMask>(e);
                                }
                                foreach (var newValue in groupMaskArray)
                                {
                                    int index = CustomPhaseUtils.TryGet(groupMaskBuffer, newValue, out SubLaneGroupMask oldValue);
                                    if (index >= 0)
                                    {
                                        groupMaskBuffer[index] = new SubLaneGroupMask(oldValue, newValue);
                                    }
                                    else
                                    {
                                        groupMaskBuffer.Add(new SubLaneGroupMask(oldValue, newValue));
                                    }
                                }
                                UpdateEdgeInfo(e);
                            }
                        }
                    );
                    AddUpdate();

                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallUpdateCustomPhaseData",
                (jsonString) =>
                {
                    var input = JsonConvert.DeserializeObject<UITypes.UpdateCustomPhaseData>(jsonString);
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }

                        int index = input.index;
                        if (index < 0 || index >= customPhaseDataBuffer.Length)
                        {
                            return "";
                        }
                        var newValue = customPhaseDataBuffer[index];

                        if (input.key == "MinimumDuration")
                        {
                            newValue.m_MinimumDuration = (ushort)input.value;
                            if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                            {
                                newValue.m_MaximumDuration = newValue.m_MinimumDuration;
                            }
                        }
                        else if (input.key == "MaximumDuration")
                        {
                            newValue.m_MaximumDuration = (ushort)input.value;
                            if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                            {
                                newValue.m_MinimumDuration = newValue.m_MaximumDuration;
                            }
                        }
                        else if (input.key == "TargetDurationMultiplier")
                        {
                            newValue.m_TargetDurationMultiplier = (float)input.value;
                        }
                        else if (input.key == "LaneOccupiedMultiplier")
                        {
                            newValue.m_LaneOccupiedMultiplier = (float)input.value;
                        }
                        else if (input.key == "IntervalExponent")
                        {
                            newValue.m_IntervalExponent = (float)input.value;
                        }
                        else if (input.key == "PrioritiseTrack")
                        {
                            newValue.m_Options ^= CustomPhaseData.Options.PrioritiseTrack;
                        }
                        else if (input.key == "PrioritisePublicCar")
                        {
                            newValue.m_Options ^= CustomPhaseData.Options.PrioritisePublicCar;
                        }
                        else if (input.key == "PrioritisePedestrian")
                        {
                            newValue.m_Options ^= CustomPhaseData.Options.PrioritisePedestrian;
                        }
                        else if (input.key == "LinkedWithNextPhase")
                        {
                            newValue.m_Options ^= CustomPhaseData.Options.LinkedWithNextPhase;
                        }
                        else if (input.key == "EndPhasePrematurely")
                        {
                            newValue.m_Options ^= CustomPhaseData.Options.EndPhasePrematurely;
                        }
                        customPhaseDataBuffer[index] = newValue;

                        m_CustomPhaseItemsBinding.Update();
                        ForEachTrafficLight(UpdateEdgeInfo);
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallAddWorldPosition",
                (input) =>
                {
                    UITypes.WorldPosition[] posArray = JsonConvert.DeserializeObject<UITypes.WorldPosition[]>(input);
                    foreach (var pos in posArray)
                    {
                        m_WorldPositionList.Add(pos);
                    }
                    m_CameraPosition = float.MaxValue; // Trigger binding update
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallRemoveWorldPosition",
                (input) =>
                {
                    UITypes.WorldPosition[] posArray = JsonConvert.DeserializeObject<UITypes.WorldPosition[]>(input);
                    foreach (var pos in posArray)
                    {
                        m_WorldPositionList.Remove(pos);
                    }
                    m_CameraPosition = float.MaxValue; // Trigger binding update
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "C2VM.TLE",
                "CallOpenBrowser",
                (jsonString) =>
                {
                    var keyDefinition = new { key = "", value = "" };
                    var parsedKey = JsonConvert.DeserializeAnonymousType(jsonString, keyDefinition);
                    System.Diagnostics.Process.Start(parsedKey.value);
                    return "";
                }
            )
        );

        AddBinding(
            new TriggerBinding<int>(
                "TrafficLightManager",
                "SetToolState",
                (state) =>
                {
                    SetToolState((ToolState)state);
                }
            )
        );
        AddBinding(
            new TriggerBinding<int>(
                "C2VM.TLE",
                "SetDebugDisplayGroup",
                (group) =>
                {
                    m_DebugDisplayGroup = group;
                    RedrawGizmo();
                }
            )
        );
        AddBinding(new TriggerBinding<int>("TrafficLightManager", "SetToolState", (index) => SetToolState((ToolState)index)));
        AddBinding(
            new TriggerBinding<int>(
                "TrafficLightManager",
                "SetDisplayPhaseIndex",
                (index) =>
                {
                    m_DisplayPhaseIndexBinding.Update(index);
                    RedrawGizmo();
                }
            )
        );
        AddBinding(new TriggerBinding<int>("TrafficLightManager", "SetEditingPhaseIndex", (index) => m_EditingPhaseIndexBinding.Update(index)));
        AddBinding(new TriggerBinding<int>("TrafficLightManager", "SetManualPhaseIndex", UpdateManualPhaseIndex));
    }

    public void UpdateManualPhaseIndex(int index)
    {
        if (m_SelectedTrafficLightGroupEntity != Entity.Null)
        {
            EntityManager.TryGetComponent(m_SelectedTrafficLightGroupEntity, out TrafficLightGroup trafficLightGroup);
            trafficLightGroup.m_ManualSignalGroup = (byte)(index + 1);
            EntityManager.SetComponentData(m_SelectedTrafficLightGroupEntity, trafficLightGroup);
            m_ManualSignalGroupBinding.Update();
            RedrawGizmo();
        }
    }
}
