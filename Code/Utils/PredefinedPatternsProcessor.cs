using System;
using System.Collections.Generic;
using Game.Net;
using TrafficLightManager.Code.Components;
using TrafficLightManager.Code.Systems.Simulation;
using Unity.Collections;
using Unity.Entities;
using static TrafficLightManager.Code.Utils.NodeUtils;

namespace TrafficLightManager.Code.Utils;

public class PredefinedPatternsProcessor
{
    public static void GenerateCustomPhases(
        EntityManager entityManager, Entity group, BufferLookup<SubLane> subLaneLookup,
        BufferLookup<ConnectedEdge> connectedEdgeLookup,
        BufferLookup<LaneOverlap> laneOverlapLookup,
        ComponentLookup<Lane> laneLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        ComponentLookup<Edge> edgeData,
        ComponentLookup<EdgeGeometry> edgeGeometryData,
        ComponentLookup<Curve> curveData,
        bool leftHandTraffic,
        Settings.Values settings,
        CustomPhaseTemplate template
    )
    {
        var phases = entityManager.GetBuffer<CustomPhaseData>(group, false);
        if (phases.Length != 0)
        {
            return;
        }
        var edgePlans = new Dictionary<Entity, List<EdgeGroupMask>>();
        var lanePlans = new Dictionary<Entity, Dictionary<Entity, SubLaneGroupMask>>();
        int phaseCount = 0;
        foreach (var member in entityManager.GetBuffer<TrafficLightsMemberRef>(group, true))
        {
            Entity node = member.m_Entity;
            var lanes = subLaneLookup[node];
            var edges = connectedEdgeLookup[node];
            var signals = new Dictionary<Entity, LaneSignal>();
            foreach (var lane in lanes)
            {
                if (laneSignalData.TryGetComponent(lane.m_SubLane, out var signal))
                {
                    signals.Add(lane.m_SubLane, signal);
                }
            }
            try
            {
                var trafficLights = entityManager.GetComponentData<TrafficLights>(node);
                int count;
                if (edges.Length == 3)
                {
                    SetupThreeWayPhasing(subLaneLookup, laneOverlapLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, edges, lanes, out count, ref trafficLights);
                }
                else if (edges.Length == 4)
                {
                    SetupProtectedCentreTurn(subLaneLookup, laneOverlapLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, edges, lanes, out count, ref trafficLights);
                }
                else
                {
                    SetupSplitPhasing(subLaneLookup, laneOverlapLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, edges, lanes, out count, ref trafficLights);
                }
                if (settings.m_DefaultAlwaysGreenKerbsideTurn)
                {
                    AddAlwaysGreenKerbsideTurn(subLaneLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, edges, 0, lanes, ref count, ref trafficLights, updateExtraSignals: false);
                }
                if (settings.m_DefaultExclusivePedestrian)
                {
                    var customTrafficLights = entityManager.GetComponentData<CustomTrafficLights>(node);
                    AddExclusivePedestrianPhase(laneSignalData, pedestrianLaneData, lanes, ref count, ref trafficLights, ref customTrafficLights);
                }
                if (count > sizeof(ushort) * 8)
                {
                    throw new InvalidOperationException("Predefined pattern exceeds the 16-phase signal mask.");
                }
                phaseCount = Math.Max(phaseCount, count);
                var edgeMasks = new List<EdgeGroupMask>();
                foreach (var edge in edges)
                {
                    edgeMasks.Add(new EdgeGroupMask(edge.m_Edge,
                        GetEdgePosition(node, edge.m_Edge, edgeData, edgeGeometryData))
                    {
                        m_Options = EdgeGroupMask.Options.PerLaneSignal,
                    });
                }
                edgePlans.Add(node, edgeMasks);
                var masks = new Dictionary<Entity, SubLaneGroupMask>();
                using var connections = GetLaneConnectionMap(
                    Allocator.Temp, lanes, edges, subLaneLookup, laneLookup);
                foreach (var lane in lanes)
                {
                    Entity entity = lane.m_SubLane;
                    if (!signals.ContainsKey(entity) || masterLaneData.HasComponent(entity))
                    {
                        continue;
                    }
                    bool pedestrian = pedestrianLaneData.TryGetComponent(entity, out var pedestrianLane);
                    if (pedestrian && (pedestrianLane.m_Flags &
                        (PedestrianLaneFlags.Crosswalk | PedestrianLaneFlags.Unsafe)) ==
                        (PedestrianLaneFlags.Crosswalk | PedestrianLaneFlags.Unsafe))
                    {
                        continue;
                    }
                    var connection = GetLaneConnectionFromNodeSubLane(entity, connections, pedestrian);
                    Entity key = pedestrian ? entity : connection.m_SourceSubLane;
                    if (key == Entity.Null)
                    {
                        throw new InvalidOperationException("Signalized lane has no source lane.");
                    }
                    if (!masks.TryGetValue(key, out var mask))
                    {
                        mask = new SubLaneGroupMask(key, GetSubLanePosition(key, curveData));
                    }
                    ushort bits = laneSignalData[entity].m_GroupMask;
                    if (pedestrian)
                    {
                        mask.m_Pedestrian.m_GoGroupMask |= bits;
                    }
                    else if (trackLaneData.TryGetComponent(entity, out var track))
                    {
                        if ((track.m_Flags & TrackLaneFlags.TurnLeft) != 0)
                        {
                            mask.m_Track.m_Left.m_GoGroupMask |= bits;
                        }
                        else if ((track.m_Flags & TrackLaneFlags.TurnRight) != 0)
                        {
                            mask.m_Track.m_Right.m_GoGroupMask |= bits;
                        }
                        else
                        {
                            mask.m_Track.m_Straight.m_GoGroupMask |= bits;
                        }
                    }
                    else if (carLaneData.TryGetComponent(entity, out var car))
                    {
                        if ((car.m_Flags & (CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) != 0)
                        {
                            mask.m_Car.m_UTurn.m_GoGroupMask |= bits;
                        }
                        else if ((car.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft)) != 0)
                        {
                            mask.m_Car.m_Left.m_GoGroupMask |= bits;
                        }
                        else if ((car.m_Flags & (CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight)) != 0)
                        {
                            mask.m_Car.m_Right.m_GoGroupMask |= bits;
                        }
                        else
                        {
                            mask.m_Car.m_Straight.m_GoGroupMask |= bits;
                        }
                    }
                    masks[key] = mask;
                }
                lanePlans.Add(node, masks);
            }
            finally
            {
                foreach (var signal in signals)
                {
                    laneSignalData[signal.Key] = signal.Value;
                }
            }
        }
        for (int i = 0; i < phaseCount; i++)
        {
            phases.Add(new CustomPhaseData(template));
        }
        foreach (var plan in edgePlans)
        {
            var edges = entityManager.GetBuffer<EdgeGroupMask>(plan.Key, false);
            var lanes = entityManager.GetBuffer<SubLaneGroupMask>(plan.Key, false);
            edges.Clear();
            lanes.Clear();
            foreach (var mask in plan.Value)
            {
                edges.Add(mask);
            }
            foreach (var mask in lanePlans[plan.Key].Values)
            {
                lanes.Add(mask);
            }
        }
        var trafficLightGroup = entityManager.GetComponentData<TrafficLightGroup>(group);
        trafficLightGroup.m_ManualSignalGroup = 0;
        entityManager.SetComponentData(group, trafficLightGroup);
    }

