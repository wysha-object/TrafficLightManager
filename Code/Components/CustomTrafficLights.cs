using System.Linq;
using Colossal.Serialization.Entities;
using Game.Vehicles;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficLightManager.Code.Components;

public struct CustomTrafficLights : IComponentData, IQueryTypeParameter, ISerializable
{
    // Schema 1
    private const int DefaultSelectedPatternLength = 16;

    // Schema 3
    public float m_PedestrianPhaseDurationMultiplier { get; private set; }

    public int m_PedestrianPhaseGroupMask { get; private set; }

    // Schema 4
    public uint m_Timer;

    public byte m_ManualSignalGroup;

    // Schema 5
    public Entity m_TrafficLightGroupEntity;

    // Schema 7
    public int m_NextPassedCarCount;

    public void Serialize<TWriter>(TWriter writer)
        where TWriter : IWriter
    {
        int schemaVersion = 8;
        writer.Write(uint.MaxValue);
        writer.Write(schemaVersion);
        writer.Write(uint.MaxValue);
        writer.Write(m_PedestrianPhaseDurationMultiplier);
        writer.Write(m_PedestrianPhaseGroupMask);
        writer.Write(m_Timer);
        writer.Write(m_TrafficLightGroupEntity);
        writer.Write(m_NextPassedCarCount);
    }

    public void Deserialize<TReader>(TReader reader)
        where TReader : IReader
    {
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
        }

        if (schemaVersion == 6)
        {
            reader.Read(out float _);
        }

        if (schemaVersion >= 7)
        {
            reader.Read(out m_NextPassedCarCount);
        }
    }

    public CustomTrafficLights()
    {
        m_PedestrianPhaseDurationMultiplier = 1;
        m_PedestrianPhaseGroupMask = 0;
        m_Timer = 0;
        m_ManualSignalGroup = 0;
        m_TrafficLightGroupEntity = Entity.Null;
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
