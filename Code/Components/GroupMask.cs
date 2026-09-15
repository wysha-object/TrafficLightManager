using Colossal.Serialization.Entities;
using Colossal.UI.Binding;

namespace TrafficLightManager.Code.Components;

public struct GroupMask
{
    public struct Signal : ISerializable, IJsonWritable
    {
        public ushort m_GoGroupMask;

        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            reader.Read(out ushort schemaVersion);

            reader.Read(out m_GoGroupMask);
            if (schemaVersion <= 1)
            {
                reader.Read(out ushort _);
            }
        }

        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            ushort schemaVersion = 2;
            writer.Write(schemaVersion);

            writer.Write(m_GoGroupMask);
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(Signal).FullName);
            writer.PropertyName("m_GoGroupMask");
            writer.Write(m_GoGroupMask);
            writer.TypeEnd();
        }

        public Signal()
        {
            m_GoGroupMask = 0;
        }
    }

    public struct Turn : ISerializable, IJsonWritable
    {
        private ushort m_SchemaVersion;

        public Signal m_Left;

        public Signal m_Straight;

        public Signal m_Right;

        public Signal m_UTurn;

        public void Deserialize<TReader>(TReader reader)
            where TReader : IReader
        {
            reader.Read(out m_SchemaVersion);
            reader.Read(out m_Left);
            reader.Read(out m_Straight);
            reader.Read(out m_Right);
            reader.Read(out m_UTurn);
        }

        public void Serialize<TWriter>(TWriter writer)
            where TWriter : IWriter
        {
            writer.Write(m_SchemaVersion);
            writer.Write(m_Left);
            writer.Write(m_Straight);
            writer.Write(m_Right);
            writer.Write(m_UTurn);
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(Turn).FullName);
            writer.PropertyName("m_Left");
            writer.Write(m_Left);
            writer.PropertyName("m_Straight");
            writer.Write(m_Straight);
            writer.PropertyName("m_Right");
            writer.Write(m_Right);
            writer.PropertyName("m_UTurn");
            writer.Write(m_UTurn);
            writer.TypeEnd();
        }

        public Turn()
        {
            m_SchemaVersion = 1;
            m_Left = new Signal();
            m_Straight = new Signal();
            m_Right = new Signal();
            m_UTurn = new Signal();
        }
    }
}