    public static void SetupThreeWayPhasing(
        BufferLookup<SubLane> subLaneLookup,
        BufferLookup<LaneOverlap> laneOverlapLookup,
        ComponentLookup<Lane> laneLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        bool leftHandTraffic,
        DynamicBuffer<ConnectedEdge> connectedEdges,
        DynamicBuffer<SubLane> subLanes,
        out int groupCount,
        ref TrafficLights trafficLights
    )
    {
        using var connections = GetLaneConnectionMap(
            Allocator.Temp, subLanes, connectedEdges, subLaneLookup, laneLookup);
        foreach (var subLane in subLanes)
        {
            Entity lane = subLane.m_SubLane;
            if (!laneSignalData.TryGetComponent(lane, out var signal))
            {
                continue;
            }
            signal.m_GroupMask = 0;
            laneSignalData[lane] = signal;
        }

        groupCount = 0;
        for (int i = 0; i < connectedEdges.Length; i++)
        {
            for (int j = i + 1; j < connectedEdges.Length; j++)
            {
                Entity first = connectedEdges[i].m_Edge;
                Entity second = connectedEdges[j].m_Edge;
                foreach (var subLane in subLanes)
                {
                    Entity lane = subLane.m_SubLane;
                    if (!laneSignalData.TryGetComponent(lane, out var signal) ||
                        masterLaneData.HasComponent(lane) ||
                        (!trackLaneData.HasComponent(lane) && !carLaneData.HasComponent(lane)))
                    {
                        continue;
                    }
                    var connection = connections[lane];
                    bool uTurn = !trackLaneData.HasComponent(lane) &&
                        carLaneData.TryGetComponent(lane, out var car) &&
                        (car.m_Flags & (CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) != 0;
                    if ((connection.m_SourceEdge == first && connection.m_DestEdge == second) ||
                        (connection.m_SourceEdge == second && connection.m_DestEdge == first) ||
                        (uTurn && connection.m_SourceEdge == connection.m_DestEdge &&
                            (connection.m_SourceEdge == first || connection.m_SourceEdge == second)))
                    {
                        signal.m_GroupMask |= (ushort)(1 << groupCount);
                        laneSignalData[lane] = signal;
                    }
                }
                groupCount++;
            }
        }
        SetupMasterLanes(laneSignalData, masterLaneData, subLanes);
        SetupPedestrianLanes(laneOverlapLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, subLanes, groupCount, connections, threeWay: true);
        UpdateLaneSignal(laneSignalData, carLaneData, subLanes, ref trafficLights);
    }

    public static void SetupSplitPhasing(
        BufferLookup<SubLane> subLaneLookup,
        BufferLookup<LaneOverlap> laneOverlapLookup,
        ComponentLookup<Lane> laneLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        bool leftHandTraffic,
        DynamicBuffer<ConnectedEdge> connectedEdges,
        DynamicBuffer<SubLane> subLanes,
        out int groupCount,
        ref TrafficLights trafficLights
    )
    {
        using var laneConnectionMap = NodeUtils.GetLaneConnectionMap(
            Allocator.Temp,
            subLanes,
            connectedEdges,
            subLaneLookup,
            laneLookup
        );
        groupCount = 0;

        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            laneSignal.m_GroupMask = 0;
            laneSignalData[subLane] = laneSignal;
        }

        // Set up car lanes
        for (int i = 0; i < connectedEdges.Length; i++)
        {
            Entity edge = connectedEdges[i].m_Edge;
            bool modified = false;
            for (int j = 0; j < subLanes.Length; j++)
            {
                Entity subLane = subLanes[j].m_SubLane;
                if (laneConnectionMap[subLane].m_SourceEdge != edge)
                {
                    continue;
                }
                if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
                {
                    continue;
                }
                if (!carLaneData.HasComponent(laneConnectionMap[subLane].m_SourceSubLane))
                {
                    continue;
                }
                laneSignal.m_GroupMask |= (ushort)(1 << groupCount);
                laneSignalData[subLane] = laneSignal;
                modified = true;
            }
            if (modified)
            {
                groupCount++;
            }
        }

        // Set up track lanes
        for (int i = 0; i < connectedEdges.Length; i++)
        {
            Entity edge = connectedEdges[i].m_Edge;
            bool modified = false;
            for (int j = 0; j < subLanes.Length; j++)
            {
                Entity subLane = subLanes[j].m_SubLane;
                if (laneConnectionMap[subLane].m_SourceEdge != edge)
                {
                    continue;
                }
                if (!laneSignalData.TryGetComponent(subLane, out var laneSignal) || !trackLaneData.HasComponent(subLane))
                {
                    continue;
                }
                if (carLaneData.HasComponent(laneConnectionMap[subLane].m_SourceSubLane))
                {
                    continue;
                }
                laneSignal.m_GroupMask |= (ushort)(1 << groupCount);
                laneSignalData[subLane] = laneSignal;
                modified = true;
            }
            if (modified)
            {
                groupCount++;
            }
        }

        SetupNonOverlapLanes(laneOverlapLookup, laneSignalData, carLaneData, trackLaneData, subLanes, groupCount, laneConnectionMap);
        SetupMasterLanes(laneSignalData, masterLaneData, subLanes);
        RemoveDuplicateGroups(laneSignalData, subLanes, ref groupCount);
        SetupPedestrianLanes(laneOverlapLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, subLanes, groupCount, laneConnectionMap, threeWay: connectedEdges.Length == 3);
        CheckPedestrianLanes(laneSignalData, pedestrianLaneData, subLanes, ref groupCount);
        UpdateLaneSignal(laneSignalData, carLaneData, subLanes, ref trafficLights);
    }

