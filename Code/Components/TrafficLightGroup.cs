using Colossal.Serialization.Entities;
using Game.Net;
using Unity.Entities;

namespace TrafficLightManager.Code.Components
{
    // 联动组实体组件
    public struct TrafficLightGroup : IComponentData, ISerializable
    {
        private int m_SchemaVersion;

        // Schema 1
        public TrafficLightState m_State;
        public byte m_CurrentSignalGroup;
        public byte m_NextSignalGroup;
        public uint m_Timer;
        public byte m_ManualSignalGroup;

        // Schema 2
        public float m_TargetDuration;

        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            reader.Read(out m_SchemaVersion);
            reader.Read(out byte state);
            m_State = (TrafficLightState)state;
            reader.Read(out m_CurrentSignalGroup);
            reader.Read(out m_NextSignalGroup);
            reader.Read(out m_Timer);
            m_ManualSignalGroup = 0;
        }

        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            m_SchemaVersion = 1;
            writer.Write(m_SchemaVersion);
            writer.Write((byte)m_State);
            writer.Write(m_CurrentSignalGroup);
            writer.Write(m_NextSignalGroup);
            writer.Write(m_Timer);
        }
    }
}
