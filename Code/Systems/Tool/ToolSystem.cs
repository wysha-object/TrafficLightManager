using System.Linq;
using System.Reflection;
using Colossal.Entities;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.UI.Localization;
using Game.UI.Tooltip;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Overlay;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using static TrafficLightManager.Code.Systems.UI.UISystem;

namespace TrafficLightManager.Code.Systems.Tool;

public partial class ToolSystem : NetToolSystem
{
    public override string toolID => "TrafficLightManager Tool";

    private RenderSystem m_RenderSystem;

    private UI.TooltipSystem m_TooltipSystem;

    private UI.UISystem m_UISystem;

    private NativeList<ControlPoint> m_ParentControlPoints;

    private NativeReference<AppliedUpgrade> m_ParentAppliedUpgrade;

    private Entity m_PrefabEntity = Entity.Null;

    private Entity m_RaycastResult = Entity.Null;

    public bool m_Suspended { get; private set; }

    private PropertyInfo m_DisplayOverridePropertyInfo;

    private StringTooltip m_ChooseGroupTooltip;

    private StringTooltip m_AddMemberTooltip;

    private StringTooltip m_RemoveMemberTooltip;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_RenderSystem = World.GetOrCreateSystemManaged<RenderSystem>();
        m_TooltipSystem = World.GetOrCreateSystemManaged<UI.TooltipSystem>();
        m_UISystem = World.GetOrCreateSystemManaged<UI.UISystem>();
        m_ParentControlPoints = GetControlPoints(out JobHandle _);
        m_ParentAppliedUpgrade = (NativeReference<AppliedUpgrade>)typeof(NetToolSystem).GetField("m_AppliedUpgrade", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
        m_DisplayOverridePropertyInfo = typeof(Game.Input.ProxyAction).GetProperty("displayOverride");
        m_ToolSystem.EventToolChanged += ToolChanged;

        m_ChooseGroupTooltip = new StringTooltip
        {
            path = "TrafficLightManager.Code.Tooltips.Configure",
            icon = "Media/Mouse/LMB.svg",
            value = LocalizedString.Id("TrafficLightManager.Code.Tooltips.Configure"),
        };
        m_AddMemberTooltip = new StringTooltip
        {
            path = "TrafficLightManager.Code.Tooltips.AddMember",
            icon = "Media/Mouse/LMB.svg",
            value = LocalizedString.Id("TrafficLightManager.Code.Tooltips.AddMember"),
        };
        m_RemoveMemberTooltip = new StringTooltip
        {
            path = "TrafficLightManager.Code.Tooltips.RemoveMember",
            icon = "Media/Mouse/RMB.svg",
            value = LocalizedString.Id("TrafficLightManager.Code.Tooltips.RemoveMember"),
        };
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (m_Suspended)
        {
            m_ToolRaycastSystem.raycastFlags |= Game.Common.RaycastFlags.UIDisable;
        }
        var result = base.OnUpdate(inputDeps);
        base.applyAction.shouldBeEnabled = !m_Suspended;
        base.secondaryApplyAction.shouldBeEnabled = !m_Suspended;
        if ((m_ToolRaycastSystem.raycastFlags & Game.Common.RaycastFlags.UIDisable) == 0)
        {
            if (secondaryApplyAction.WasReleasedThisFrame())
            {
                if (m_RaycastResult != Entity.Null)
                {
                    if (IsValidEntity(m_RaycastResult))
                    {
                        if (m_UISystem.GetToolState() == UI.UISystem.ToolState.RemoveTrafficLights)
                        {
                            m_UISystem.RemoveTrafficLights(m_RaycastResult);
                        }
                        UpdateTooltip(m_RaycastResult);
                    }
                }
            }
            if (m_ParentControlPoints.Length >= 4)
            {
                Entity originalEntity = m_ParentControlPoints[m_ParentControlPoints.Length - 3].m_OriginalEntity;
                if (originalEntity != m_RaycastResult)
                {
                    m_RaycastResult = originalEntity;
                    m_RenderSystem.ClearLineMesh();
                    if (IsValidEntity(m_RaycastResult) && EntityManager.TryGetComponent<NodeGeometry>(m_RaycastResult, out var nodeGeometry))
                    {
                        m_RenderSystem.AddBounds(nodeGeometry.m_Bounds, new UnityEngine.Color(0.5f, 1.0f, 2.0f, 1.0f), 0.5f);
                        m_RenderSystem.BuildLineMesh();
                    }
                    UpdateTooltip(m_RaycastResult);
                }
            }
            else if (m_RaycastResult != Entity.Null)
            {
                m_RaycastResult = Entity.Null;
                m_RenderSystem.ClearLineMesh();
                UpdateTooltip(m_RaycastResult);
            }
            if (applyAction.WasReleasedThisFrame())
            {
                Entity entity = m_ParentAppliedUpgrade.Value.m_Entity;
                CompositionFlags flags = m_ParentAppliedUpgrade.Value.m_Flags;
                if (entity != Entity.Null && (flags.m_General & CompositionFlags.General.TrafficLights) != 0 && IsValidEntity(entity))
                {
                    switch (m_UISystem.GetToolState())
                    {
                        case UI.UISystem.ToolState.AddTrafficLights:
                            m_UISystem.AddTrafficLights(entity);
                            break;
                        case UI.UISystem.ToolState.ChooseGroup:
                            m_UISystem.ChangeSelectedTrafficLightGroupEntity(entity);
                            break;
                    }
                }
            }
            DisableActionTooltips();
        }
        return result;
    }