    public static void SetupProtectedCentreTurn(
        BufferLookup<SubLane> subLaneLookup,
        BufferLookup<LaneOverlap> laneOverlapLookup,
        ComponentLookup<Lane> laneLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        bool leftHandTraffic,
        DynamicBuffer<ConnectedEdge> connectedEdges,
        DynamicBuffer<SubLane> subLanes,
        out int groupCount,
        ref TrafficLights trafficLights
    )
    {
        using var laneConnectionMap = NodeUtils.GetLaneConnectionMap(
            Allocator.Temp,
            subLanes,
            connectedEdges,
            subLaneLookup,
            laneLookup
        );
        groupCount = 0;

        NativeHashMap<Entity, Entity> straightEdgeMap = new(connectedEdges.Length, Allocator.Temp);
        using var fulfilledStraightEdgeList = new NativeHashSet<Entity>(connectedEdges.Length, Allocator.Temp);
        using var fulfilledTurnEdgeList = new NativeHashSet<Entity>(connectedEdges.Length, Allocator.Temp);
        ushort protectedTurnPhases = 0;

        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            laneSignal.m_GroupMask = 0;
            laneSignalData[subLane] = laneSignal;

            bool isCarLane = carLaneData.TryGetComponent(subLane, out var carLane);
            bool isTrackLane = trackLaneData.TryGetComponent(subLane, out var trackLane);
            if (!isCarLane && !isTrackLane)
            {
                continue;
            }
            if (
                isCarLane
                && (
                    carLane.m_Flags
                    & (
                        CarLaneFlags.TurnLeft
                        | CarLaneFlags.TurnRight
                        | CarLaneFlags.GentleTurnLeft
                        | CarLaneFlags.GentleTurnRight
                        | CarLaneFlags.UTurnLeft
                        | CarLaneFlags.UTurnRight
                    )
                ) != 0
            )
            {
                continue;
            }
            else if (isTrackLane && (trackLane.m_Flags & (TrackLaneFlags.TurnLeft | TrackLaneFlags.TurnRight)) != 0)
            {
                continue;
            }
            var laneConnection = laneConnectionMap[subLane];
            if (straightEdgeMap.ContainsKey(laneConnection.m_SourceEdge) || laneConnection.m_DestEdge == Entity.Null)
            {
                continue;
            }
            straightEdgeMap[laneConnection.m_SourceEdge] = laneConnection.m_DestEdge;
        }

