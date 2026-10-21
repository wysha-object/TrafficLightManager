using System.Collections.Generic;
using cohtml.Net;
using Colossal.Entities;
using Colossal.UI.Binding;
using Game.Net;
using Game.Rendering;
using Game.UI;
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

    private GetterValueBinding<UITypes.CityConfiguration> m_CityConfigurationBinding;

    private GetterValueBinding<UITypes.Camera> m_CameraBinding;

    private GetterValueBinding<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>> m_EdgeInfoBinding;

    private GetterValueBinding<UITypes.CustomPhaseItem[]> m_CustomPhaseItemsBinding;

    private GetterValueBinding<UITypes.TrafficLightGroup> m_TrafficLightGroupBinding;

    private GetterValueBinding<UITypes.TrafficLightMember[]> m_TrafficLightsMembersBinding;

    private GetterValueBinding<UITypes.SettingsValue> m_SettingsBinding;

    private GetterValueBinding<string> m_TrafficLightManagerGroupNameBinding;

    private GetterValueBinding<UITypes.TemplateValue> m_SystemDefaultTemplateBinding;

    private void AddUIBindings()
    {
        AddBinding(m_ToolStateBinding = new ValueBinding<int>("TrafficLightManager", "GetToolState", (int)ToolState.Disabled));
        AddBinding(m_DisplayPhaseIndexBinding = new ValueBinding<int>("TrafficLightManager", "GetDisplayPhaseIndex", -1));

        AddBinding(
            m_CityConfigurationBinding = new GetterValueBinding<UITypes.CityConfiguration>("TrafficLightManager", "GetCityConfiguration", GetCityConfiguration)
        );
        AddBinding(m_CameraBinding = new GetterValueBinding<UITypes.Camera>("TrafficLightManager", "GetCamera", GetCamera));
        AddBinding(
            m_EdgeInfoBinding = new GetterValueBinding<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>>(
                "TrafficLightManager",
                "GetEdgeInfo",
                GetEdgeInfo,
                new JsonWriter.EdgeInfoWriter(),
                new JsonWriter.FalseEqualityComparer<Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>>>()
            )
        );
        AddBinding(
            m_CustomPhaseItemsBinding = new GetterValueBinding<UITypes.CustomPhaseItem[]>(
                "TrafficLightManager",
                "GetCustomPhaseItems",
                GetCustomPhaseItems,
                new ListWriter<UITypes.CustomPhaseItem>()
            )
        );
        AddBinding(
            m_TrafficLightGroupBinding = new GetterValueBinding<UITypes.TrafficLightGroup>("TrafficLightManager", "GetTrafficLightGroup", GetTrafficLightGroup)
        );
        AddBinding(
            m_TrafficLightsMembersBinding = new GetterValueBinding<UITypes.TrafficLightMember[]>(
                "TrafficLightManager",
                "GetTrafficLightsMembers",
                GetTrafficLightsMembers,
                new ListWriter<UITypes.TrafficLightMember>()
            )
        );
        AddBinding(m_SettingsBinding = new GetterValueBinding<UITypes.SettingsValue>("TrafficLightManager", "GetSettings", GetSettings));
        AddBinding(
            m_TrafficLightManagerGroupNameBinding = new GetterValueBinding<string>(
                "TrafficLightManager",
                "GetTrafficLightManagerGroupName",
                GetTrafficLightManagerGroupName
            )
        );
        AddBinding(
            m_SystemDefaultTemplateBinding = new GetterValueBinding<UITypes.TemplateValue>(
                "TrafficLightManager",
                "GetSystemDefaultTemplate",
                GetSystemDefaultTemplate
            )
        );

        AddBinding(new CallBinding<int, int>("TrafficLightManager", "SetToolState", SetToolState));
        AddBinding(new CallBinding<int, int>("TrafficLightManager", "SetDisplayPhaseIndex", SetDisplayPhaseIndexBindingCallback));
        AddBinding(new CallBinding<int, int>("TrafficLightManager", "SetManualPhaseIndex", SetManualPhaseIndex));
        AddBinding(new CallBinding<UITypes.UpdateCustomPhaseData, int>("TrafficLightManager", "SetCustomPhaseData", SetCustomPhaseData));
        AddBinding(new CallBinding<UITypes.TemplateValue, int>("TrafficLightManager", "SetTemplate", SetTemplate));
        AddBinding(new CallBinding<string, int>("TrafficLightManager", "RemoveTemplate", RemoveTemplate));
        AddBinding(new CallBinding<string, int>("TrafficLightManager", "SetTrafficLightGroupName", SetTrafficLightGroupName));
        AddBinding(new CallBinding<string, int>("TrafficLightManager", "AddCustomPhase", AddCustomPhase));
        AddBinding(new CallBinding<int, int>("TrafficLightManager", "RemoveCustomPhase", RemoveCustomPhase));
        AddBinding(new CallBinding<UITypes.SwapCustomPhaseInput, int>("TrafficLightManager", "SwapCustomPhase", SwapCustomPhase));
        AddBinding(new CallBinding<UITypes.UpdateGroupMaskInput<EdgeGroupMask>, int>("TrafficLightManager", "UpdateEdgeGroupMask", UpdateEdgeGroupMask));
        AddBinding(new CallBinding<UITypes.UpdateGroupMaskInput<SubLaneGroupMask>, int>("TrafficLightManager", "UpdateSubLaneGroupMask", UpdateSubLaneGroupMask));
        AddBinding(new CallBinding<UITypes.WorldPosition, UITypes.ScreenPoint>("TrafficLightManager", "WorldToScreenPoint", WorldToScreenPoint));
        AddBinding(new CallBinding<UITypes.LookAtInput, int>("TrafficLightManager", "LookAt", LookAt));
        AddBinding(new CallBinding<int, int>("TrafficLightManager", "CopyPhase", CopyPhase));
        AddBinding(new CallBinding<string, string>("TrafficLightManager", "GetStorage", GetStorage));
        AddBinding(new CallBinding<UITypes.UpdateStorageInput, int>("TrafficLightManager", "UpdateStorage", UpdateStorage));
    }

    private UITypes.CityConfiguration GetCityConfiguration()
    {
        return new UITypes.CityConfiguration { leftHandTraffic = m_CityConfigurationSystem.leftHandTraffic };
    }

    private UITypes.Camera GetCamera()
    {
        var camera = m_CameraUpdateSystem.activeCamera;
        if (camera == null)
        {
            return new UITypes.Camera();
        }
        var position = camera.transform.position;
        var rotation = camera.transform.rotation;
        return new UITypes.Camera
        {
            position = new UITypes.WorldPosition { x = position.x, y = position.y, z = position.z },
            rotation = new UITypes.WorldPosition { x = rotation.eulerAngles.x, y = rotation.eulerAngles.y, z = rotation.eulerAngles.z },
        };
    }

    private Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>> GetEdgeInfo()
    {
        return m_EdgeInfoDictionary;
    }

    private UITypes.CustomPhaseItem[] GetCustomPhaseItems()
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
                    carLaneOccupied = item.m_CarLaneOccupied,
                    publicCarLaneOccupied = item.m_PublicCarLaneOccupied,
                    trackLaneOccupied = item.m_TrackLaneOccupied,
                    pedestrianLaneOccupied = item.m_PedestrianLaneOccupied,
                    weightedWaiting = item.m_WeightedWaiting,
                    priority = item.m_Priority,
                    minimumDuration = item.m_MinimumDuration,
                    maximumDuration = item.m_MaximumDuration,
                    targetDurationMultiplier = item.m_TargetDurationMultiplier,
                    carLaneOccupiedMultiplier = item.m_CarLaneOccupiedMultiplier,
                    trackLaneOccupiedMultiplier = item.m_TrackLaneOccupiedMultiplier,
                    pedestrianLaneOccupiedMultiplier = item.m_PedestrianLaneOccupiedMultiplier,
                    intervalFactor = item.m_IntervalFactor,
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
        return rs;
    }

    private UITypes.TrafficLightGroup GetTrafficLightGroup()
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
                passedCarCount = (float)math.csum(trafficLightGroup.m_PassedCarCount) / 4,
            };
        }
        else
        {
            rs = new UITypes.TrafficLightGroup { };
        }
        return rs;
    }

    private UITypes.TrafficLightMember[] GetTrafficLightsMembers()
    {
        List<UITypes.TrafficLightMember> members = [];
        if (EntityManager.TryGetBuffer<TrafficLightsMemberRef>(m_SelectedTrafficLightGroupEntity, true, out var trafficLightsMembers))
        {
            foreach (var trafficLight in trafficLightsMembers)
            {
                UITypes.WorldPosition? entityPosition = EntityManager.TryGetComponent<Node>(trafficLight.m_Entity, out var node)
                    ? new UITypes.WorldPosition
                    {
                        x = node.m_Position.x,
                        y = node.m_Position.y,
                        z = node.m_Position.z,
                    }
                    : null;
                members.Add(new UITypes.TrafficLightMember { entityIndex = trafficLight.m_Entity.Index, position = entityPosition });
            }
        }
        return members.ToArray();
    }

    private UITypes.SettingsValue GetSettings()
    {
        return new UITypes.SettingsValue
        {
            customPhaseTemplates = Mod.m_Settings.GetCustomPhaseTemplates(),
            defaultCustomPhaseTemplate = Mod.m_Settings.m_DefaultCustomPhaseTemplate,
        };
    }

    private string GetTrafficLightManagerGroupName()
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

    private UITypes.TemplateValue GetSystemDefaultTemplate()
    {
        return new UITypes.TemplateValue { template = CustomPhaseTemplate.Default };
    }

    private int SetToolState(int inputValue)
    {
        SetToolState((ToolState)inputValue);
        return 0;
    }

    private int SetDisplayPhaseIndexBindingCallback(int index)
    {
        SetDisplayPhaseIndex(index);
        return 0;
    }

    private int SetManualPhaseIndex(int inputValue)
    {
        UpdateManualPhaseIndex(inputValue);
        return 0;
    }

    private int SetCustomPhaseData(UITypes.UpdateCustomPhaseData inputValue)
    {
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
                return 0;
            }
            var newValue = customPhaseDataBuffer[index];

            switch (inputValue.key)
            {
                case UITypes.UpdateCustomPhaseType.MinimumDuration:
                    newValue.m_MinimumDuration = (ushort)inputValue.value;
                    if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                    {
                        newValue.m_MaximumDuration = newValue.m_MinimumDuration;
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.MaximumDuration:
                    newValue.m_MaximumDuration = (ushort)inputValue.value;
                    if (newValue.m_MinimumDuration > newValue.m_MaximumDuration)
                    {
                        newValue.m_MinimumDuration = newValue.m_MaximumDuration;
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.TargetDurationMultiplier:
                    newValue.m_TargetDurationMultiplier = (float)inputValue.value;
                    break;
                case UITypes.UpdateCustomPhaseType.CarLaneOccupiedMultiplier:
                    newValue.m_CarLaneOccupiedMultiplier = (float)inputValue.value;
                    break;
                case UITypes.UpdateCustomPhaseType.TrackLaneOccupiedMultiplier:
                    newValue.m_TrackLaneOccupiedMultiplier = (float)inputValue.value;
                    break;
                case UITypes.UpdateCustomPhaseType.PedestrianLaneOccupiedMultiplier:
                    newValue.m_PedestrianLaneOccupiedMultiplier = (float)inputValue.value;
                    break;
                case UITypes.UpdateCustomPhaseType.IntervalFactor:
                    newValue.m_IntervalFactor = (float)inputValue.value;
                    break;
                case UITypes.UpdateCustomPhaseType.PrioritiseTrack:
                    if ((bool)inputValue.value)
                    {
                        newValue.m_Options |= CustomPhaseData.Options.PrioritiseTrack;
                    }
                    else
                    {
                        newValue.m_Options &= ~CustomPhaseData.Options.PrioritiseTrack;
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.PrioritisePublicCar:
                    if ((bool)inputValue.value)
                    {
                        newValue.m_Options |= CustomPhaseData.Options.PrioritisePublicCar;
                    }
                    else
                    {
                        newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePublicCar;
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.PrioritisePedestrian:
                    if ((bool)inputValue.value)
                    {
                        newValue.m_Options |= CustomPhaseData.Options.PrioritisePedestrian;
                    }
                    else
                    {
                        newValue.m_Options &= ~CustomPhaseData.Options.PrioritisePedestrian;
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.LinkedWithNextPhase:
                    if ((bool)inputValue.value)
                    {
                        newValue.m_Options |= CustomPhaseData.Options.LinkedWithNextPhase;
                    }
                    else
                    {
                        newValue.m_Options &= ~CustomPhaseData.Options.LinkedWithNextPhase;
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.EndPhasePrematurely:
                    newValue.m_Options |= CustomPhaseData.Options.EndPhasePrematurely;
                    break;
                case UITypes.UpdateCustomPhaseType.Name:
                    newValue.m_Name = new FixedString64Bytes((string)inputValue.value);
                    break;
                case UITypes.UpdateCustomPhaseType.BindWithTemplate:
                    var bindValue = (UITypes.BindTemplateInput)inputValue.value;
                    if (bindValue.bindWithTemplate)
                    {
                        newValue.m_Options |= CustomPhaseData.Options.BindWithTemplate;
                        newValue.m_BindTemplate = new FixedString64Bytes(bindValue.templateName);
                    }
                    else
                    {
                        newValue.m_Options &= ~CustomPhaseData.Options.BindWithTemplate;
                        newValue.m_BindTemplate = new FixedString64Bytes();
                    }
                    break;
                case UITypes.UpdateCustomPhaseType.ApplyTemplate:
                    var templates = Mod.m_Settings.GetCustomPhaseTemplates();
                    var templateIndex = templates.FindIndex(t => t.m_Name == (string)inputValue.value);
                    if (templateIndex >= 0)
                    {
                        newValue.m_MinimumDuration = templates[templateIndex].m_MinimumDuration;
                        newValue.m_MaximumDuration = templates[templateIndex].m_MaximumDuration;
                        newValue.m_TargetDurationMultiplier = templates[templateIndex].m_TargetDurationMultiplier;
                        newValue.m_CarLaneOccupiedMultiplier = templates[templateIndex].m_CarLaneOccupiedMultiplier;
                        newValue.m_TrackLaneOccupiedMultiplier = templates[templateIndex].m_TrackLaneOccupiedMultiplier;
                        newValue.m_PedestrianLaneOccupiedMultiplier = templates[templateIndex].m_PedestrianLaneOccupiedMultiplier;
                        newValue.m_IntervalFactor = templates[templateIndex].m_IntervalFactor;
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
                    break;
                default:
                    throw new System.InvalidOperationException($"Unsupported UpdateCustomPhaseType: {inputValue.key}");
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
                        m_CarLaneOccupiedMultiplier = newValue.m_CarLaneOccupiedMultiplier,
                        m_TrackLaneOccupiedMultiplier = newValue.m_TrackLaneOccupiedMultiplier,
                        m_PedestrianLaneOccupiedMultiplier = newValue.m_PedestrianLaneOccupiedMultiplier,
                        m_IntervalFactor = newValue.m_IntervalFactor,
                        m_IsPrioritiseTrack = (newValue.m_Options & CustomPhaseData.Options.PrioritiseTrack) != 0,
                        m_IsPrioritisePublicCar = (newValue.m_Options & CustomPhaseData.Options.PrioritisePublicCar) != 0,
                        m_IsPrioritisePedestrian = (newValue.m_Options & CustomPhaseData.Options.PrioritisePedestrian) != 0,
                    }
                );
            }

            ForEachTrafficLight(UpdateEdgeInfo);
        }
        return 0;
    }

    private int SetTemplate(UITypes.TemplateValue inputValue)
    {
        Mod.m_Settings.UpdateCustomPhaseTemplate(inputValue.template);
        return 0;
    }

    private int RemoveTemplate(string templeteName)
    {
        Mod.m_Settings.RemoveCustomPhaseTemplate(templeteName);
        return 0;
    }

    private int SetTrafficLightGroupName(string name)
    {
        if (m_SelectedTrafficLightGroupEntity != Entity.Null)
        {
            m_NameSystem.SetCustomName(m_SelectedTrafficLightGroupEntity, name);
            m_TrafficLightManagerGroupNameBinding.Update();
        }
        return 0;
    }

    private int AddCustomPhase(string _)
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
        return 0;
    }

    private int RemoveCustomPhase(int inputValue)
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
        return 0;
    }

    private int SwapCustomPhase(UITypes.SwapCustomPhaseInput inputValue)
    {
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
        return 0;
    }

    private int UpdateEdgeGroupMask(UITypes.UpdateGroupMaskInput<EdgeGroupMask> inputValue)
    {
        if (m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
        {
            return 0;
        }

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
        return 0;
    }

    private int UpdateSubLaneGroupMask(UITypes.UpdateGroupMaskInput<SubLaneGroupMask> inputValue)
    {
        if (m_SelectedTrafficLightGroupEntity.Equals(Entity.Null))
        {
            return 0;
        }

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
        return 0;
    }

    private UITypes.ScreenPoint WorldToScreenPoint(UITypes.WorldPosition inputValue)
    {
        var camera = m_CameraUpdateSystem.activeCamera;
        if (camera == null)
        {
            return new UITypes.ScreenPoint(0, 0);
        }
        var screenPoint = camera.WorldToScreenPoint(new Vector3(inputValue.x, inputValue.y, inputValue.z));
        return new UITypes.ScreenPoint(screenPoint, Screen.height);
    }

    private int LookAt(UITypes.LookAtInput inputValue)
    {
        var pivot = (float3)inputValue.position;
        var zoom = inputValue.distance;

        var cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
        cameraUpdateSystem.activeCameraController.pivot = pivot;
        cameraUpdateSystem.activeCameraController.zoom = zoom;
        return 0;
    }

    private int CopyPhase(int sourceIndex)
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

    private string GetStorage(string inputValue)
    {
        return Mod.m_Settings.GetStorage(inputValue);
    }

    private int UpdateStorage(UITypes.UpdateStorageInput inputValue)
    {
        Mod.m_Settings.UpdateStorage(inputValue.key, inputValue.value);
        return 0;
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
