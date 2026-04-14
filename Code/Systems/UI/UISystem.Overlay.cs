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
        if (m_SelectedTrafficLightGroupEntity != Entity.Null)
        {
            m_RenderSystem.ClearLineMesh();
            ForEachTrafficLight(
                (e) =>
                {
                    if (EntityManager.TryGetBuffer<SubLane>(e, true, out var subLaneBuffer))
                    {
                        int displayIndex = 16;
                        if (
                            EntityManager.TryGetComponent<TrafficLightGroup>(m_SelectedTrafficLightGroupEntity, out var trafficLightGroup)
                            && trafficLightGroup.m_ManualSignalGroup > 0
                        )
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
                        if (m_DebugDisplayGroup > 0)
                        {
                            displayIndex = m_DebugDisplayGroup - 1;
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
                                    m_RenderSystem.AddBezier(curve.m_Bezier, color, curve.m_Length, 0.25f);
                                }
                            }
                        }
                    }
                }
            );
            m_RenderSystem.BuildLineMesh();
        }
    }

    public void RedrawIcon()
    {
        m_RenderSystem.ClearIconList();
        if (GetToolState() == ToolState.ChooseGroup)
        {
            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Node>(), ComponentType.ReadOnly<CustomTrafficLights>());
            var nodeArray = entityQuery.ToComponentDataArray<Node>(Allocator.Temp);
            var customTrafficLightsArray = entityQuery.ToComponentDataArray<CustomTrafficLights>(Allocator.Temp);
            for (int i = 0; i < nodeArray.Length; i++)
            {
                var node = nodeArray[i];
                var customTrafficLights = customTrafficLightsArray[i];
                RenderSystem.Icon icon = RenderSystem.Icon.TrafficLight;
                if (customTrafficLights.GetPatternOnly() == CustomTrafficLights.Patterns.CustomPhase)
                {
                    icon = RenderSystem.Icon.TrafficLightWrench;
                }
                m_RenderSystem.AddIcon(node.m_Position, icon);
            }
        }
    }
}
