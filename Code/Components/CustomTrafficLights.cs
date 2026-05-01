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

    // Schema 6
    public float m_TargetDuration;

    public byte m_Status;

    public void Serialize<TWriter>(TWriter writer)
        where TWriter : IWriter
    {
        int schemaVersion = 6;
        writer.Write(uint.MaxValue);
        writer.Write(schemaVersion);
        writer.Write(uint.MaxValue);
        writer.Write(m_PedestrianPhaseDurationMultiplier);
        writer.Write(m_PedestrianPhaseGroupMask);
        writer.Write(m_Timer);
        writer.Write(m_TrafficLightGroupEntity);
        writer.Write(m_TargetDuration);
    }

    public void Deserialize<TReader>(TReader reader)
        where TReader : IReader
    {
        m_Status = 0;
        int schemaVersion;

        reader.Read(out uint uint1);
        if (uint1 == uint.MaxValue)
        {
            reader.Read(out schemaVersion);
        }
        else
        {
            schemaVersion = 1;
        }

        if (schemaVersion == 1)
        {
            for (int i = 1; i < DefaultSelectedPatternLength; i++)
            {
                reader.Read(out uint _);
            }
        }

        if (schemaVersion >= 2)
        {
            reader.Read(out uint _);
        }

        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        if (schemaVersion >= 3)
        {
            reader.Read(out float pedestrianPhaseDurationMultiplier);
            reader.Read(out int pedestrianPhaseGroupMask);
            m_PedestrianPhaseDurationMultiplier = pedestrianPhaseDurationMultiplier;
            m_PedestrianPhaseGroupMask = pedestrianPhaseGroupMask;
        }

        if (schemaVersion >= 4)
        {
            reader.Read(out m_Timer);
        }
        m_ManualSignalGroup = 0;

        if (schemaVersion >= 5)
        {
            reader.Read(out m_TrafficLightGroupEntity);
            SetPatternOnly(Patterns.CustomPhase);
        }
        else
        {
            SetPatternOnly(Patterns.ModDefault);
        }

        if (schemaVersion >= 6)
        {
            reader.Read(out m_TargetDuration);
        }
    }

    public CustomTrafficLights()
    {
        m_Pattern = Patterns.ModDefault;
        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        m_Timer = 0;
        m_ManualSignalGroup = 0;
        m_TrafficLightGroupEntity = Entity.Null;
    }

    public CustomTrafficLights(Patterns pattern)
    {
        m_Pattern = pattern;
        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        m_Timer = 0;
        m_ManualSignalGroup = 0;
        m_TrafficLightGroupEntity = Entity.Null;
    }

    public CustomTrafficLights(Patterns pattern, Entity trafficLightGroupEntity)
    {
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
