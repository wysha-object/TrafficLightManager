using System.Linq;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace TrafficLightManager.Code.Components;

public struct CustomTrafficLights : IComponentData, IQueryTypeParameter, ISerializable
{
    /**
     * 只使用CustomPhase,其他模式不直接使用,仅用于完成默认模式下的更新
     */
    public enum Patterns : uint
    {
        Vanilla = 0,

        SplitPhasing = 1,

        ProtectedCentreTurn = 2,

        SplitPhasingAdvancedObsolete = 3,

        ModDefault = 4,

        CustomPhase = 5,

        ExclusivePedestrian = 1 << 16,

        AlwaysGreenKerbsideTurn = 1 << 17,

        CentreTurnGiveWay = 1 << 18,
    }

    private int m_SchemaVersion;

    // Schema 1
    private const int DefaultSelectedPatternLength = 16;

    // Schema 2
    private Patterns m_Pattern;

    // Schema 3
    public float m_PedestrianPhaseDurationMultiplier { get; private set; }

    public int m_PedestrianPhaseGroupMask { get; private set; }

    // Schema 4
    public uint m_Timer;

    public byte m_ManualSignalGroup;

    // Schema 5
    public Entity m_TrafficLightGroupEntity;

    public void Serialize<TWriter>(TWriter writer)
        where TWriter : IWriter
    {
        m_SchemaVersion = 5;
        writer.Write(uint.MaxValue);
        writer.Write(m_SchemaVersion);
        writer.Write(uint.MaxValue);
        writer.Write(m_PedestrianPhaseDurationMultiplier);
        writer.Write(m_PedestrianPhaseGroupMask);
        writer.Write(m_Timer);
        writer.Write(m_TrafficLightGroupEntity);
    }

    public void Deserialize<TReader>(TReader reader)
        where TReader : IReader
    {
        reader.Read(out uint uint1);
        if (uint1 == uint.MaxValue)
        {
            reader.Read(out m_SchemaVersion);
        }
        else
        {
            m_SchemaVersion = 1;
        }

        if (m_SchemaVersion == 1)
        {
            for (int i = 1; i < DefaultSelectedPatternLength; i++)
            {
                reader.Read(out uint _);
            }
        }

        if (m_SchemaVersion >= 2)
        {
            reader.Read(out uint _);
        }

        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        if (m_SchemaVersion >= 3)
        {
            reader.Read(out float pedestrianPhaseDurationMultiplier);
            reader.Read(out int pedestrianPhaseGroupMask);
            m_PedestrianPhaseDurationMultiplier = pedestrianPhaseDurationMultiplier;
            m_PedestrianPhaseGroupMask = pedestrianPhaseGroupMask;
        }

        if (m_SchemaVersion >= 4)
        {
            reader.Read(out m_Timer);
        }
        m_ManualSignalGroup = 0;

        if (m_SchemaVersion >= 5)
        {
            reader.Read(out m_TrafficLightGroupEntity);
            SetPatternOnly(Patterns.CustomPhase);
        }
        else
        {
            SetPatternOnly(Patterns.ModDefault);
        }
    }

    public CustomTrafficLights()
    {
        m_SchemaVersion = 5;
        m_Pattern = Patterns.ModDefault;
        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        m_Timer = 0;
        m_ManualSignalGroup = 0;
        m_TrafficLightGroupEntity = Entity.Null;
    }

    public CustomTrafficLights(Patterns pattern)
    {
        m_SchemaVersion = 5;
        m_Pattern = pattern;
        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        m_Timer = 0;
        m_ManualSignalGroup = 0;
        m_TrafficLightGroupEntity = Entity.Null;
    }

    public CustomTrafficLights(Patterns pattern, Entity trafficLightGroupEntity)
    {
        m_SchemaVersion = 5;
        m_Pattern = pattern;
        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        m_Timer = 0;
        m_ManualSignalGroup = 0;
        m_TrafficLightGroupEntity = trafficLightGroupEntity;
    }

    public Patterns GetPattern()
    {
        return m_Pattern;
    }

    public Patterns GetPatternOnly()
    {
        return (Patterns)((uint)GetPattern() & 0xFFFF);
    }

    public void SetPattern(uint pattern)
    {
        SetPattern((Patterns)pattern);
    }

    public void SetPattern(Patterns pattern)
    {
        m_Pattern = pattern;
    }

    public void SetPatternOnly(Patterns pattern)
    {
        m_Pattern = (Patterns)(((uint)m_Pattern & 0xFFFF0000) | ((uint)pattern & 0xFFFF));
    }

    public void SetPedestrianPhaseDurationMultiplier(float durationMultiplier)
    {
        m_PedestrianPhaseDurationMultiplier = durationMultiplier;
    }

    public void SetPedestrianPhaseGroupMask(int groupMask)
    {
        m_PedestrianPhaseGroupMask = groupMask;
    }
}
