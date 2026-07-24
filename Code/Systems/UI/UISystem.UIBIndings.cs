using System.Collections.Generic;
using cohtml.Net;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Net;
using Game.Rendering;
using Game.UI;
using Newtonsoft.Json;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    private ValueBinding<int> m_ToolStateBinding;

    private ValueBinding<int> m_DisplayPhaseIndexBinding;

    private GetterValueBinding<string> m_LocalisationBinding;

    private GetterValueBinding<string> m_CityConfigurationBinding;

    private GetterValueBinding<Dictionary<string, UITypes.ScreenPoint>> m_ScreenPointBinding;

    private GetterValueBinding<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>> m_EdgeInfoBinding;

    private GetterValueBinding<string> m_CustomPhaseItemsBinding;

    private GetterValueBinding<string> m_TrafficLightGroupBinding;

    private GetterValueBinding<string> m_TrafficLightsMembersBinding;

    private GetterValueBinding<string> m_SettingsBinding;

    private GetterValueBinding<string> m_TrafficLightManagerGroupNameBinding;

    private GetterValueBinding<string> m_SystemDefaultTemplateBinding;

    private void AddUIBindings()
    {
        AddBinding(m_ToolStateBinding = new ValueBinding<int>("TrafficLightManager", "GetToolState", (int)ToolState.Disabled));
        AddBinding(m_DisplayPhaseIndexBinding = new ValueBinding<int>("TrafficLightManager", "GetDisplayPhaseIndex", -1));

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
                            object? entityPosition = EntityManager.TryGetComponent<Node>(trafficLight.m_Entity, out var node)
                                ? new
                                {
                                    x = node.m_Position.x,
                                    y = node.m_Position.y,
                                    z = node.m_Position.z,
                                }
                                : null;
                            members.Add(new { entityIndex = trafficLight.m_Entity.Index, position = entityPosition });
                        }
                    }
                    return JsonConvert.SerializeObject(members);
                }
            )
        );
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
            m_SystemDefaultTemplateBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetSystemDefaultTemplate",
                () =>
                {
                    return JsonConvert.SerializeObject(CustomPhaseTemplate.Default);
                }
            )
        );

        AddBinding(
            new CallBinding<int, string>(
                "TrafficLightManager",
                "SetToolState",
                (inputValue) =>
                {
                    SetToolState((ToolState)inputValue);
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<int, string>(
                "TrafficLightManager",
                "SetDisplayPhaseIndex",
                (index) =>
                {
                    SetDisplayPhaseIndex(index);
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<int, string>(
                "TrafficLightManager",
                "SetManualPhaseIndex",
                (inputValue) =>
                {
                    UpdateManualPhaseIndex(inputValue);
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
                "SetCustomPhaseData",
                (inputJsonString) =>
                {
                    var inputValue = JsonConvert.DeserializeObject<UITypes.UpdateCustomPhaseData>(inputJsonString);
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }

                        int index = inputValue.index;
                        if (index < 0 || index >= customPhaseDataBuffer.Length)
                        {
                            return "";
                        }
                        var newValue = customPhaseDataBuffer[index];

                        if (inputValue.key == "MinimumDuration")
                        {
                            newValue.m_MinimumDuration = ushort.Parse(inputValue.value);
                            if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                            {
                                newValue.m_MaximumDuration = newValue.m_MinimumDuration;
                            }
                        }
                        else if (inputValue.key == "MaximumDuration")
                        {
                            newValue.m_MaximumDuration = ushort.Parse(inputValue.value);
                            if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                            {
                                newValue.m_MinimumDuration = newValue.m_MaximumDuration;
                            }
                        }
                        else if (inputValue.key == "TargetDurationMultiplier")
                        {
                            newValue.m_TargetDurationMultiplier = float.Parse(inputValue.value);
                        }
                        else if (inputValue.key == "LaneOccupiedMultiplier")
                        {
                            newValue.m_LaneOccupiedMultiplier = float.Parse(inputValue.value);
                        }
                        else if (inputValue.key == "IntervalExponent")
                        {
                            newValue.m_IntervalExponent = float.Parse(inputValue.value);
                        }
                        else if (inputValue.key == "PrioritiseTrack")
                        {
                            if (bool.Parse(inputValue.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.PrioritiseTrack;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.PrioritiseTrack;
                            }
                        }
                        else if (inputValue.key == "PrioritisePublicCar")
                        {
                            if (bool.Parse(inputValue.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.PrioritisePublicCar;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePublicCar;
                            }
                        }
                        else if (inputValue.key == "PrioritisePedestrian")
                        {
                            if (bool.Parse(inputValue.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.PrioritisePedestrian;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePedestrian;
                            }
                        }
                        else if (inputValue.key == "LinkedWithNextPhase")
                        {
                            if (bool.Parse(inputValue.value))
                            {
                                newValue.m_Options |= CustomPhaseData.Options.LinkedWithNextPhase;
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.LinkedWithNextPhase;
                            }
                        }
                        else if (inputValue.key == "EndPhasePrematurely")
                        {
                            newValue.m_Options |= CustomPhaseData.Options.EndPhasePrematurely;
                        }
                        else if (inputValue.key == "Name")
                        {
                            newValue.m_Name = new FixedString64Bytes(inputValue.value);
                        }
                        else if (inputValue.key == "BindWithTemplate")
                        {
                            var definition = new { bindWithTemplate = false, templateName = "" };
                            var value = JsonConvert.DeserializeAnonymousType(inputValue.value, definition);

                            if (value.bindWithTemplate)
                            {
                                newValue.m_Options |= CustomPhaseData.Options.BindWithTemplate;
                                newValue.m_BindTemplate = new FixedString64Bytes(value.templateName);
                            }
                            else
                            {
                                newValue.m_Options &= ~CustomPhaseData.Options.BindWithTemplate;
                                newValue.m_BindTemplate = new FixedString64Bytes();
                            }
                        }
                        else if (inputValue.key == "ApplyTemplate")
                        {
                            var templates = Mod.m_Settings.GetCustomPhaseTemplates();
                            var templateIndex = templates.FindIndex(t => t.m_Name == inputValue.value);
                            if (templateIndex >= 0)
                            {
                                newValue.m_MinimumDuration = templates[templateIndex].m_MinimumDuration;
                                newValue.m_MaximumDuration = templates[templateIndex].m_MaximumDuration;
                                newValue.m_TargetDurationMultiplier = templates[templateIndex].m_TargetDurationMultiplier;
                                newValue.m_LaneOccupiedMultiplier = templates[templateIndex].m_LaneOccupiedMultiplier;
                                newValue.m_IntervalExponent = templates[templateIndex].m_IntervalExponent;
                                if (templates[templateIndex].m_IsPrioritiseTrack)
                                {
                                    newValue.m_Options |= CustomPhaseData.Options.PrioritiseTrack;
                                }
                                else
                                {
                                    newValue.m_Options &= ~CustomPhaseData.Options.PrioritiseTrack;
                                }
                                if (templates[templateIndex].m_IsPrioritisePublicCar)
                                {
                                    newValue.m_Options |= CustomPhaseData.Options.PrioritisePublicCar;
                                }
                                else
                                {
                                    newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePublicCar;
                                }
                                if (templates[templateIndex].m_IsPrioritisePedestrian)
                                {
                                    newValue.m_Options |= CustomPhaseData.Options.PrioritisePedestrian;
                                }
                                else
                                {
                                    newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePedestrian;
                                }
                            }
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
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
                "SetTemplate",
                (inputValue) =>
                {
                    var template = JsonConvert.DeserializeObject<CustomPhaseTemplate>(inputValue);
                    Mod.m_Settings.UpdateCustomPhaseTemplate(template);
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
                "RemoveTemplate",
                (templeteName) =>
                {
                    Mod.m_Settings.RemoveCustomPhaseTemplate(templeteName);
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
                "SetTrafficLightGroupName",
                (name) =>
                {
                    if (m_SelectedTrafficLightGroupEntity != Entity.Null)
                    {
                        m_NameSystem.SetCustomName(m_SelectedTrafficLightGroupEntity, name);
                        m_TrafficLightManagerGroupNameBinding.Update();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
                "AddCustomPhase",
                (_) =>
                {
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }
                        customPhaseDataBuffer.Add(new CustomPhaseData(Mod.m_Settings.m_DefaultCustomPhaseTemplate));
                        ForEachTrafficLight(UpdateEdgeInfo);
                        AddUpdate();
                    }
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<int, string>(
                "TrafficLightManager",
                "RemoveCustomPhase",
                (inputValue) =>
                {
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }
                        customPhaseDataBuffer.RemoveAt(inputValue);

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

                                for (int i = inputValue; i < 16; i++)
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
                "SwapCustomPhase",
                (inputJsonString) =>
                {
                    var inputDefinition = new { index1 = 0, index2 = 0 };
                    var inputValue = JsonConvert.DeserializeAnonymousType(inputJsonString, inputDefinition);
                    if (!m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        DynamicBuffer<CustomPhaseData> customPhaseDataBuffer;
                        if (!EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, false, out customPhaseDataBuffer))
                        {
                            customPhaseDataBuffer = EntityManager.AddBuffer<CustomPhaseData>(m_SelectedTrafficLightGroupEntity);
                        }
                        (customPhaseDataBuffer[inputValue.index2], customPhaseDataBuffer[inputValue.index1]) = (
                            customPhaseDataBuffer[inputValue.index1],
                            customPhaseDataBuffer[inputValue.index2]
                        );

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

                                CustomPhaseUtils.SwapBit(edgeGroupMaskBuffer, inputValue.index1, inputValue.index2);
                                CustomPhaseUtils.SwapBit(subLaneGroupMaskBuffer, inputValue.index1, inputValue.index2);
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
                "UpdateEdgeGroupMask",
                (inputJsonString) =>
                {
                    if (m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        return "";
                    }

                    var inputDefinition = new { groupMaskArray = new EdgeGroupMask[0], entity = Entity.Null };
                    var inputValue = JsonConvert.DeserializeAnonymousType(inputJsonString, inputDefinition);
                    EdgeGroupMask[] groupMaskArray = inputValue.groupMaskArray;
                    Entity entity = inputValue.entity;

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
                "UpdateSubLaneGroupMask",
                (inputJsonString) =>
                {
                    if (m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
                    {
                        return "";
                    }

                    var inputDefinition = new { groupMaskArray = new SubLaneGroupMask[0], entity = Entity.Null };
                    var inputValue = JsonConvert.DeserializeAnonymousType(inputJsonString, inputDefinition);
                    SubLaneGroupMask[] groupMaskArray = inputValue.groupMaskArray;
                    Entity entity = inputValue.entity;

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
                "AddWorldPosition",
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
                "RemoveWorldPosition",
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
                "TrafficLightManager",
                "LookAt",
                (inputJsonString) =>
                {
                    var inputDefinition = new
                    {
                        x = 0f,
                        y = 0f,
                        z = 0f,
                        distance = 0f,
                    };
                    var inputValue = JsonConvert.DeserializeAnonymousType(inputJsonString, inputDefinition);
                    var pivot = new float3(inputValue.x, inputValue.y, inputValue.z);
                    var zoom = inputValue.distance;

                    var cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
                    cameraUpdateSystem.activeCameraController.pivot = pivot;
                    cameraUpdateSystem.activeCameraController.zoom = zoom;
                    return "";
                }
            )
        );
        AddBinding(
            new CallBinding<int, int>(
                "TrafficLightManager",
                "CopyPhase",
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
        AddBinding(new CallBinding<string, string>("TrafficLightManager", "GetStorage", (inputValue) => Mod.m_Settings.GetStorage(inputValue)));
        AddBinding(
            new CallBinding<string, string>(
                "TrafficLightManager",
                "UpdateStorage",
                (inputJsonString) =>
                {
                    var inputDefinition = new { key = "", value = "" };
                    var inputValue = JsonConvert.DeserializeAnonymousType(inputJsonString, inputDefinition);
                    Mod.m_Settings.UpdateStorage(inputValue.key, inputValue.value);
                    return "";
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
