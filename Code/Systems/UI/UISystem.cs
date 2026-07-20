using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Colossal.Entities;
using Game;
using Game.Net;
using Game.Rendering;
using Game.SceneFlow;
using Game.UI;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Overlay;
using TrafficLightManager.Code.Systems.Update;
using TrafficLightManager.Code.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static TrafficLightManager.Code.Components.CustomTrafficLights;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    public enum ToolState
    {
        Disabled = 0,
        ChooseGroup = 1,
        Choosed = 2,
        AddTrafficLights = 3,
        RemoveTrafficLights = 4,
        Editing = 5,
    }

    public Entity m_SelectedTrafficLightGroupEntity { get; private set; }

    private Game.City.CityConfigurationSystem m_CityConfigurationSystem;

    private RenderSystem m_RenderSystem;

    private Tool.ToolSystem m_ToolSystem;

    private Update.ModificationUpdateSystem m_ModificationUpdateSystem;

    private SimulationUpdateSystem m_SimulationUpdateSystem;

    private NameSystem m_NameSystem;

    private Camera m_Camera;

    private int m_ScreenHeight;

    private CameraUpdateSystem m_CameraUpdateSystem;

    private float3 m_CameraPosition;

    private List<UITypes.WorldPosition> m_WorldPositionList;

    private Dictionary<Entity, NativeArray<NodeUtils.EdgeInfo>> m_EdgeInfoDictionary;

    private UITypes.ScreenPoint m_MainPanelPosition;

    public TypeHandle m_TypeHandle;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_TypeHandle.AssignHandles(ref base.CheckedStateRef);

        m_Camera = Camera.main;
        m_ScreenHeight = Screen.height;
        m_MainPanelPosition = new(-999999, -999999);

        m_WorldPositionList = [];
        m_EdgeInfoDictionary = [];

        m_CameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
        m_CityConfigurationSystem = World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>();
        m_RenderSystem = World.GetOrCreateSystemManaged<RenderSystem>();
        m_ToolSystem = World.GetOrCreateSystemManaged<Tool.ToolSystem>();
        m_ModificationUpdateSystem = World.GetOrCreateSystemManaged<Update.ModificationUpdateSystem>();
        m_SimulationUpdateSystem = World.GetOrCreateSystemManaged<SimulationUpdateSystem>();
        m_NameSystem = World.GetOrCreateSystemManaged<NameSystem>();

        AddUIBindings();
        SetupKeyBindings();
        UpdateLocale();

        GameManager.instance.localizationManager.onActiveDictionaryChanged += UpdateLocale;
    }

    protected override void OnUpdate()
    {
        if (m_WorldPositionList.Count > 0 && !m_CameraPosition.Equals(m_CameraUpdateSystem.position))
        {
            m_CameraPosition = m_CameraUpdateSystem.position;
            m_ScreenPointBinding.Update();
        }
    }

    protected override void OnDestroy()
    {
        ClearEdgeInfo();
    }

    protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
    {
        m_CityConfigurationBinding.Update();
    }

    public void SimulationUpdate()
    {
        m_TrafficLightGroupBinding.Update();
    }

    public void ModificationUpdate()
    {
        DrawIcon();
        var overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
        var overlayBuffer = overlayRenderSystem.GetBuffer(out JobHandle dependencies);
        dependencies.Complete();
        DrawGizmo(overlayBuffer);
        DrawEdge(overlayBuffer);
        DrawTrafficLightGroupName(overlayBuffer);
        m_CustomPhaseItemsBinding.Update();
    }

    public static string GetLocaleCode()
    {
        string locale = Utils.LocalisationUtils.GetAutoLocale(GameManager.instance.localizationManager.activeLocaleId, CultureInfo.CurrentCulture.Name);
        if (Mod.m_Settings != null && Mod.m_Settings.m_Locale != "auto")
        {
            locale = Mod.m_Settings.m_Locale;
        }
        return locale;
    }

    public static void UpdateLocale()
    {
        LocalisationUtils localisationsHelper = new LocalisationUtils(GetLocaleCode());
        localisationsHelper.AddToDictionary(GameManager.instance.localizationManager.activeDictionary);
        localisationsHelper.UpdateActiveDictionary();

        if (m_LocalisationBinding != null)
        {
            m_LocalisationBinding.Update();
        }
    }

    public void ForEachTrafficLight(Action<Entity> action)
    {
        if (m_TrafficLightGroupBinding != null)
        {
            if (EntityManager.TryGetBuffer(m_SelectedTrafficLightGroupEntity, true, out DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefs))
            {
                for (int i = 0; i < trafficLightsMemberRefs.Length; i++)
                {
                    action(trafficLightsMemberRefs[i].m_Entity);
                }
            }
        }
    }

    public void UpdateEdgeInfo(Entity node)
    {
        if (node == Entity.Null)
        {
            return;
        }
        if (m_EdgeInfoDictionary.ContainsKey(node))
        {
            NodeUtils.Dispose(m_EdgeInfoDictionary[node]);
        }
        m_EdgeInfoDictionary[node] = NodeUtils.GetEdgeInfoList(Allocator.Persistent, node, this).AsArray();
        m_EdgeInfoBinding.Update();
    }

    public void ClearEdgeInfo()
    {
        foreach (var kV in m_EdgeInfoDictionary)
        {
            NodeUtils.Dispose(kV.Value);
        }
        m_EdgeInfoDictionary.Clear();
    }

    public void AddTrafficLights(Entity entity)
    {
        if (EntityManager.HasComponent<CustomTrafficLights>(entity))
        {
            return;
        }
        if (m_SelectedTrafficLightGroupEntity == Entity.Null)
        {
            return;
        }

        if (EntityManager.TryGetBuffer<TrafficLightsMemberRef>(m_SelectedTrafficLightGroupEntity, false, out var trafficLightsMemberRefs))
        {
            trafficLightsMemberRefs.Add(new TrafficLightsMemberRef { m_Entity = entity });
            EntityManager.AddComponentData(entity, new CustomTrafficLights(Patterns.CustomPhase, m_SelectedTrafficLightGroupEntity));
            EntityManager.AddComponent<SubLane>(entity);
            EntityManager.AddComponent<ConnectedEdge>(entity);
            EntityManager.AddComponent<EdgeGroupMask>(entity);
            EntityManager.AddComponent<SubLaneGroupMask>(entity);
        }

        m_TrafficLightsMembersBinding.Update();
        SetToolState(ToolState.Choosed);
        AddUpdate();
    }

    public void RemoveTrafficLights(Entity entity)
    {
        if (EntityManager.TryGetComponent<CustomTrafficLights>(entity, out var customTrafficLights))
        {
            if (
                customTrafficLights.m_TrafficLightGroupEntity != Entity.Null
                && EntityManager.TryGetBuffer<TrafficLightsMemberRef>(customTrafficLights.m_TrafficLightGroupEntity, false, out var trafficLightsMemberRefs)
            )
            {
                for (int i = 0; i < trafficLightsMemberRefs.Length; i++)
                {
                    var item = trafficLightsMemberRefs[i];
                    if (item.m_Entity == entity)
                    {
                        trafficLightsMemberRefs.RemoveAt(i);
                        break;
                    }
                }
                if (trafficLightsMemberRefs.Length == 0)
                {
                    if (customTrafficLights.m_TrafficLightGroupEntity == m_SelectedTrafficLightGroupEntity)
                    {
                        SetSelectedTrafficLightGroupEntity(Entity.Null);
                    }
                    EntityManager.DestroyEntity(customTrafficLights.m_TrafficLightGroupEntity);
                }
            }
            EntityManager.RemoveComponent<CustomTrafficLights>(entity);
        }
        EntityManager.AddComponentData(entity, default(Game.Common.Updated));

        m_TrafficLightsMembersBinding.Update();
        SetToolState(ToolState.Choosed);
        AddUpdate();
    }

    public void ChangeSelectedTrafficLightGroupEntity(Entity trafficLightsEntity)
    {
        if (trafficLightsEntity == Entity.Null)
        {
            return;
        }

        if (!EntityManager.TryGetComponent(trafficLightsEntity, out CustomTrafficLights customTrafficLights) || customTrafficLights.m_TrafficLightGroupEntity == Entity.Null)
        {
            Entity groupEntity = EntityManager.CreateEntity(typeof(TrafficLightGroup), typeof(TrafficLightsMemberRef), typeof(CustomPhaseData));
            EntityManager.SetComponentData(groupEntity, new TrafficLightGroup());
            DynamicBuffer<TrafficLightsMemberRef> memberRefs = EntityManager.GetBuffer<TrafficLightsMemberRef>(groupEntity, false);
            memberRefs.Add(new TrafficLightsMemberRef { m_Entity = trafficLightsEntity });

            customTrafficLights = new CustomTrafficLights(Patterns.CustomPhase, groupEntity);
            EntityManager.AddComponentData(trafficLightsEntity, customTrafficLights);
            EntityManager.AddComponent<SubLane>(trafficLightsEntity);
            EntityManager.AddComponent<ConnectedEdge>(trafficLightsEntity);
            EntityManager.AddComponent<EdgeGroupMask>(trafficLightsEntity);
            EntityManager.AddComponent<SubLaneGroupMask>(trafficLightsEntity);
        }

        if (m_SelectedTrafficLightGroupEntity != Entity.Null)
        {
            EntityManager.TryGetComponent(m_SelectedTrafficLightGroupEntity, out TrafficLightGroup trafficLightGroup);
            trafficLightGroup.m_ManualSignalGroup = 0;
            EntityManager.SetComponentData(m_SelectedTrafficLightGroupEntity, trafficLightGroup);
        }

        Entity trafficLightGroupEntity = customTrafficLights.m_TrafficLightGroupEntity;
        DynamicBuffer<TrafficLightsMemberRef> trafficLightsMemberRefs = EntityManager.GetBuffer<TrafficLightsMemberRef>(trafficLightGroupEntity, true);
        Entity[] trafficLightsEntites = new Entity[trafficLightsMemberRefs.Length];
        for (int i = 0; i < trafficLightsMemberRefs.Length; i++)
        {
            trafficLightsEntites[i] = trafficLightsMemberRefs[i].m_Entity;
        }

        if (trafficLightGroupEntity != m_SelectedTrafficLightGroupEntity)
        {
            SetSelectedTrafficLightGroupEntity(trafficLightGroupEntity);
        }

        SetToolState(ToolState.Choosed);
        AddUpdate();
        m_TrafficLightManagerGroupNameBinding.Update();
    }

    public void SetSelectedTrafficLightGroupEntity(Entity entity)
    {
        m_SelectedTrafficLightGroupEntity = entity;
        m_CustomPhaseItemsBinding.Update();
        m_TrafficLightGroupBinding.Update();
        m_TrafficLightsMembersBinding.Update();
    }

    public ToolState GetToolState()
    {
        return (ToolState)m_ToolStateBinding.value;
    }

    public void SetToolState(ToolState toolState)
    {
        UpdateManualPhaseIndex(-1);
        if (!new[] { ToolState.Disabled, ToolState.ChooseGroup }.Contains(toolState) && m_SelectedTrafficLightGroupEntity == Entity.Null)
        {
            toolState = ToolState.Disabled;
        }
        switch (toolState)
        {
            case ToolState.Disabled:
                m_SelectedTrafficLightGroupEntity = Entity.Null;
                m_EditingPhaseIndexBinding.Update(-1);
                m_ToolSystem.Disable();
                break;
            case ToolState.ChooseGroup:
            case ToolState.AddTrafficLights:
            case ToolState.RemoveTrafficLights:
            case ToolState.Choosed:
                m_EditingPhaseIndexBinding.Update(-1);
                m_ToolSystem.Enable();
                break;
            case ToolState.Editing:
                m_ToolSystem.Enable();
                break;
        }
        m_ToolStateBinding.Update((int)toolState);

        ClearEdgeInfo();
        ForEachTrafficLight(UpdateEdgeInfo);

        m_DisplayPhaseIndexBinding.Update(-1);
    }

    public void AddUpdate()
    {
        ForEachTrafficLight(
            (e) =>
            {
                EntityManager.AddComponentData(e, default(Game.Common.Updated));
            }
        );
    }
}
