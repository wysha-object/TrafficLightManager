using Colossal.Serialization.Entities;
using Unity.Entities;
using Unity.Mathematics;

namespace C2VM.CommonLibraries.LaneSystem;

public struct CustomLaneDirection : IBufferElementData, IQueryTypeParameter, ISerializable
{

    public static Restriction LeftOnly = new Restriction{
        m_BanStraight = true,
        m_BanRight = true,
        m_BanUTurn = true
    };

    public static Restriction StraightOnly = new Restriction{
        m_BanLeft = true,
        m_BanRight = true,
        m_BanUTurn = true
    };

    public static Restriction RightOnly = new Restriction{
        m_BanLeft = true,
        m_BanStraight = true,
        m_BanUTurn = true
    };

    public static Restriction LeftAndStraight = new Restriction{
        m_BanRight = true,
        m_BanUTurn = true
    };

    public static Restriction RightAndStraight = new Restriction{
        m_BanLeft = true,
        m_BanUTurn = true
    };

    public static Restriction[][] DefaultConfig = new Restriction[9][]{
        new Restriction[0],
        new Restriction[1]{
            new Restriction{
                m_BanUTurn = true
            }
        },
        new Restriction[2]{
            LeftAndStraight,
            RightAndStraight
        },
        new Restriction[3]{
            LeftOnly,
            StraightOnly,
            RightOnly,
        },
        new Restriction[4]{
            LeftOnly,
            LeftAndStraight,
            RightAndStraight,
            RightOnly
        },
        new Restriction[5]{
            LeftOnly,
            StraightOnly,
            StraightOnly,
            StraightOnly,
            RightOnly
        },
        new Restriction[6]{
            LeftOnly,
            LeftOnly,
            StraightOnly,
            StraightOnly,
            RightOnly,
            RightOnly
        },
        new Restriction[7]{
            LeftOnly,
            LeftOnly,
            StraightOnly,
            StraightOnly,
            StraightOnly,
            RightOnly,
            RightOnly
        },
        new Restriction[8]{
            LeftOnly,
            LeftOnly,
            StraightOnly,
            StraightOnly,
            StraightOnly,
            StraightOnly,
            RightOnly,
            RightOnly
        }
    };

    public struct Restriction {
        public bool m_BanLeft;

        public bool m_BanRight;

        public bool m_BanStraight;

        public bool m_BanUTurn;
    }

    public float3 m_Position;

    public float3 m_Tangent;

    public Entity m_Owner;

    public ushort m_GroupIndex;

    public int m_LaneIndex;

    public Restriction m_Restriction;

    public bool m_Initialised;

    public static Restriction DefaultRestriction(int laneCount, int laneIndex)
    {
        if (laneCount >= DefaultConfig.Length || laneIndex >= DefaultConfig[laneCount].Length)
        {
            return default(Restriction);
        }
        return DefaultConfig[laneCount][laneIndex];
    }

    public CustomLaneDirection(float3 position, float3 tangent, Entity owner, ushort groupIndex, int laneIndex, Restriction restriction)
    {
        m_Position = position;
        m_Tangent = tangent;
        m_Owner = owner;
        m_GroupIndex = groupIndex;
        m_LaneIndex = laneIndex;
        m_Restriction = restriction;
        m_Initialised = true;
    }

    public bool Equals(CustomLaneDirection other)
    {
        if (this.Equals(other.m_Position, other.m_Tangent, other.m_Owner, other.m_GroupIndex, other.m_LaneIndex))
        {
            return true;
        }
        return false;
    }

    public bool Equals(float3 position, float3 tangent, Entity owner, ushort groupIndex, int laneIndex)
    {
        if (m_Position.Equals(position))
        {
            return true;
        }
        if (m_Owner.Equals(owner) && m_GroupIndex.Equals(groupIndex) && m_LaneIndex.Equals(laneIndex))
        {
            return true;
        }
        // Backward compatibility with schema version 2
        if (m_Owner.Equals(Entity.Null) && math.dot(math.normalizesafe(m_Tangent.xz), math.normalizesafe(tangent.xz)) > 0.99f && m_LaneIndex.Equals(laneIndex))
        {
            return true;
        }
        return false;
    }

    // Match with tangent instead of edge entity
    public bool LooseEquals(float3 position, float3 tangent, Entity owner, ushort groupIndex, int laneIndex)
    {
        if (m_Position.Equals(position))
        {
            return true;
        }
        if (
            (math.abs(m_Position.x - position.x) + math.abs(m_Position.y - position.y) + math.abs(m_Position.z - position.z)) < 3.0f &&
            math.dot(math.normalizesafe(m_Tangent.xz), math.normalizesafe(tangent.xz)) > 0.99f &&
            m_LaneIndex.Equals(laneIndex)
        )
        {
            return true;
        }
        return false;
    }

    public static bool Get(DynamicBuffer<CustomLaneDirection> buffer, float3 position, float3 tangent, Entity owner, ushort groupIndex, int laneIndex, out CustomLaneDirection customLaneDirection)
    {
        customLaneDirection = default;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Equals(position, tangent, owner, groupIndex, laneIndex))
            {
                customLaneDirection = buffer[i];
                return true;
            }
        }
        return false;
    }

    public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
    {
        writer.Write(float.MaxValue);
        writer.Write((int) 3); // Schema version
        writer.Write(m_Position);
        writer.Write(m_Tangent);
        writer.Write(m_GroupIndex);
        writer.Write(m_LaneIndex);
        writer.Write(m_Restriction.m_BanLeft);
        writer.Write(m_Restriction.m_BanRight);
        writer.Write(m_Restriction.m_BanStraight);
        writer.Write(m_Restriction.m_BanUTurn);
        writer.Write(m_Owner);
    }

    public void Deserialize<TReader>(TReader reader) where TReader : IReader
    {
        int schemaVersion;
        reader.Read(out float float1);
        if (float1 == float.MaxValue)
        {
            reader.Read(out schemaVersion);
            if (schemaVersion == 2 || schemaVersion == 3)
            {
                reader.Read(out m_Position);
                reader.Read(out m_Tangent);
                reader.Read(out m_GroupIndex);
                reader.Read(out m_LaneIndex);
                reader.Read(out m_Restriction.m_BanLeft);
                reader.Read(out m_Restriction.m_BanRight);
                reader.Read(out m_Restriction.m_BanStraight);
                reader.Read(out m_Restriction.m_BanUTurn);
                m_Owner = Entity.Null;
                if (schemaVersion == 3)
                {
                    reader.Read(out m_Owner);
                }
            }
        }
        else
        {
            schemaVersion = 1;
            reader.Read(out float y);
            reader.Read(out float z);
            m_Position = new float3(float1, y, z);
            reader.Read(out m_Restriction.m_BanLeft);
            reader.Read(out m_Restriction.m_BanRight);
            reader.Read(out m_Restriction.m_BanStraight);
            reader.Read(out m_Restriction.m_BanUTurn);
        }
        m_Initialised = true;
    }
}