using Colossal.Serialization.Entities;
using TrafficLightManager.Code.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficLightManager.Code.Components;

public struct CustomPhaseData : IBufferElementData, ISerializable
{
    public enum Options : uint
    {
        PrioritiseTrack = 1 << 0,

        PrioritisePublicCar = 1 << 1,

        PrioritisePedestrian = 1 << 2,

        LinkedWithNextPhase = 1 << 3,

        EndPhasePrematurely = 1 << 4,

        BindWithTemplate = 1 << 5,
    }

    // Statistics
    public ushort m_TurnsSinceLastRun;

    public ushort m_LowFlowTimer;

    public ushort m_LowPriorityTimer;

    public float3 m_CarFlow;

    public ushort m_CarLaneOccupied;

    public ushort m_PublicCarLaneOccupied;

    public ushort m_TrackLaneOccupied;

    public ushort m_PedestrianLaneOccupied;

    public float m_WeightedWaiting;

    public int m_Priority;

    // User configurable variables
    public Options m_Options;

    public ushort m_MinimumDuration;

    public ushort m_MaximumDuration;

    public float m_TargetDurationMultiplier;

    public float m_LaneOccupiedMultiplier;

    public float m_IntervalExponent;

    // Schema 3
    public FixedString64Bytes m_Name;

    public FixedString64Bytes m_BindTemplate;

    public void Serialize<TWriter>(TWriter writer)
        where TWriter : IWriter
    {
        ushort schemaVersion = 3;
        writer.Write(schemaVersion);
        writer.Write(m_TurnsSinceLastRun);
        writer.Write(m_LowFlowTimer);
        writer.Write(m_LowPriorityTimer);
        writer.Write(m_CarFlow);
        writer.Write(m_CarLaneOccupied);
        writer.Write(m_PublicCarLaneOccupied);
        writer.Write(m_TrackLaneOccupied);
        writer.Write(m_PedestrianLaneOccupied);
        writer.Write(m_WeightedWaiting);
        writer.Write(m_Priority);
        writer.Write((uint)m_Options);
        writer.Write(m_MinimumDuration);
        writer.Write(m_MaximumDuration);
        writer.Write(m_TargetDurationMultiplier);
        writer.Write(m_LaneOccupiedMultiplier);
        writer.Write(m_IntervalExponent);
        writer.Write(m_Name.ToString());
        writer.Write(m_BindTemplate.ToString());
    }

    public void Deserialize<TReader>(TReader reader)
        where TReader : IReader
    {
        ushort schemaVersion;
        reader.Read(out schemaVersion);
        Initialisation(Mod.m_Settings != null ? Mod.m_Settings.m_DefaultCustomPhaseTemplate : CustomPhaseTemplate.Default);
        reader.Read(out m_TurnsSinceLastRun);
        reader.Read(out m_LowFlowTimer);
        reader.Read(out m_LowPriorityTimer);
        reader.Read(out m_CarFlow);
        reader.Read(out m_CarLaneOccupied);
        reader.Read(out m_PublicCarLaneOccupied);
        reader.Read(out m_TrackLaneOccupied);
        reader.Read(out m_PedestrianLaneOccupied);
        reader.Read(out m_WeightedWaiting);
        if (schemaVersion <= 1)
        {
            reader.Read(out float _);
        }
        reader.Read(out m_Priority);
        reader.Read(out uint options);
        reader.Read(out m_MinimumDuration);
        reader.Read(out m_MaximumDuration);
        reader.Read(out m_TargetDurationMultiplier);
        reader.Read(out m_LaneOccupiedMultiplier);
        reader.Read(out m_IntervalExponent);
        if (schemaVersion >= 3)
        {
            reader.Read(out string name);
            m_Name = name;
            reader.Read(out string bindTemplate);
            m_BindTemplate = bindTemplate;
        }
        else
        {
            m_Name = "";
            m_BindTemplate = "";
        }
        m_Options = (Options)options;
    }

    private void Initialisation(CustomPhaseTemplate customPhaseTemplate)
    {
        m_Name = "";
        m_TurnsSinceLastRun = 0;
        m_LowFlowTimer = 0;
        m_LowPriorityTimer = 0;
        m_CarFlow = 0;
        m_CarLaneOccupied = 0;
        m_PublicCarLaneOccupied = 0;
        m_TrackLaneOccupied = 0;
        m_PedestrianLaneOccupied = 0;
        m_WeightedWaiting = 0;
        m_Priority = 0;
        m_Options = 0;
        if (customPhaseTemplate.m_IsPrioritiseTrack)
        {
            m_Options |= Options.PrioritiseTrack;
        }
        if (customPhaseTemplate.m_IsPrioritisePublicCar)
        {
            m_Options |= Options.PrioritisePublicCar;
        }
        if (customPhaseTemplate.m_IsPrioritisePedestrian)
        {
            m_Options |= Options.PrioritisePedestrian;
        }
        m_MinimumDuration = customPhaseTemplate.m_MinimumDuration;
        m_MaximumDuration = customPhaseTemplate.m_MaximumDuration;
        m_TargetDurationMultiplier = customPhaseTemplate.m_TargetDurationMultiplier;
        m_LaneOccupiedMultiplier = customPhaseTemplate.m_LaneOccupiedMultiplier;
        m_IntervalExponent = customPhaseTemplate.m_IntervalExponent;
        m_BindTemplate = "";
    }

    public CustomPhaseData()
    {
        Initialisation(CustomPhaseTemplate.Default);
    }

    public CustomPhaseData(CustomPhaseTemplate customPhaseTemplate)
    {
        Initialisation(customPhaseTemplate);
    }

    public readonly float AverageCarFlow()
    {
        return (m_CarFlow.x + m_CarFlow.y + m_CarFlow.z) / 3f;
    }

    public readonly int TotalLaneOccupied()
    {
        return m_CarLaneOccupied + m_TrackLaneOccupied + m_PedestrianLaneOccupied;
    }
}