    protected override void OnGameLoadingComplete(Colossal.Serialization.Entities.Purpose purpose, Game.GameMode mode)
    {
        Mod.m_Log.Info($"Searching for traffic light prefab asset entities");
        m_PrefabEntity = Entity.Null;
        EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<PlaceableNetData>());
        NativeArray<Entity> entityArray = query.ToEntityArray(Allocator.Temp);
        NativeArray<PlaceableNetData> placeableNetDataArray = query.ToComponentDataArray<PlaceableNetData>(Allocator.Temp);
        for (int i = 0; i < entityArray.Length; i++)
        {
            if ((placeableNetDataArray[i].m_SetUpgradeFlags.m_General & CompositionFlags.General.TrafficLights) == 0)
            {
                continue;
            }
            if (m_PrefabSystem.TryGetPrefab(entityArray[i], out PrefabBase prefabBase) && prefabBase is NetPrefab)
            {
                m_PrefabEntity = entityArray[i];
                Mod.m_Log.Info($"{m_PrefabEntity} prefabBase.uiTag: {prefabBase.uiTag}");
            }
        }
        Mod.Assert(m_PrefabEntity != Entity.Null, "Traffic lights prefab asset entity not found. The tool system will not work.");
    }

    protected override bool GetAllowApply()
    {
        if (m_RaycastResult != Entity.Null && !EntityManager.HasComponent<CustomTrafficLights>(m_RaycastResult))
        {
            return true;
        }
        return !(secondaryApplyAction.WasReleasedThisFrame() || secondaryApplyAction.IsPressed());
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        return false;
    }

    public override PrefabBase GetPrefab()
    {
        return null;
    }

    private void UpdateTooltip(Entity entity)
    {
        m_TooltipSystem.m_TooltipList.Clear();
        if (IsValidEntity(entity))
        {
            switch (m_UISystem.GetToolState())
            {
                case UI.UISystem.ToolState.ChooseGroup:
                    m_TooltipSystem.m_TooltipList.Add(m_ChooseGroupTooltip);
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

    private void DisableActionTooltips()
    {
        if (applyAction is Game.Input.UIInputAction.State applyActionState)
        {
            m_DisplayOverridePropertyInfo.SetValue(applyActionState.action, null);
        }
        if (secondaryApplyAction is Game.Input.UIInputAction.State secondaryApplyActionState)
        {
            m_DisplayOverridePropertyInfo.SetValue(secondaryApplyActionState.action, null);
        }
    }

    public bool IsValidEntity(Entity entity)
    {
        if (new[] { ToolState.Choosed, ToolState.Disabled }.Contains(m_UISystem.GetToolState()))
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
        if (m_PrefabSystem.TryGetPrefab(m_PrefabEntity, out NetPrefab netPrefab))
        {
            this.prefab = netPrefab;
            this.underground = m_ToolSystem.activeTool.requireUnderground;
            m_Suspended = false;
            m_ToolSystem.activeTool = this;
            m_TooltipSystem.m_TooltipList.Clear();
            m_TooltipSystem.Enabled = true;
        }
    }

    public void Suspend()
    {
        m_Suspended = true;
        m_TooltipSystem.m_TooltipList.Clear();
        m_TooltipSystem.Enabled = false;
    }

    public void Disable()
    {
        if (m_ToolSystem.activeTool == this)
        {
            m_ToolSystem.activeTool = m_DefaultToolSystem;
            m_TooltipSystem.m_TooltipList.Clear();
            m_TooltipSystem.Enabled = false;
        }
    }

    private void ToolChanged(ToolBaseSystem system)
    {
        if (system != this)
        {
            m_UISystem.SetToolState(UI.UISystem.ToolState.Disabled);
        }
    }
}
