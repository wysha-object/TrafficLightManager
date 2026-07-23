using Colossal.Serialization.Entities;
using Game.Net;
using Unity.Entities;

namespace TrafficLightManager.Code.Components
{
    // 联动组实体组件
    public struct TrafficLightGroup : IComponentData, ISerializable
    {
        public byte m_ManualSignalGroup;

        // Schema 1
        public TrafficLightState m_State;
        public byte m_CurrentSignalGroup;
        public byte m_NextSignalGroup;
        public uint m_Timer;

        // Schema 2
        public float m_TargetDuration;

        public byte m_Status;

        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            m_ManualSignalGroup = 0;
            m_Status = 0;
            int schemaVersion;

            reader.Read(out schemaVersion);
            reader.Read(out byte state);
            m_State = (TrafficLightState)state;
            reader.Read(out m_CurrentSignalGroup);
            reader.Read(out m_NextSignalGroup);
            reader.Read(out m_Timer);

            if (schemaVersion >= 2)
            {
                reader.Read(out m_TargetDuration);
            }

            if (schemaVersion == 3)
            {
                reader.Read(out string _);
            }
        }

        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            int schemaVersion = 3;

            writer.Write(schemaVersion);
            writer.Write((byte)m_State);
            writer.Write(m_CurrentSignalGroup);
            writer.Write(m_NextSignalGroup);
            writer.Write(m_Timer);
            writer.Write(m_TargetDuration);
        }
    }
}
