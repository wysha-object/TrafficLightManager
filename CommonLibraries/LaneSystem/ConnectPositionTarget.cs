using Unity.Entities;
using Unity.Mathematics;

namespace C2VM.CommonLibraries.LaneSystem;

public struct ConnectPositionTarget : IBufferElementData
{
    public float3 m_Position;

    public ConnectPositionTarget(float3 position)
    {
        m_Position = position;
    }

    public bool Equals(ConnectPositionTarget other)
    {
        return m_Position.Equals(other.m_Position);
    }

    public override int GetHashCode()
    {
        return m_Position.GetHashCode();
    }
}