using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colossal.Serialization.Entities;
using Unity.Entities;

namespace TrafficLightManager.Code.Components
{
    public struct TrackedCar : IComponentData, ISerializable
    {
        public Entity m_CustomTrafficLights;
        public Entity m_SubLane;

        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            reader.Read(out ushort schemaVersion);

            reader.Read(out m_CustomTrafficLights);
            reader.Read(out m_SubLane);
        }

        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            ushort schemaVersion = 1;
            writer.Write(schemaVersion);

            writer.Write(m_CustomTrafficLights);
            writer.Write(m_SubLane);
        }
    }
}
