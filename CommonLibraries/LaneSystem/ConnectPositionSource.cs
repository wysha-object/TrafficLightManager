using Unity.Entities;
using Unity.Mathematics;

namespace C2VM.CommonLibraries.LaneSystem;

public struct ConnectPositionSource : IBufferElementData
{
    public float3 m_Position;

    public float3 m_Tangent;

    public Entity m_Owner;

    public ushort m_GroupIndex;

    public int m_LaneIndex;

    public bool m_HasLeftTurnEdge;

    public bool m_HasRightTurnEdge;

    public bool m_HasStraightEdge;

    public ConnectPositionSource(float3 position, float3 tangent, Entity owner, ushort groupIndex, int laneIndex)
    {
        m_Position = position;
        m_Tangent = tangent;
        m_Owner = owner;
        m_GroupIndex = groupIndex;
        m_LaneIndex = laneIndex;
    }

    public bool Equals(ConnectPositionSource other)
    {
        if (m_Position.Equals(other.m_Position))
        {
            return true;
        }
        if (m_Owner.Equals(other.m_Owner) && m_GroupIndex.Equals(other.m_GroupIndex) && m_LaneIndex.Equals(other.m_LaneIndex))
        {
            return true;
        }
        return false;
    }

    public static bool Contains(DynamicBuffer<ConnectPositionSource> buffer, ConnectPositionSource position)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Equals(position))
            {
                return true;
            }
        }
        return false;
    }

    public static explicit operator ConnectPositionSource(CustomLaneDirection lane) {
        return new ConnectPositionSource(lane.m_Position, lane.m_Tangent, lane.m_Owner, lane.m_GroupIndex, lane.m_LaneIndex);
    }
}