        NativeArray<Entity> edgeArray = new(2, Allocator.Temp);
        for (int i = 0; i < connectedEdges.Length; i++)
        {
            edgeArray[0] = connectedEdges[i].m_Edge;
            straightEdgeMap.TryGetValue(edgeArray[0], out var opposite);
            edgeArray[1] = opposite;
            bool modified = false;
            foreach (Entity edge in edgeArray)
            {
                if (edge == Entity.Null || fulfilledStraightEdgeList.Contains(edge))
                {
                    continue;
                }
                fulfilledStraightEdgeList.Add(edge);
                for (int j = 0; j < subLanes.Length; j++)
                {
                    Entity subLane = subLanes[j].m_SubLane;
                    bool isCarLane = carLaneData.TryGetComponent(subLane, out var carLane);
                    bool isTrackLane = trackLaneData.TryGetComponent(subLane, out var trackLane);
                    if (laneConnectionMap[subLane].m_SourceEdge != edge)
                    {
                        continue;
                    }
                    if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
                    {
                        continue;
                    }
                    if (!isCarLane && !isTrackLane)
                    {
                        continue;
                    }
                    if (leftHandTraffic)
                    {
                        if (isCarLane && (carLane.m_Flags & (CarLaneFlags.GentleTurnRight | CarLaneFlags.TurnRight | CarLaneFlags.UTurnRight)) != 0)
                        {
                            continue;
                        }
                        else if (isTrackLane && (trackLane.m_Flags & TrackLaneFlags.TurnRight) != 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (isCarLane && (carLane.m_Flags & (CarLaneFlags.GentleTurnLeft | CarLaneFlags.TurnLeft | CarLaneFlags.UTurnLeft)) != 0)
                        {
                            continue;
                        }
                        else if (isTrackLane && (trackLane.m_Flags & TrackLaneFlags.TurnLeft) != 0)
                        {
                            continue;
                        }
                    }
                    laneSignal.m_GroupMask |= (ushort)(1 << groupCount);
                    laneSignalData[subLane] = laneSignal;
                    modified = true;
                }
            }
            if (modified)
            {
                groupCount++;
            }

            modified = false;
            foreach (Entity edge in edgeArray)
            {
                if (edge == Entity.Null || fulfilledTurnEdgeList.Contains(edge))
                {
                    continue;
                }
                fulfilledTurnEdgeList.Add(edge);
                for (int j = 0; j < subLanes.Length; j++)
                {
                    Entity subLane = subLanes[j].m_SubLane;
                    bool isCarLane = carLaneData.TryGetComponent(subLane, out var carLane);
                    bool isTrackLane = trackLaneData.TryGetComponent(subLane, out var trackLane);
                    if (laneConnectionMap[subLane].m_SourceEdge != edge)
                    {
                        continue;
                    }
                    if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
                    {
                        continue;
                    }
                    if (!isCarLane && !isTrackLane)
                    {
                        continue;
                    }
                    if (leftHandTraffic)
                    {
                        if (isCarLane && (carLane.m_Flags & (CarLaneFlags.GentleTurnRight | CarLaneFlags.TurnRight | CarLaneFlags.UTurnRight)) == 0)
                        {
                            continue;
                        }
                        else if (isTrackLane && (trackLane.m_Flags & TrackLaneFlags.TurnRight) == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (isCarLane && (carLane.m_Flags & (CarLaneFlags.GentleTurnLeft | CarLaneFlags.TurnLeft | CarLaneFlags.UTurnLeft)) == 0)
                        {
                            continue;
                        }
                        else if (isTrackLane && (trackLane.m_Flags & TrackLaneFlags.TurnLeft) == 0)
                        {
                            continue;
                        }
                    }
                    laneSignal.m_GroupMask |= (ushort)(1 << groupCount);
                    laneSignalData[subLane] = laneSignal;
                    modified = true;
                }
            }
            if (modified)
            {
                protectedTurnPhases |= (ushort)(1 << groupCount);
                groupCount++;
            }
        }

        SetupNonOverlapLanes(laneOverlapLookup, laneSignalData, carLaneData, trackLaneData, subLanes, groupCount, laneConnectionMap);
        SetupMasterLanes(laneSignalData, masterLaneData, subLanes);
        SetupPedestrianLanes(laneOverlapLookup, laneLookup, laneSignalData, masterLaneData, pedestrianLaneData, carLaneData, trackLaneData, leftHandTraffic, subLanes, groupCount, laneConnectionMap, protectedTurnPhases: protectedTurnPhases);
        CheckPedestrianLanes(laneSignalData, pedestrianLaneData, subLanes, ref groupCount);
        UpdateLaneSignal(laneSignalData, carLaneData, subLanes, ref trafficLights);
        edgeArray.Dispose();
        straightEdgeMap.Dispose();
    }

    private static void SetupNonOverlapLanes(
        BufferLookup<LaneOverlap> laneOverlapLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        DynamicBuffer<SubLane> subLanes,
        int groupCount,
        NativeHashMap<Entity, NodeUtils.LaneConnection> laneConnectionMap
    )
    {
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (!carLaneData.TryGetComponent(subLane, out var carLane) && !trackLaneData.HasComponent(subLane))
            {
                continue;
            }
            if ((carLane.m_Flags & (CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) != 0)
            {
                continue;
            }
            if (!laneOverlapLookup.TryGetBuffer(subLane, out var laneOverlapBuffer) || laneOverlapBuffer.Length == 0)
            {
                // Set all groups to green since there is no overlap
                laneSignal.m_GroupMask |= (ushort)((1 << groupCount) - 1);
            }
            else
            {
                ushort overlapGroupMask = 0;
                foreach (var laneOverlap in laneOverlapBuffer)
                {
                    Entity overlapSubLane = laneOverlap.m_Other;
                    if (!laneSignalData.TryGetComponent(overlapSubLane, out var overlapLaneSignal))
                    {
                        continue;
                    }
                    if (laneConnectionMap[subLane].m_SourceSubLane == laneConnectionMap[overlapSubLane].m_SourceSubLane)
                    {
                        continue;
                    }
                    if (!carLaneData.TryGetComponent(overlapSubLane, out var overlapCarLane) && !trackLaneData.HasComponent(overlapSubLane))
                    {
                        continue;
                    }
                    if ((overlapCarLane.m_Flags & (CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) != 0)
                    {
                        continue;
                    }
                    overlapGroupMask |= overlapLaneSignal.m_GroupMask;
                }
                laneSignal.m_GroupMask |= (ushort)((~overlapGroupMask) & ((1 << groupCount) - 1));
            }
            laneSignalData[subLane] = laneSignal;
        }
    }

    private static void RemoveDuplicateGroups(ComponentLookup<LaneSignal> laneSignalData, DynamicBuffer<SubLane> subLanes, ref int groupCount)
    {
        NativeList<NativeList<int>> greenLanes = new(groupCount, Allocator.Temp);
        for (int i = 0; i < groupCount; i++)
        {
            greenLanes.Add(new NativeList<int>(subLanes.Length, Allocator.Temp));
            for (int j = 0; j < subLanes.Length; j++)
            {
                Entity subLane = subLanes[j].m_SubLane;
                if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
                {
                    continue;
                }
                if ((laneSignal.m_GroupMask & (1 << i)) != 0)
                {
                    greenLanes[i].Add(j);
                }
            }
        }

        for (int i = 0; i < groupCount; i++)
        {
            for (int j = i + 1; j < groupCount; j++)
            {
                if (greenLanes[i].Length != greenLanes[j].Length)
                {
                    continue;
                }
                int k = 0;
                while (k < greenLanes[i].Length)
                {
                    if (greenLanes[i][k] != greenLanes[j][k])
                    {
                        break;
                    }
                    k++;
                }
                if (k >= greenLanes[i].Length)
                {
                    RemoveGroup(laneSignalData, subLanes, j);
                    greenLanes[j].Dispose();
                    greenLanes.RemoveAt(j);
                    groupCount--;
                    j--;
                }
            }
        }
        for (int i = 0; i < greenLanes.Length; i++)
        {
            greenLanes[i].Dispose();
        }
        greenLanes.Dispose();
    }

    private static void RemoveGroup(ComponentLookup<LaneSignal> laneSignalData, DynamicBuffer<SubLane> subLanes, int index)
    {
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            ushort mask = (ushort)((1 << index) - 1);
            laneSignal.m_GroupMask = (ushort)(((laneSignal.m_GroupMask >> 1) & ~mask) | (laneSignal.m_GroupMask & mask));
            laneSignalData[subLane] = laneSignal;
        }
    }

    private static void SetupMasterLanes(ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData, DynamicBuffer<SubLane> subLanes)
    {
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!masterLaneData.TryGetComponent(subLane, out MasterLane masterLane))
            {
                continue;
            }
            if (!laneSignalData.TryGetComponent(subLane, out LaneSignal laneSignal))
            {
                continue;
            }

            laneSignal.m_GroupMask = 0;
            for (int j = masterLane.m_MinIndex; j <= masterLane.m_MaxIndex; j++)
            {
                Entity slaveSubLane = subLanes[j].m_SubLane;
                if (!laneSignalData.TryGetComponent(slaveSubLane, out LaneSignal slaveLaneSignal))
                {
                    continue;
                }
                laneSignal.m_GroupMask |= slaveLaneSignal.m_GroupMask;
            }

            laneSignalData[subLane] = laneSignal;
        }
    }

    private static void SetupPedestrianLanes(
        BufferLookup<LaneOverlap> laneOverlapLookup,
        ComponentLookup<Lane> laneLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        bool leftHandTraffic,
        DynamicBuffer<SubLane> subLanes,
        int groupCount,
        NativeHashMap<Entity, NodeUtils.LaneConnection> laneConnectionMap,
        bool threeWay = false,
        ushort protectedTurnPhases = 0
    )
    {
        ushort allPhases = (ushort)((1 << groupCount) - 1);
        var crosswalkGroups = new NativeHashMap<Entity, Entity>(subLanes.Length, Allocator.Temp);
        var incomingMasks = new NativeHashMap<Entity, ushort>(subLanes.Length, Allocator.Temp);
        foreach (var subLane in subLanes)
        {
            Entity lane = subLane.m_SubLane;
            if (protectedTurnPhases == 0 || !laneSignalData.HasComponent(lane) ||
                !pedestrianLaneData.TryGetComponent(lane, out var pedestrian) ||
                (pedestrian.m_Flags & (PedestrianLaneFlags.Crosswalk | PedestrianLaneFlags.Unsafe)) != PedestrianLaneFlags.Crosswalk)
            {
                continue;
            }
            crosswalkGroups.Add(lane, lane);
        }
        foreach (var subLane in subLanes)
        {
            Entity lane = subLane.m_SubLane;
            if (!crosswalkGroups.ContainsKey(lane))
            {
                continue;
            }
            var path = laneLookup[lane];
            foreach (var other in subLanes)
            {
                if (!crosswalkGroups.TryGetValue(other.m_SubLane, out var otherGroup) ||
                    otherGroup == crosswalkGroups[lane])
                {
                    continue;
                }
                var otherPath = laneLookup[other.m_SubLane];
                if (!path.m_StartNode.Equals(otherPath.m_StartNode) && !path.m_StartNode.Equals(otherPath.m_EndNode) &&
                    !path.m_EndNode.Equals(otherPath.m_StartNode) && !path.m_EndNode.Equals(otherPath.m_EndNode))
                {
                    continue;
                }
                Entity group = crosswalkGroups[lane];
                foreach (var member in subLanes)
                {
                    if (crosswalkGroups.TryGetValue(member.m_SubLane, out var memberGroup) && memberGroup == otherGroup)
                    {
                        crosswalkGroups[member.m_SubLane] = group;
                    }
                }
            }
        }
        using var rightTurnEdges = new NativeHashSet<Entity>(subLanes.Length, Allocator.Temp);
        if (threeWay)
        {
            foreach (var subLane in subLanes)
            {
                Entity lane = subLane.m_SubLane;
                if (!laneSignalData.HasComponent(lane) || masterLaneData.HasComponent(lane))
                {
                    continue;
                }
                bool right = trackLaneData.TryGetComponent(lane, out var track)
                    ? (track.m_Flags & TrackLaneFlags.TurnRight) != 0 && (track.m_Flags & TrackLaneFlags.TurnLeft) == 0
                    : carLaneData.TryGetComponent(lane, out var car) &&
                        (car.m_Flags & (CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight)) != 0 &&
                        (car.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft | CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) == 0;
                if (right)
                {
                    rightTurnEdges.Add(laneConnectionMap[lane].m_SourceEdge);
                }
            }
        }
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal) || !pedestrianLaneData.TryGetComponent(subLane, out var pedestrianLane))
            {
                continue;
            }
            var subLaneConnection = GetLaneConnectionFromNodeSubLane(subLane, laneConnectionMap, true);
            crosswalkGroups.TryGetValue(subLane, out var edge);
            if (!laneOverlapLookup.TryGetBuffer(subLane, out var laneOverlapBuffer) || laneOverlapBuffer.Length == 0)
            {
                // Set all groups to green since there is no overlap
                laneSignal.m_GroupMask |= allPhases;
            }
            else
            {
                ushort overlapGroupMask = 0;
                foreach (var laneOverlap in laneOverlapBuffer)
                {
                    Entity overlapSubLane = laneOverlap.m_Other;
                    bool isCarLane = carLaneData.TryGetComponent(overlapSubLane, out var overlapCarLane);
                    bool isTrackLane = trackLaneData.TryGetComponent(overlapSubLane, out var overlapTrackLane);
                    if (((threeWay || protectedTurnPhases != 0) && masterLaneData.HasComponent(overlapSubLane)) ||
                        !laneSignalData.TryGetComponent(overlapSubLane, out var overlapLaneSignal))
                    {
                        continue;
                    }
                    if (!isCarLane && !isTrackLane)
                    {
                        continue;
                    }
                    if (!isTrackLane && (overlapCarLane.m_Flags & (CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) != 0)
                    {
                        continue;
                    }
                    if (threeWay)
                    {
                        bool right = isTrackLane
                            ? (overlapTrackLane.m_Flags & TrackLaneFlags.TurnRight) != 0 &&
                                (overlapTrackLane.m_Flags & TrackLaneFlags.TurnLeft) == 0
                            : (overlapCarLane.m_Flags & (CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight)) != 0 &&
                                (overlapCarLane.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft)) == 0;
                        bool straight = isTrackLane
                            ? (overlapTrackLane.m_Flags & (TrackLaneFlags.TurnLeft | TrackLaneFlags.TurnRight)) == 0
                            : (overlapCarLane.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft |
                                CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight |
                                CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) == 0;
                        if (right || (straight && !rightTurnEdges.Contains(laneConnectionMap[overlapSubLane].m_SourceEdge)))
                        {
                            continue;
                        }
                    }
                    if (
                        protectedTurnPhases != 0 ||
                        subLaneConnection.m_SourceEdge == laneConnectionMap[overlapSubLane].m_DestEdge
                        || subLaneConnection.m_DestEdge == laneConnectionMap[overlapSubLane].m_DestEdge
                    )
                    {
                        if (!threeWay && leftHandTraffic)
                        {
                            if (isTrackLane ? (overlapTrackLane.m_Flags & TrackLaneFlags.TurnLeft) != 0
                                : (overlapCarLane.m_Flags & (CarLaneFlags.GentleTurnLeft | CarLaneFlags.TurnLeft)) != 0)
                            {
                                continue;
                            }
                        }
                        if (threeWay || !leftHandTraffic)
                        {
                            if (
                                isTrackLane ? (overlapTrackLane.m_Flags & TrackLaneFlags.TurnRight) != 0
                                : (overlapCarLane.m_Flags & (CarLaneFlags.GentleTurnRight | CarLaneFlags.TurnRight)) != 0
                            )
                            {
                                continue;
                            }
                        }
                    }
                    overlapGroupMask |= overlapLaneSignal.m_GroupMask;
                    if (edge != Entity.Null && laneOverlap.m_OtherStart + laneOverlap.m_OtherEnd > byte.MaxValue)
                    {
                        incomingMasks.TryGetValue(edge, out var incoming);
                        incomingMasks[edge] = (ushort)(incoming | overlapLaneSignal.m_GroupMask);
                    }
                }
                laneSignal.m_GroupMask |= (ushort)(~overlapGroupMask & allPhases);
            }
            laneSignalData[subLane] = laneSignal;
        }
        foreach (var subLane in subLanes)
        {
            Entity lane = subLane.m_SubLane;
            if (!crosswalkGroups.TryGetValue(lane, out var edge))
            {
                continue;
            }
            var signal = laneSignalData[lane];
            if (protectedTurnPhases != 0)
            {
                bool divided = false;
                var path = laneLookup[lane];
                foreach (var other in subLanes)
                {
                    if (other.m_SubLane == lane || edge == Entity.Null ||
                        !crosswalkGroups.TryGetValue(other.m_SubLane, out var otherEdge) || otherEdge != edge)
                    {
                        continue;
                    }
                    var otherPath = laneLookup[other.m_SubLane];
                    bool sharesStart = path.m_StartNode.Equals(otherPath.m_StartNode) || path.m_StartNode.Equals(otherPath.m_EndNode);
                    bool sharesEnd = path.m_EndNode.Equals(otherPath.m_StartNode) || path.m_EndNode.Equals(otherPath.m_EndNode);
                    divided |= sharesStart != sharesEnd;
                }
                incomingMasks.TryGetValue(edge, out var incoming);
                signal.m_GroupMask &= (ushort)(~protectedTurnPhases | (divided ? incoming : 0));
            }
            laneSignalData[lane] = signal;
        }
        incomingMasks.Dispose();
        crosswalkGroups.Dispose();
    }

    public static void CheckPedestrianLanes(ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<PedestrianLane> pedestrianLaneData, DynamicBuffer<SubLane> subLanes, ref int groupCount)
    {
        bool needPedestrianPhase = false;
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal) || !pedestrianLaneData.HasComponent(subLane))
            {
                continue;
            }
            if (laneSignal.m_GroupMask == 0)
            {
                needPedestrianPhase = true;
                break;
            }
        }
        if (needPedestrianPhase)
        {
            for (int i = 0; i < subLanes.Length; i++)
            {
                Entity subLane = subLanes[i].m_SubLane;
                if (!laneSignalData.TryGetComponent(subLane, out var laneSignal) || !pedestrianLaneData.HasComponent(subLane))
                {
                    continue;
                }
                laneSignal.m_GroupMask |= (ushort)(1 << groupCount);
                laneSignalData[subLane] = laneSignal;
            }
            groupCount++;
        }
    }

    private static void UpdateLaneSignal(ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<CarLane> carLaneData, DynamicBuffer<SubLane> subLanes, ref TrafficLights trafficLights)
    {
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (carLaneData.HasComponent(subLane))
            {
                laneSignal.m_Flags |= LaneSignalFlags.CanExtend;
            }
            laneSignal.m_Default = 0;
            TrafficLightsUtils.UpdateLaneSignal(trafficLights, ref laneSignal);
            laneSignalData[subLane] = laneSignal;
        }
    }

    public static void AddExclusivePedestrianPhase(
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        DynamicBuffer<SubLane> subLanes,
        ref int groupCount,
        ref TrafficLights trafficLights,
        ref CustomTrafficLights customTrafficLights
    )
    {
        ushort pedestrianGroupMask = ushort.MaxValue;
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (pedestrianLaneData.HasComponent(subLane))
            {
                pedestrianGroupMask &= laneSignal.m_GroupMask;
            }
            else
            {
                pedestrianGroupMask &= (ushort)~laneSignal.m_GroupMask;
            }
        }

        if (pedestrianGroupMask == 0)
        {
            pedestrianGroupMask = (ushort)(1 << groupCount);
            groupCount++;
        }

        customTrafficLights.SetPedestrianPhaseGroupMask(pedestrianGroupMask);

        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (!pedestrianLaneData.HasComponent(subLane))
            {
                continue;
            }
            laneSignal.m_GroupMask = pedestrianGroupMask;
            TrafficLightsUtils.UpdateLaneSignal(trafficLights, ref laneSignal);
            laneSignalData[subLane] = laneSignal;
        }
    }

    public static void AddAlwaysGreenKerbsideTurn(
        BufferLookup<SubLane> subLaneLookup,
        ComponentLookup<Lane> laneLookup,
        ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<MasterLane> masterLaneData,
        ComponentLookup<PedestrianLane> pedestrianLaneData,
        ComponentLookup<CarLane> carLaneData,
        ComponentLookup<TrackLane> trackLaneData,
        bool leftHandTraffic,
        DynamicBuffer<ConnectedEdge> connectedEdges,
        int unfilteredChunkIndex,
        DynamicBuffer<SubLane> subLanes,
        ref int groupCount,
        ref TrafficLights trafficLights,
        bool updateExtraSignals = true,
        EntityCommandBuffer.ParallelWriter commandBuffer = default
    )
    {
        bool threeWay = connectedEdges.Length == 3;
        using var connections = GetLaneConnectionMap(Allocator.Temp, subLanes, connectedEdges, subLaneLookup, laneLookup);
        using var rightTurnEdges = new NativeHashSet<Entity>(connectedEdges.Length, Allocator.Temp);
        if (threeWay)
        {
            foreach (var subLane in subLanes)
            {
                Entity lane = subLane.m_SubLane;
                if (!laneSignalData.HasComponent(lane) || masterLaneData.HasComponent(lane))
                {
                    continue;
                }
                bool right = trackLaneData.TryGetComponent(lane, out var track)
                    ? (track.m_Flags & TrackLaneFlags.TurnRight) != 0 && (track.m_Flags & TrackLaneFlags.TurnLeft) == 0
                    : carLaneData.TryGetComponent(lane, out var car) &&
                        (car.m_Flags & (CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight)) != 0 &&
                        (car.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft | CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) == 0;
                if (right)
                {
                    rightTurnEdges.Add(connections[lane].m_SourceEdge);
                }
            }
        }
        ushort pedestrianGroupMask = ushort.MaxValue;
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (pedestrianLaneData.HasComponent(subLane))
            {
                pedestrianGroupMask &= laneSignal.m_GroupMask;
            }
            else
            {
                pedestrianGroupMask &= (ushort)~laneSignal.m_GroupMask;
            }
        }

        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (masterLaneData.HasComponent(subLane))
            {
                continue;
            }
            if (threeWay)
            {
                bool right;
                bool straight;
                if (trackLaneData.TryGetComponent(subLane, out var track))
                {
                    right = (track.m_Flags & TrackLaneFlags.TurnRight) != 0 && (track.m_Flags & TrackLaneFlags.TurnLeft) == 0;
                    straight = (track.m_Flags & (TrackLaneFlags.TurnLeft | TrackLaneFlags.TurnRight)) == 0;
                }
                else if (carLaneData.TryGetComponent(subLane, out var car))
                {
                    var turns = car.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft |
                        CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight | CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight);
                    right = (turns & (CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight)) != 0 &&
                        (turns & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft | CarLaneFlags.UTurnLeft | CarLaneFlags.UTurnRight)) == 0;
                    straight = turns == 0;
                }
                else
                {
                    continue;
                }
                if (!(rightTurnEdges.Contains(connections[subLane].m_SourceEdge) ? right : straight))
                {
                    continue;
                }
            }
            else
            {
                if (!carLaneData.TryGetComponent(subLane, out var carLane))
                {
                    continue;
                }
                var turn = leftHandTraffic ? CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft
                    : CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight;
                if ((carLane.m_Flags & turn) == 0)
                {
                    continue;
                }
            }

            ushort groupMask = (ushort)(~pedestrianGroupMask & ~laneSignal.m_GroupMask & ((1 << groupCount) - 1));
            laneSignal.m_GroupMask |= groupMask;

            ExtraLaneSignal extraLaneSignal = new();
            extraLaneSignal.m_IgnorePriorityGroupMask = groupMask;

            TrafficLightsUtils.UpdateLaneSignal(trafficLights, ref laneSignal, ref extraLaneSignal);
            laneSignalData[subLane] = laneSignal;
            if (updateExtraSignals)
            {
                commandBuffer.AddComponent(unfilteredChunkIndex, subLane, extraLaneSignal);
                commandBuffer.SetComponent(unfilteredChunkIndex, subLane, extraLaneSignal);
            }
        }
        SetupMasterLanes(laneSignalData, masterLaneData, subLanes);
    }

    public static void AddCentreTurnGiveWay(ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<CarLane> carLaneData,
        bool leftHandTraffic,
        EntityCommandBuffer.ParallelWriter commandBuffer, int unfilteredChunkIndex, DynamicBuffer<SubLane> subLanes, ref TrafficLights trafficLights)
    {
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (!carLaneData.TryGetComponent(subLane, out var carLane))
            {
                continue;
            }
            if (leftHandTraffic && (carLane.m_Flags & (CarLaneFlags.TurnRight | CarLaneFlags.GentleTurnRight)) == 0)
            {
                continue;
            }
            if (!leftHandTraffic && (carLane.m_Flags & (CarLaneFlags.TurnLeft | CarLaneFlags.GentleTurnLeft)) == 0)
            {
                continue;
            }

            ExtraLaneSignal extraLaneSignal = new();
            extraLaneSignal.m_IgnorePriorityGroupMask = 0;

            TrafficLightsUtils.UpdateLaneSignal(trafficLights, ref laneSignal, ref extraLaneSignal);
            laneSignalData[subLane] = laneSignal;
            commandBuffer.AddComponent(unfilteredChunkIndex, subLane, extraLaneSignal);
            commandBuffer.SetComponent(unfilteredChunkIndex, subLane, extraLaneSignal);
        }
    }

    public static void ResetExtraLaneSignal(ComponentLookup<LaneSignal> laneSignalData,
        ComponentLookup<ExtraLaneSignal> extraLaneSignalData, DynamicBuffer<SubLane> subLanes, ref TrafficLights trafficLights)
    {
        for (int i = 0; i < subLanes.Length; i++)
        {
            Entity subLane = subLanes[i].m_SubLane;
            if (!laneSignalData.TryGetComponent(subLane, out var laneSignal))
            {
                continue;
            }
            if (!extraLaneSignalData.TryGetComponent(subLane, out var extraLaneSignal))
            {
                continue;
            }
            extraLaneSignal.m_IgnorePriorityGroupMask = 0;
            TrafficLightsUtils.UpdateLaneSignal(trafficLights, ref laneSignal, ref extraLaneSignal);
            laneSignalData[subLane] = laneSignal;
            extraLaneSignalData[subLane] = extraLaneSignal;
        }
    }
}
