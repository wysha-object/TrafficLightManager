using System.Collections.Generic;
using System.Linq;
using Colossal.Entities;
using Colossal.Mathematics;
using Game.Net;
using Game.Rendering;
using Game.UI;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Overlay;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    public void DrawNodeOutline(Entity node, Color color, float lineWidth, float offsetLength)
    {
        var overlayRenderSystem = World.GetOrCreateSystemManaged<OverlayRenderSystem>();
        var overlayBuffer = overlayRenderSystem.GetBuffer(out JobHandle dependencies);
        dependencies.Complete();
        if (EntityManager.TryGetBuffer<ConnectedEdge>(node, true, out var connectedEdges))
        {
            for (var i = 0; i < connectedEdges.Length; i++)
            {
                ConnectedEdge edge = connectedEdges[i];
                bool isNearEnd = node == EntityManager.GetComponentData<Edge>(edge.m_Edge).m_End;
                EdgeGeometry edgeGeometry = EntityManager.GetComponentData<EdgeGeometry>(edge.m_Edge);
                Segment edgeSegment = !isNearEnd ? edgeGeometry.m_Start : edgeGeometry.m_End;
                if (offsetLength > 0f && math.all(edgeSegment.m_Length > 0f))
                {
                    // more or less the offset length :)
                    float2 offsetFrac = math.clamp(offsetLength / edgeSegment.m_Length, float2.zero, new float2(1f));
                    float4 cut = new float4(
                        math.select(new float2(0f, offsetFrac.x), new float2(1f - offsetFrac.x, 1f), isNearEnd),
                        math.select(new float2(0f, offsetFrac.y), new float2(1f - offsetFrac.y, 1f), isNearEnd)
                    );
                    Bezier4x3 leftToCorner = MathUtils.Cut(edgeSegment.m_Left, cut.xy);
                    Bezier4x3 rightToCorner = MathUtils.Cut(edgeSegment.m_Right, cut.zw);
                    float3 leftCorner = math.select(leftToCorner.a, leftToCorner.d, !isNearEnd);
                    float3 rightCorner = math.select(rightToCorner.a, rightToCorner.d, !isNearEnd);
                    overlayBuffer.DrawLine(color, color, 0, 0, new Line3.Segment(leftCorner, rightCorner), lineWidth, 1);
                    overlayBuffer.DrawCurve(color, color, 0, 0, leftToCorner, lineWidth, 1);
                    overlayBuffer.DrawCurve(color, color, 0, 0, rightToCorner, lineWidth, 1);
                }
                else
                {
                    overlayBuffer.DrawLine(
                        color,
                        color,
                        0,
                        0,
                        new Line3.Segment(math.select(edgeSegment.m_Left.a, edgeSegment.m_Left.d, isNearEnd), math.select(edgeSegment.m_Right.a, edgeSegment.m_Right.d, isNearEnd)),
                        lineWidth,
                        1
                    );
                }

                EdgeNodeGeometry edgeNodeGeometry = !isNearEnd
                    ? EntityManager.GetComponentData<StartNodeGeometry>(edge.m_Edge).m_Geometry
                    : EntityManager.GetComponentData<EndNodeGeometry>(edge.m_Edge).m_Geometry;
                overlayBuffer.DrawCurve(color, color, 0, 0, edgeNodeGeometry.m_Left.m_Left, lineWidth, 1);
                overlayBuffer.DrawCurve(color, color, 0, 0, edgeNodeGeometry.m_Right.m_Right, lineWidth, 1);
                if (edgeNodeGeometry.m_MiddleRadius > 0)
                {
                    overlayBuffer.DrawCurve(color, color, 0, 0, edgeNodeGeometry.m_Left.m_Right, lineWidth, 1);
                    overlayBuffer.DrawCurve(color, color, 0, 0, edgeNodeGeometry.m_Right.m_Left, lineWidth, 1);
                }
            }
        }
    }

    public void DrawIcon()
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

    public void DrawGizmo(OverlayRenderSystem.Buffer overlayBuffer)
    {
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
                        && (GetDisplayPhaseIndex() < 0 || GetDisplayPhaseIndex() == displayIndex)
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
                                color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, color.a * 0.5f);
                                if ((laneSignal.m_GroupMask & 1 << displayIndex) != 0)
                                {
                                    overlayBuffer.DrawCurve(color, curve.m_Bezier, 2f);
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
                    else if (GetDisplayPhaseIndex() >= 0)
                    {
                        displayIndex = GetDisplayPhaseIndex();
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
                                overlayBuffer.DrawCurve(color, curve.m_Bezier, 0.3f);
                            }
                        }
                    }
                }
            }
        );
    }

    public void DrawEdge(OverlayRenderSystem.Buffer overlayBuffer)
    {
        ForEachTrafficLight(
            (e) =>
            {
                DrawNodeOutline(e, new Color(0f, 0.8f, 1f, 1f), 1.0f, 0.0f);
            }
        );
    }

    public void DrawTrafficLightGroupName(OverlayRenderSystem.Buffer overlayBuffer)
    {
        if (Mod.m_Settings.m_DisplayTrafficLightGroupNameWhenToolDisabled || GetToolState() != ToolState.Disabled)
        {
            var trafficLightGroupEntityArray = EntityManager
                .CreateEntityQuery(new EntityQueryDesc { All = [ComponentType.ReadOnly<TrafficLightGroup>()] })
                .ToEntityArray(Allocator.Temp);
            foreach (var trafficLightGroupEntity in trafficLightGroupEntityArray)
            {
                if (trafficLightGroupEntity == m_SelectedTrafficLightGroupEntity)
                {
                    continue;
                }
                if (EntityManager.TryGetBuffer(trafficLightGroupEntity, true, out DynamicBuffer<TrafficLightsMemberRef> trafficLightsMembers))
                {
                    float3 sum = 0f;
                    int count = 0;

                    for (int i = 0; i < trafficLightsMembers.Length; i++)
                    {
                        if (EntityManager.TryGetComponent<Node>(trafficLightsMembers[i].m_Entity, out var node))
                        {
                            sum += node.m_Position;
                            count++;
                        }
                    }

                    if (count == 0)
                    {
                        continue;
                    }

                    UITypes.WorldPosition center = sum / count;

                    overlayBuffer.DrawText(trafficLightGroupEntity, center, cameraFace: true);
                    overlayBuffer.DrawLine(new Color(0f, 0f, 0f, 0f), new Line3.Segment(center, center + new float3(0.02f, 0f, 0f)), 0.001f); // 只有 DrawText 时, OverlayRenderSystem.OnUpdate() 可能提前 return, OverlayRenderSystem.Render() 又只按曲线/网格计数开渲染通道,文本不会进入真正绘制.提交一条线, 激活 overlay 渲染通道
                }
            }
        }
    }
}
