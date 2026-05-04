using System.Linq;
using Colossal.Entities;
using Game.Net;
using Game.UI;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Overlay;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    public void RedrawGizmo()
    {
        m_RenderSystem.ClearLineMesh("gizmo");
        if (Mod.m_Settings.m_DisplayCurrentPhase)
        {
            if (GetToolState() != ToolState.Disabled || Mod.m_Settings.m_DisplayCurrentPhaseWhenToolDisabled)
            {
                var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CustomTrafficLights>());
                var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
                var customTrafficLightsArray = entityQuery.ToComponentDataArray<CustomTrafficLights>(Allocator.Temp);
                var TrafficLightsArray = entityQuery.ToComponentDataArray<TrafficLights>(Allocator.Temp);
                for (int i = 0; i < entityArray.Length; i++)
                {
                    var entity = entityArray[i];
                    var customTrafficLights = customTrafficLightsArray[i];
                    var trafficLights = TrafficLightsArray[i];
                    int displayIndex = trafficLights.m_CurrentSignalGroup - 1;
                    if (
                        customTrafficLights.m_TrafficLightGroupEntity == m_SelectedTrafficLightGroupEntity
                        && (m_DisplayPhaseIndexBinding.value < 0 || m_DisplayPhaseIndexBinding.value == displayIndex)
                    )
                    {
                        continue;
                    }
                    if (EntityManager.TryGetBuffer<SubLane>(entity, true, out var subLaneBuffer))
                    {
                        foreach (var subLane in subLaneBuffer)
                        {
                            Entity subLaneEntity = subLane.m_SubLane;
                            bool isPedestrian = EntityManager.TryGetComponent<PedestrianLane>(subLaneEntity, out var pedestrianLane);
                            if (EntityManager.HasComponent<MasterLane>(subLaneEntity))
                            {
                                continue;
                            }
                            if (!EntityManager.HasComponent<CarLane>(subLaneEntity) && !EntityManager.HasComponent<TrackLane>(subLaneEntity) && !isPedestrian)
                            {
                                continue;
                            }
                            if (isPedestrian && (pedestrianLane.m_Flags & PedestrianLaneFlags.Crosswalk) == 0)
                            {
                                continue;
                            }
                            if (EntityManager.TryGetComponent<LaneSignal>(subLaneEntity, out var laneSignal) && EntityManager.TryGetComponent<Curve>(subLaneEntity, out var curve))
                            {
                                Color color = Color.green;
                                if (
                                    EntityManager.TryGetComponent<ExtraLaneSignal>(subLaneEntity, out var extraLaneSignal)
                                    && (extraLaneSignal.m_YieldGroupMask & 1 << displayIndex) != 0
                                )
                                {
                                    color = Color.blue;
                                }
                                color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, color.a * 0.3f);
                                if ((laneSignal.m_GroupMask & 1 << displayIndex) != 0)
                                {
                                    m_RenderSystem.AddBezier("gizmo", curve.m_Bezier, color, curve.m_Length, 1f);
                                }
                            }
                        }
                    }
                }
            }
        }
        ForEachTrafficLight(
            (e) =>
            {
                if (EntityManager.TryGetBuffer<SubLane>(e, true, out var subLaneBuffer))
                {
                    int displayIndex = 16;
                    if (EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup) && trafficLightGroup.m_ManualSignalGroup > 0)
                    {
                        displayIndex = trafficLightGroup.m_ManualSignalGroup - 1;
                    }
                    else if (m_DisplayPhaseIndexBinding.value >= 0)
                    {
                        displayIndex = m_DisplayPhaseIndexBinding.value;
                    }
                    else if (EntityManager.TryGetComponent<TrafficLights>(e, out var trafficLights))
                    {
                        displayIndex = trafficLights.m_CurrentSignalGroup - 1;
                    }
                    foreach (var subLane in subLaneBuffer)
                    {
                        Entity subLaneEntity = subLane.m_SubLane;
                        bool isPedestrian = EntityManager.TryGetComponent<PedestrianLane>(subLaneEntity, out var pedestrianLane);
                        if (EntityManager.HasComponent<MasterLane>(subLaneEntity))
                        {
                            continue;
                        }
                        if (!EntityManager.HasComponent<CarLane>(subLaneEntity) && !EntityManager.HasComponent<TrackLane>(subLaneEntity) && !isPedestrian)
                        {
                            continue;
                        }
                        if (isPedestrian && (pedestrianLane.m_Flags & PedestrianLaneFlags.Crosswalk) == 0)
                        {
                            continue;
                        }
                        if (EntityManager.TryGetComponent<LaneSignal>(subLaneEntity, out var laneSignal) && EntityManager.TryGetComponent<Curve>(subLaneEntity, out var curve))
                        {
                            Color color = Color.green;
                            if (
                                EntityManager.TryGetComponent<ExtraLaneSignal>(subLaneEntity, out var extraLaneSignal)
                                && (extraLaneSignal.m_YieldGroupMask & 1 << displayIndex) != 0
                            )
                            {
                                color = Color.blue;
                            }
                            if ((laneSignal.m_GroupMask & 1 << displayIndex) != 0)
                            {
                                m_RenderSystem.AddBezier("gizmo", curve.m_Bezier, color, curve.m_Length, 0.25f);
                            }
                        }
                    }
                }
            }
        );
        m_RenderSystem.BuildLineMesh();
    }

    public void RedrawIcon()
    {
        m_RenderSystem.ClearIconList();
        if (GetToolState() == ToolState.ChooseGroup)
        {
            var trafficLightsNodeArray = EntityManager
                .CreateEntityQuery(new EntityQueryDesc { All = [ComponentType.ReadOnly<TrafficLights>()], None = [ComponentType.ReadOnly<CustomTrafficLights>()] })
                .ToComponentDataArray<Node>(Allocator.Temp);
            var customTrafficLightsNodeArray = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CustomTrafficLights>()).ToComponentDataArray<Node>(Allocator.Temp);

            foreach (var item in trafficLightsNodeArray)
            {
                m_RenderSystem.AddIcon(item.m_Position, RenderSystem.Icon.TrafficLight);
            }
            foreach (var item in customTrafficLightsNodeArray)
            {
                m_RenderSystem.AddIcon(item.m_Position, RenderSystem.Icon.TrafficLightWrench);
            }
        }
        else if (GetToolState() == ToolState.AddTrafficLights)
        {
            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CustomTrafficLights>());
            var customTrafficLightsNodeArray = entityQuery.ToComponentDataArray<Node>(Allocator.Temp);
            var customTrafficLightsArray = entityQuery.ToComponentDataArray<CustomTrafficLights>(Allocator.Temp);
            for (int i = 0; i < customTrafficLightsNodeArray.Length; i++)
            {
                Node node = customTrafficLightsNodeArray[i];
                CustomTrafficLights customTrafficLights = customTrafficLightsArray[i];
                if (customTrafficLights.m_TrafficLightGroupEntity == m_SelectedTrafficLightGroupEntity)
                {
                    m_RenderSystem.AddIcon(node.m_Position, RenderSystem.Icon.TrafficLightLink);
                }
            }

            var trafficLightsNodeArray = EntityManager
                .CreateEntityQuery(new EntityQueryDesc { All = [ComponentType.ReadOnly<TrafficLights>()], None = [ComponentType.ReadOnly<CustomTrafficLights>()] })
                .ToComponentDataArray<Node>(Allocator.Temp);

            foreach (var item in trafficLightsNodeArray)
            {
                m_RenderSystem.AddIcon(item.m_Position, RenderSystem.Icon.TrafficLight);
            }
        }
        else if (GetToolState() == ToolState.RemoveTrafficLights)
        {
            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CustomTrafficLights>());
            var customTrafficLightsNodeArray = entityQuery.ToComponentDataArray<Node>(Allocator.Temp);
            var customTrafficLightsArray = entityQuery.ToComponentDataArray<CustomTrafficLights>(Allocator.Temp);
            for (int i = 0; i < customTrafficLightsNodeArray.Length; i++)
            {
                Node node = customTrafficLightsNodeArray[i];
                CustomTrafficLights customTrafficLights = customTrafficLightsArray[i];
                if (customTrafficLights.m_TrafficLightGroupEntity == m_SelectedTrafficLightGroupEntity)
                {
                    m_RenderSystem.AddIcon(node.m_Position, RenderSystem.Icon.TrafficLightLink);
                }
            }
        }
        else if (GetToolState() == ToolState.Choosed)
        {
            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<CustomTrafficLights>());
            var customTrafficLightsNodeArray = entityQuery.ToComponentDataArray<Node>(Allocator.Temp);
            var customTrafficLightsArray = entityQuery.ToComponentDataArray<CustomTrafficLights>(Allocator.Temp);
            for (int i = 0; i < customTrafficLightsNodeArray.Length; i++)
            {
                Node node = customTrafficLightsNodeArray[i];
                CustomTrafficLights customTrafficLights = customTrafficLightsArray[i];
                if (customTrafficLights.m_TrafficLightGroupEntity == m_SelectedTrafficLightGroupEntity)
                {
                    m_RenderSystem.AddIcon(node.m_Position, RenderSystem.Icon.TrafficLightLink);
                }
                else
                {
                    m_RenderSystem.AddIcon(node.m_Position, RenderSystem.Icon.TrafficLightWrench);
                }
            }

            var trafficLightsNodeArray = EntityManager
                .CreateEntityQuery(new EntityQueryDesc { All = [ComponentType.ReadOnly<TrafficLights>()], None = [ComponentType.ReadOnly<CustomTrafficLights>()] })
                .ToComponentDataArray<Node>(Allocator.Temp);
            foreach (var item in trafficLightsNodeArray)
            {
                m_RenderSystem.AddIcon(item.m_Position, RenderSystem.Icon.TrafficLight);
            }
        }
    }
}
