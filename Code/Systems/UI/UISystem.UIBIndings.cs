using System;
using System.Collections.Generic;
using cohtml.Net;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Buildings;
using Game.Net;
using Game.UI;
using Newtonsoft.Json;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    private static GetterValueBinding<string> m_LocalisationBinding;

    private GetterValueBinding<string> m_CityConfigurationBinding;

    private GetterValueBinding<Dictionary<string, UITypes.ScreenPoint>> m_ScreenPointBinding;

    private GetterValueBinding<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>> m_EdgeInfoBinding;

    private GetterValueBinding<string> m_CustomPhaseItemsBinding;

    private GetterValueBinding<string> m_TrafficLightGroupBinding;

    private GetterValueBinding<string> m_TrafficLightsMembersBinding;

    private GetterValueBinding<string> m_SettingsBinding;

    private GetterValueBinding<string> m_TrafficLightManagerGroupNameBinding;

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
                "TrafficLightManager",
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
                "TrafficLightManager",
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
                "TrafficLightManager",
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
                    UITypes.CustomPhaseItem[] rs;
                    if (EntityManager.TryGetBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity, true, out var customPhaseDataBuffer))
                    {
                        rs = new UITypes.CustomPhaseItem[customPhaseDataBuffer.Length];
                        for (int i = 0; i < customPhaseDataBuffer.Length; i++)
                        {
                            CustomPhaseData item = customPhaseDataBuffer[i];
                            rs[i] = new UITypes.CustomPhaseItem
                            {
                                turnsSinceLastRun = item.m_TurnsSinceLastRun,
                                lowFlowTimer = item.m_LowFlowTimer,
                                carFlow = item.AverageCarFlow(),
                                carLaneOccupied = item.m_CarLaneOccupied,
                                publicCarLaneOccupied = item.m_PublicCarLaneOccupied,
                                trackLaneOccupied = item.m_TrackLaneOccupied,
                                pedestrianLaneOccupied = item.m_PedestrianLaneOccupied,
                                weightedWaiting = item.m_WeightedWaiting,
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
                                bindWithTemplate = (item.m_Options & CustomPhaseData.Options.BindWithTemplate) != 0,
                                name = item.m_Name.ToString(),
                                bindTemplate = item.m_BindTemplate.ToString(),
                            };
                        }
                    }
                    else
                    {
                        rs = [];
                    }
                    return JsonConvert.SerializeObject(rs);
                }
            )
        );
        AddBinding(
            m_TrafficLightGroupBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetTrafficLightGroup",
                () =>
                {
                    UITypes.TrafficLightGroup rs;
                    if (EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup))
                    {
                        rs = new UITypes.TrafficLightGroup
                        {
                            timer = trafficLightGroup.m_Timer,
                            currentPhaseIndex = trafficLightGroup.m_CurrentSignalGroup - 1,
                            manualPhaseIndex = trafficLightGroup.m_ManualSignalGroup - 1,
                            targetDuration = trafficLightGroup.m_TargetDuration,
                        };
                    }
                    else
                    {
                        rs = new UITypes.TrafficLightGroup { };
                    }
                    return JsonConvert.SerializeObject(rs);
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
                "TrafficLightManager",
                "GetToolTooltipMessage",
                [],
                new ListWriter<UITypes.ToolTooltipMessage>(new ValueWriter<UITypes.ToolTooltipMessage>())
            )
        );
        AddBinding(m_ToolStateBinding = new ValueBinding<int>("TrafficLightManager", "GetToolState", (int)ToolState.Disabled));
        AddBinding(m_DisplayPhaseIndexBinding = new ValueBinding<int>("TrafficLightManager", "GetDisplayPhaseIndex", -1));
        AddBinding(m_EditingPhaseIndexBinding = new ValueBinding<int>("TrafficLightManager", "GetEditingPhaseIndex", -1));
        AddBinding(
            m_SettingsBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetSettings",
                () =>
                {
                    var result = new { customPhaseTemplates = Mod.m_Settings.GetCustomPhaseTemplates(), defaultCustomPhaseTemplate = Mod.m_Settings.m_DefaultCustomPhaseTemplate };
                    return JsonConvert.SerializeObject(result);
                }
            )
        );
        AddBinding(
            m_TrafficLightManagerGroupNameBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetTrafficLightManagerGroupName",
                () =>
                {
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        if (m_NameSystem.TryGetCustomName(m_SelectedTrafficLightGroupEntity, out var customName))
                        {
                            return customName;
                        }
                    }
                    return string.Empty;
                }
            )
        );

        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
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
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
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
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
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
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
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
                "TrafficLightManager",
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
                "TrafficLightManager",
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
                "TrafficLightManager",
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
            new CallBinding<int, int>(
                "TrafficLightManager",
                "CallCopyPhase",
                (sourceIndex) =>
                {
                    if (EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out DynamicBuffer<CustomPhaseData> customPhaseDataBuffer))
                    {
                        CustomPhaseData source = customPhaseDataBuffer[sourceIndex];
                        customPhaseDataBuffer.Add(source);
                        int destIndex = customPhaseDataBuffer.Length - 1;

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

                                CustomPhaseUtils.CopyBit(edgeGroupMaskBuffer, sourceIndex, destIndex);
                                CustomPhaseUtils.CopyBit(subLaneGroupMaskBuffer, sourceIndex, destIndex);
                                UpdateEdgeInfo(e);
                            }
                        );

                        AddUpdate();

                        return destIndex;
                    }
                    return -1;
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
        AddBinding(new TriggerBinding<int>("TrafficLightManager", "SetToolState", (index) => SetToolState((ToolState)index)));
        AddBinding(
            new TriggerBinding<int>(
                "TrafficLightManager",
                "SetDisplayPhaseIndex",
                (index) =>
                {
                    m_DisplayPhaseIndexBinding.Update(index);
                }
            )
        );
        AddBinding(new TriggerBinding<int>("TrafficLightManager", "SetEditingPhaseIndex", (index) => m_EditingPhaseIndexBinding.Update(index)));
        AddBinding(new TriggerBinding<int>("TrafficLightManager", "SetManualPhaseIndex", UpdateManualPhaseIndex));
        AddBinding(
            new TriggerBinding<string>(
                "TrafficLightManager",
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
                            return;
                        }
                        var newValue = customPhaseDataBuffer[index];

                        if (input.key == "MinimumDuration")
                        {
                            newValue.m_MinimumDuration = ushort.Parse(input.value);
                            if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                            {
                                newValue.m_MaximumDuration = newValue.m_MinimumDuration;
                            }
                        }
                        else if (input.key == "MaximumDuration")
                        {
                            newValue.m_MaximumDuration = ushort.Parse(input.value);
                            if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                            {
                                newValue.m_MinimumDuration = newValue.m_MaximumDuration;
                            }
                        }
                        else if (input.key == "TargetDurationMultiplier")
                        {
                            newValue.m_TargetDurationMultiplier = float.Parse(input.value);
                        }
                        else if (input.key == "LaneOccupiedMultiplier")
                        {
                            newValue.m_LaneOccupiedMultiplier = float.Parse(input.value);
                        }
                        else if (input.key == "IntervalExponent")
                        {
                            newValue.m_IntervalExponent = float.Parse(input.value);
                        }
                        else if (input.key == "PrioritiseTrack")
                        {
                            if (bool.Parse(input.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.PrioritiseTrack;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.PrioritiseTrack;
                            }
                        }
                        else if (input.key == "PrioritisePublicCar")
                        {
                            if (bool.Parse(input.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.PrioritisePublicCar;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePublicCar;
                            }
                        }
                        else if (input.key == "PrioritisePedestrian")
                        {
                            if (bool.Parse(input.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.PrioritisePedestrian;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePedestrian;
                            }
                        }
                        else if (input.key == "LinkedWithNextPhase")
                        {
                            if (bool.Parse(input.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.LinkedWithNextPhase;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.LinkedWithNextPhase;
                            }
                        }
                        else if (input.key == "EndPhasePrematurely")
                        {
                            newValue.m_Options |= CustomPhaseData.Options.EndPhasePrematurely;
                        }
                        else if (input.key == "Name")
                        {
                            newValue.m_Name = new FixedString64Bytes(input.value);
                        }
                        customPhaseDataBuffer[index] = newValue;

                        if (
                            (newValue.m_Options & CustomPhaseData.Options.BindWithTemplate) != 0
                            && Mod.m_Settings.GetCustomPhaseTemplates().FindIndex(t => t.m_Name == newValue.m_BindTemplate) >= 0
                        )
                        {
                            Mod.m_Settings.UpdateCustomPhaseTemplate(
                                new CustomPhaseTemplate
                                {
                                    m_Name = newValue.m_BindTemplate.ToString(),
                                    m_MinimumDuration = newValue.m_MinimumDuration,
                                    m_MaximumDuration = newValue.m_MaximumDuration,
                                    m_TargetDurationMultiplier = newValue.m_TargetDurationMultiplier,
                                    m_LaneOccupiedMultiplier = newValue.m_LaneOccupiedMultiplier,
                                    m_IntervalExponent = newValue.m_IntervalExponent,
                                    m_IsPrioritiseTrack = (newValue.m_Options & CustomPhaseData.Options.PrioritiseTrack) != 0,
                                    m_IsPrioritisePublicCar = (newValue.m_Options & CustomPhaseData.Options.PrioritisePublicCar) != 0,
                                    m_IsPrioritisePedestrian = (newValue.m_Options & CustomPhaseData.Options.PrioritisePedestrian) != 0,
                                }
                            );
                        }

                        ForEachTrafficLight(UpdateEdgeInfo);
                    }
                    return;
                }
            )
        );
        AddBinding(
            new TriggerBinding<string>(
                "TrafficLightManager",
                "UpdatePhaseTemplateBind",
                (input) =>
                {
                    var definition = new
                    {
                        index = 0,
                        bindWithTemplate = false,
                        templateName = "",
                    };
                    var value = JsonConvert.DeserializeAnonymousType(input, definition);
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        if (EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out DynamicBuffer<CustomPhaseData> customPhaseDataBuffer))
                        {
                            var phase = customPhaseDataBuffer[value.index];
                            if (value.bindWithTemplate)
                            {
                                phase.m_Options |= CustomPhaseData.Options.BindWithTemplate;
                                phase.m_BindTemplate = new FixedString64Bytes(value.templateName);
                            }
                            else
                            {
                                phase.m_Options &= ~CustomPhaseData.Options.BindWithTemplate;
                                phase.m_BindTemplate = new FixedString64Bytes();
                            }
                            customPhaseDataBuffer[value.index] = phase;
                        }
                    }
                }
            )
        );
        AddBinding(
            new TriggerBinding<string>(
                "TrafficLightManager",
                "ApplyTemplate",
                (input) =>
                {
                    var definition = new { index = 0, templateName = "" };
                    var value = JsonConvert.DeserializeAnonymousType(input, definition);

                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        if (EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out DynamicBuffer<CustomPhaseData> customPhaseDataBuffer))
                        {
                            var phase = customPhaseDataBuffer[value.index];

                            var templates = Mod.m_Settings.GetCustomPhaseTemplates();
                            var index = templates.FindIndex(t => t.m_Name == value.templateName);
                            if (index >= 0)
                            {
                                phase.m_MinimumDuration = templates[index].m_MinimumDuration;
                                phase.m_MaximumDuration = templates[index].m_MaximumDuration;
                                phase.m_TargetDurationMultiplier = templates[index].m_TargetDurationMultiplier;
                                phase.m_LaneOccupiedMultiplier = templates[index].m_LaneOccupiedMultiplier;
                                phase.m_IntervalExponent = templates[index].m_IntervalExponent;
                                if (templates[index].m_IsPrioritiseTrack)
                                {
                                    phase.m_Options |= CustomPhaseData.Options.PrioritiseTrack;
                                }
                                else
                                {
                                    phase.m_Options &= ~CustomPhaseData.Options.PrioritiseTrack;
                                }
                                if (templates[index].m_IsPrioritisePublicCar)
                                {
                                    phase.m_Options |= CustomPhaseData.Options.PrioritisePublicCar;
                                }
                                else
                                {
                                    phase.m_Options &= ~CustomPhaseData.Options.PrioritisePublicCar;
                                }
                                if (templates[index].m_IsPrioritisePedestrian)
                                {
                                    phase.m_Options |= CustomPhaseData.Options.PrioritisePedestrian;
                                }
                                else
                                {
                                    phase.m_Options &= ~CustomPhaseData.Options.PrioritisePedestrian;
                                }

                                customPhaseDataBuffer[value.index] = phase;
                            }
                        }
                    }
                }
            )
        );
        AddBinding(
            new TriggerBinding<string>(
                "TrafficLightManager",
                "UpdateTemplate",
                (input) =>
                {
                    var template = JsonConvert.DeserializeObject<CustomPhaseTemplate>(input);
                    Mod.m_Settings.UpdateCustomPhaseTemplate(template);
                }
            )
        );
        AddBinding(new TriggerBinding<string>("TrafficLightManager", "RemoveTemplate", (templeteName) => Mod.m_Settings.RemoveCustomPhaseTemplate(templeteName)));
        AddBinding(
            new TriggerBinding<string>(
                "TrafficLightManager",
                "SetTrafficLightGroupName",
                (name) =>
                {
                    if (m_SelectedTrafficLightGroupEntity != Entity.Null)
                    {
                        m_NameSystem.SetCustomName(m_SelectedTrafficLightGroupEntity, name);
                        m_TrafficLightManagerGroupNameBinding.Update();
                    }
                }
            )
        );
    }

    public void SettingUpdate()
    {
        m_SettingsBinding.Update();
    }

    public void UpdateManualPhaseIndex(int index)
    {
        if (m_SelectedTrafficLightGroupEntity != Entity.Null)
        {
            EntityManager.TryGetComponent(m_SelectedTrafficLightGroupEntity, out TrafficLightGroup trafficLightGroup);
            trafficLightGroup.m_ManualSignalGroup = (byte)(index + 1);
            EntityManager.SetComponentData(m_SelectedTrafficLightGroupEntity, trafficLightGroup);
            m_TrafficLightGroupBinding.Update();
        }
    }
}
