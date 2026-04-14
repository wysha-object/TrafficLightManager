using Colossal.Serialization.Entities;
using Unity.Entities;

namespace TrafficLightManager.Code.Components
{
    // 联动组实体组件
    public struct TrafficLightsMemberRef : IBufferElementData, ISerializable
    {
        public Entity m_Entity;

        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            reader.Read(out m_Entity);
        }

        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            writer.Write(m_Entity);
        }
    }
}
