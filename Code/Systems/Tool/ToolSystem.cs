using System.Linq;
using Colossal.Entities;
using Game.Common;
using Game.Net;
using Game.Notifications;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;
using Game.UI.Localization;
using Game.UI.Tooltip;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Overlay;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using static TrafficLightManager.Code.Systems.UI.UISystem;

namespace TrafficLightManager.Code.Systems.Tool;

public partial class ToolSystem : ToolBaseSystem
{
    public override string toolID => "TrafficLightManager Tool";

    private UI.TooltipSystem m_TooltipSystem;

    private UI.UISystem m_UISystem;

    private StringTooltip m_ConfigureTooltip;

    private StringTooltip m_CreateGroupTooltip;

    private StringTooltip m_AddMemberTooltip;

    private StringTooltip m_RemoveMemberTooltip;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_TooltipSystem = World.GetOrCreateSystemManaged<UI.TooltipSystem>();
        m_UISystem = World.GetOrCreateSystemManaged<UI.UISystem>();
        m_ToolSystem.EventToolChanged += ToolChanged;

        m_ConfigureTooltip = new StringTooltip
        {
            path = "TrafficLightManager.Code.Tooltips.Configure",
            icon = "Media/Mouse/LMB.svg",
            value = LocalizedString.Id("Configure"),
        };
        m_CreateGroupTooltip = new StringTooltip
        {
            path = "CreateGroup",
            icon = "Media/Mouse/LMB.svg",
            value = LocalizedString.Id("CreateGroup"),
        };
        m_AddMemberTooltip = new StringTooltip
        {
            path = "TrafficLightManager.Code.Tooltips.AddMember",
            icon = "Media/Mouse/LMB.svg",
            value = LocalizedString.Id("AddMember"),
        };
        m_RemoveMemberTooltip = new StringTooltip
        {
            path = "TrafficLightManager.Code.Tooltips.RemoveMember",
            icon = "Media/Mouse/RMB.svg",
            value = LocalizedString.Id("RemoveMember"),
        };
    }

    protected override void OnStartRunning()
    {
        applyAction.shouldBeEnabled = true;
        secondaryApplyAction.shouldBeEnabled = true;
        requireUnderground = false;
    }

    protected override void OnStopRunning()
    {
        applyAction.shouldBeEnabled = false;
        secondaryApplyAction.shouldBeEnabled = false;
    }

    protected override bool GetAllowApply()
    {
        return true;
    }

    public override PrefabBase GetPrefab()
    {
        return null;
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        return false;
    }

    private bool m_Underground;

    public override bool allowUnderground => true;

    public override void SetUnderground(bool isUnderground)
    {
        m_Underground = isUnderground;
    }

    public override void ElevationUp()
    {
        m_Underground = false;
    }

    public override void ElevationDown()
    {
        m_Underground = true;
    }

    public override void ElevationScroll()
    {
        m_Underground = !m_Underground;
    }

    public override void InitializeRaycast()
    {
        if (m_Underground)
        {
            m_ToolRaycastSystem.collisionMask = CollisionMask.Underground;
        }
        else
        {
            m_ToolRaycastSystem.collisionMask = (CollisionMask.OnGround | CollisionMask.Overground);
        }
        m_ToolRaycastSystem.typeMask = (TypeMask.Net);
        m_ToolRaycastSystem.raycastFlags = RaycastFlags.SubElements | RaycastFlags.Cargo | RaycastFlags.Passenger | RaycastFlags.EditorContainers;
        m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.TrainTrack | Layer.TramTrack | Layer.SubwayTrack | Layer.PublicTransportRoad | Layer.Pathway;
        m_ToolRaycastSystem.iconLayerMask = IconLayerMask.None;
        m_ToolRaycastSystem.utilityTypeMask = UtilityTypes.None;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        requireUnderground = m_Underground;

        base.applyAction.shouldBeEnabled = m_UISystem.GetToolState() != ToolState.Editing;
        base.secondaryApplyAction.shouldBeEnabled = m_UISystem.GetToolState() != ToolState.Editing;
        bool raycastFlag = GetRaycastResult(out Entity entity, out RaycastHit hit);
        if (m_UISystem.GetToolState() != ToolState.Editing)
        {
            if (secondaryApplyAction.WasReleasedThisFrame())
            {
                if (IsValidEntity(entity))
                {
                    if (m_UISystem.GetToolState() == UI.UISystem.ToolState.RemoveTrafficLights)
                    {
                        m_UISystem.RemoveTrafficLights(entity);
                    }
                }
            }
            if (applyAction.WasReleasedThisFrame())
            {
                if (IsValidEntity(entity))
                {
                    switch (m_UISystem.GetToolState())
                    {
                        case UI.UISystem.ToolState.AddTrafficLights:
                            m_UISystem.AddTrafficLights(entity);
                            break;
                        case UI.UISystem.ToolState.ChooseGroup:
                        case UI.UISystem.ToolState.Choosed:
                            m_UISystem.ChangeSelectedTrafficLightGroupEntity(entity);
                            break;
                    }
                }
            }

            if (IsValidEntity(entity))
            {
                if (
                    new[] { ToolState.ChooseGroup, ToolState.Choosed }.Contains(m_UISystem.GetToolState())
                    && EntityManager.TryGetComponent<CustomTrafficLights>(entity, out var customTrafficLights)
                    && EntityManager.TryGetBuffer<TrafficLightsMemberRef>(customTrafficLights.m_TrafficLightGroupEntity, true, out var trafficLightsMemberRefs)
                )
                {
                    foreach (var item in trafficLightsMemberRefs)
                    {
                        m_UISystem.DrawNodeOutline(item.m_Entity, Color.white, 1.0f, 0.0f);
                    }
                }
                else
                {
                    m_UISystem.DrawNodeOutline(entity, Color.white, 1.0f, 0.0f);
                }
            }
        }
        UpdateTooltip(entity);
        return inputDeps;
    }

    protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, Game.GameMode mode)
    {
        Mod.m_Log.Info($"Searching for traffic light prefab asset entities");
        EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<PlaceableNetData>());
        NativeArray<Entity> entityArray = query.ToEntityArray(Allocator.Temp);
        NativeArray<PlaceableNetData> placeableNetDataArray = query.ToComponentDataArray<PlaceableNetData>(Allocator.Temp);
        for (int i = 0; i < entityArray.Length; i++)
        {
            if ((placeableNetDataArray[i].m_SetUpgradeFlags.m_General & CompositionFlags.General.TrafficLights) == 0)
            {
                continue;
            }
        }
    }

    private void UpdateTooltip(Entity entity)
    {
        m_TooltipSystem.m_TooltipList.Clear();
        if (IsValidEntity(entity))
        {
            switch (m_UISystem.GetToolState())
            {
                case UI.UISystem.ToolState.ChooseGroup:
                case UI.UISystem.ToolState.Choosed:
                    if (EntityManager.HasComponent<CustomTrafficLights>(entity))
                    {
                        m_TooltipSystem.m_TooltipList.Add(m_ConfigureTooltip);
                    }
                    else
                    {
                        m_TooltipSystem.m_TooltipList.Add(m_CreateGroupTooltip);
                    }
                    break;
                case UI.UISystem.ToolState.AddTrafficLights:
                    m_TooltipSystem.m_TooltipList.Add(m_AddMemberTooltip);
                    break;
                case UI.UISystem.ToolState.RemoveTrafficLights:
                    m_TooltipSystem.m_TooltipList.Add(m_RemoveMemberTooltip);
                    break;
            }
        }
    }

    public bool IsValidEntity(Entity entity)
    {
        if (new[] { ToolState.Editing, ToolState.Disabled }.Contains(m_UISystem.GetToolState()))
        {
            return false;
        }

        if (entity == Entity.Null)
        {
            return false;
        }

        if (EntityManager.TryGetComponent<TrafficLights>(entity, out var trafficLights))
        {
            if ((trafficLights.m_Flags & TrafficLightFlags.MoveableBridge) != 0)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        if (new[] { ToolState.AddTrafficLights }.Contains(m_UISystem.GetToolState()))
        {
            if (EntityManager.HasComponent<CustomTrafficLights>(entity))
            {
                return false;
            }
        }

        if (new[] { ToolState.RemoveTrafficLights }.Contains(m_UISystem.GetToolState()))
        {
            if (EntityManager.TryGetComponent<CustomTrafficLights>(entity, out var customTrafficLights))
            {
                if (customTrafficLights.m_TrafficLightGroupEntity != m_UISystem.m_SelectedTrafficLightGroupEntity)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void Enable()
    {
        m_ToolSystem.activeTool = this;
        m_TooltipSystem.m_TooltipList.Clear();
        m_TooltipSystem.Enabled = true;
    }

    public void Disable()
    {
        if (m_ToolSystem.activeTool == this)
        {
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }
        m_TooltipSystem.m_TooltipList.Clear();
        m_TooltipSystem.Enabled = false;
    }

    private void ToolChanged(ToolBaseSystem system)
    {
        if (system != this)
        {
            m_UISystem.SetToolState(UI.UISystem.ToolState.Disabled);
        }
    }
}
