using System;
using System.Collections.Generic;
using Colossal.UI.Binding;
using TrafficLightManager.Code.Utils;

namespace TrafficLightManager.Code.Systems.UI;

public static class UITypes
{
    public struct CustomPhaseItem : IJsonWritable
    {
        public ushort turnsSinceLastRun;

        public ushort lowFlowTimer;

        public ushort carLaneOccupied;

        public ushort publicCarLaneOccupied;

        public ushort trackLaneOccupied;

        public ushort pedestrianLaneOccupied;

        public float weightedWaiting;

        public int priority;

        public ushort minimumDuration;

        public ushort maximumDuration;

        public float targetDurationMultiplier;

        public float laneOccupiedMultiplier;

        public float intervalFactor;

        public bool prioritiseTrack;

        public bool prioritisePublicCar;

        public bool prioritisePedestrian;

        public bool linkedWithNextPhase;

        public bool endPhasePrematurely;

        public bool bindWithTemplate;

        public string name;

        public string bindTemplate;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(CustomPhaseItem).FullName);
            writer.PropertyName(nameof(turnsSinceLastRun));
            writer.Write((int)turnsSinceLastRun);
            writer.PropertyName(nameof(lowFlowTimer));
            writer.Write((int)lowFlowTimer);
            writer.PropertyName(nameof(carLaneOccupied));
            writer.Write((int)carLaneOccupied);
            writer.PropertyName(nameof(publicCarLaneOccupied));
            writer.Write((int)publicCarLaneOccupied);
            writer.PropertyName(nameof(trackLaneOccupied));
            writer.Write((int)trackLaneOccupied);
            writer.PropertyName(nameof(pedestrianLaneOccupied));
            writer.Write((int)pedestrianLaneOccupied);
            writer.PropertyName(nameof(weightedWaiting));
            writer.Write(weightedWaiting);
            writer.PropertyName(nameof(priority));
            writer.Write(priority);
            writer.PropertyName(nameof(minimumDuration));
            writer.Write((int)minimumDuration);
            writer.PropertyName(nameof(maximumDuration));
            writer.Write((int)maximumDuration);
            writer.PropertyName(nameof(targetDurationMultiplier));
            writer.Write(targetDurationMultiplier);
            writer.PropertyName(nameof(laneOccupiedMultiplier));
            writer.Write(laneOccupiedMultiplier);
            writer.PropertyName(nameof(intervalFactor));
            writer.Write(intervalFactor);
            writer.PropertyName(nameof(prioritiseTrack));
            writer.Write(prioritiseTrack);
            writer.PropertyName(nameof(prioritisePublicCar));
            writer.Write(prioritisePublicCar);
            writer.PropertyName(nameof(prioritisePedestrian));
            writer.Write(prioritisePedestrian);
            writer.PropertyName(nameof(linkedWithNextPhase));
            writer.Write(linkedWithNextPhase);
            writer.PropertyName(nameof(endPhasePrematurely));
            writer.Write(endPhasePrematurely);
            writer.PropertyName(nameof(bindWithTemplate));
            writer.Write(bindWithTemplate);
            writer.PropertyName(nameof(name));
            writer.Write(name);
            writer.PropertyName(nameof(bindTemplate));
            writer.Write(bindTemplate);
            writer.TypeEnd();
        }
    }

    public struct TrafficLightGroup : IJsonWritable
    {
        public uint timer;
        public int currentPhaseIndex;
        public int manualPhaseIndex;
        public float targetDuration;
        public float passedCarCount;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(TrafficLightGroup).FullName);
            writer.PropertyName(nameof(timer));
            writer.Write(timer);
            writer.PropertyName(nameof(currentPhaseIndex));
            writer.Write(currentPhaseIndex);
            writer.PropertyName(nameof(manualPhaseIndex));
            writer.Write(manualPhaseIndex);
            writer.PropertyName(nameof(targetDuration));
            writer.Write(targetDuration);
            writer.PropertyName(nameof(passedCarCount));
            writer.Write(passedCarCount);
            writer.TypeEnd();
        }
    }

    public struct CityConfiguration : IJsonWritable
    {
        public bool leftHandTraffic;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(CityConfiguration).FullName);
            writer.PropertyName(nameof(leftHandTraffic));
            writer.Write(leftHandTraffic);
            writer.TypeEnd();
        }
    }

    public struct Camera : IJsonWritable
    {
        public WorldPosition position;
        public WorldPosition rotation;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(Camera).FullName);
            writer.PropertyName(nameof(position));
            writer.Write(position);
            writer.PropertyName(nameof(rotation));
            writer.Write(rotation);
            writer.TypeEnd();
        }
    }

    public struct TrafficLightMember : IJsonWritable
    {
        public int entityIndex;
        public WorldPosition? position;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(TrafficLightMember).FullName);
            writer.PropertyName(nameof(entityIndex));
            writer.Write(entityIndex);
            writer.PropertyName(nameof(position));
            if (position.HasValue)
            {
                writer.Write(position.Value);
            }
            else
            {
                writer.WriteNull();
            }
            writer.TypeEnd();
        }
    }

    public struct TemplateValue : IJsonReadable, IJsonWritable
    {
        public CustomPhaseTemplate template;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_Name));
            reader.Read(out template.m_Name);
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_IsPrioritiseTrack));
            reader.Read(out template.m_IsPrioritiseTrack);
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_IsPrioritisePublicCar));
            reader.Read(out template.m_IsPrioritisePublicCar);
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_IsPrioritisePedestrian));
            reader.Read(out template.m_IsPrioritisePedestrian);
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_MinimumDuration));
            reader.Read(out int minimumDuration);
            template.m_MinimumDuration = (ushort)minimumDuration;
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_MaximumDuration));
            reader.Read(out int maximumDuration);
            template.m_MaximumDuration = (ushort)maximumDuration;
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_TargetDurationMultiplier));
            reader.Read(out template.m_TargetDurationMultiplier);
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_LaneOccupiedMultiplier));
            reader.Read(out template.m_LaneOccupiedMultiplier);
            reader.ReadProperty(nameof(CustomPhaseTemplate.m_IntervalFactor));
            reader.Read(out template.m_IntervalFactor);
            reader.ReadMapEnd();
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(TemplateValue).FullName);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_Name));
            writer.Write(template.m_Name);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_IsPrioritiseTrack));
            writer.Write(template.m_IsPrioritiseTrack);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_IsPrioritisePublicCar));
            writer.Write(template.m_IsPrioritisePublicCar);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_IsPrioritisePedestrian));
            writer.Write(template.m_IsPrioritisePedestrian);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_MinimumDuration));
            writer.Write(template.m_MinimumDuration);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_MaximumDuration));
            writer.Write(template.m_MaximumDuration);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_TargetDurationMultiplier));
            writer.Write(template.m_TargetDurationMultiplier);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_LaneOccupiedMultiplier));
            writer.Write(template.m_LaneOccupiedMultiplier);
            writer.PropertyName(nameof(CustomPhaseTemplate.m_IntervalFactor));
            writer.Write(template.m_IntervalFactor);
            writer.TypeEnd();
        }
    }

    public struct SettingsValue : IJsonWritable
    {
        public List<CustomPhaseTemplate> customPhaseTemplates;
        public CustomPhaseTemplate defaultCustomPhaseTemplate;

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(SettingsValue).FullName);
            writer.PropertyName(nameof(customPhaseTemplates));
            writer.ArrayBegin(customPhaseTemplates.Count);
            foreach (var template in customPhaseTemplates)
            {
                writer.Write(new TemplateValue { template = template });
            }
            writer.ArrayEnd();
            writer.PropertyName(nameof(defaultCustomPhaseTemplate));
            writer.Write(new TemplateValue { template = defaultCustomPhaseTemplate });
            writer.TypeEnd();
        }
    }

    public struct BindTemplateInput : IJsonReadable
    {
        public bool bindWithTemplate;
        public string templateName;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(bindWithTemplate));
            reader.Read(out bindWithTemplate);
            templateName = string.Empty;
            if (reader.ReadProperty(nameof(templateName)))
            {
                reader.Read(out templateName);
            }
            reader.ReadMapEnd();
        }
    }

    public enum UpdateCustomPhaseType : int
    {
        MinimumDuration = 0,
        MaximumDuration = 1,
        TargetDurationMultiplier = 2,
        LaneOccupiedMultiplier = 3,
        IntervalFactor = 4,
        PrioritiseTrack = 5,
        PrioritisePublicCar = 6,
        PrioritisePedestrian = 7,
        LinkedWithNextPhase = 8,
        EndPhasePrematurely = 9,
        Name = 10,
        BindWithTemplate = 11,
        ApplyTemplate = 12,
    }

    public struct UpdateCustomPhaseData : IJsonReadable
    {
        public int index;

        public UpdateCustomPhaseType key;

        public object value;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            index = -1;
            if (reader.ReadProperty(nameof(index)))
            {
                reader.Read(out index);
            }
            reader.ReadProperty(nameof(key));
            reader.Read(out int keyInt);
            key = (UpdateCustomPhaseType)keyInt;
            if (key != UpdateCustomPhaseType.EndPhasePrematurely)
            {
                reader.ReadProperty(nameof(value));
                switch (key)
                {
                    case UpdateCustomPhaseType.MinimumDuration:
                    case UpdateCustomPhaseType.MaximumDuration:
                        reader.Read(out int duration);
                        value = (ushort)Math.Clamp(duration, 0, ushort.MaxValue);
                        break;
                    case UpdateCustomPhaseType.TargetDurationMultiplier:
                    case UpdateCustomPhaseType.LaneOccupiedMultiplier:
                    case UpdateCustomPhaseType.IntervalFactor:
                        reader.Read(out float multiplier);
                        value = multiplier;
                        break;
                    case UpdateCustomPhaseType.PrioritiseTrack:
                    case UpdateCustomPhaseType.PrioritisePublicCar:
                    case UpdateCustomPhaseType.PrioritisePedestrian:
                    case UpdateCustomPhaseType.LinkedWithNextPhase:
                        reader.Read(out bool enabled);
                        value = enabled;
                        break;
                    case UpdateCustomPhaseType.Name:
                    case UpdateCustomPhaseType.ApplyTemplate:
                        reader.Read(out string name);
                        value = name;
                        break;
                    case UpdateCustomPhaseType.BindWithTemplate:
                        var binding = new BindTemplateInput();
                        binding.Read(reader);
                        value = binding;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported custom phase field: {key}");
                }
            }
            reader.ReadMapEnd();
        }
    }

    public struct SwapCustomPhaseInput : IJsonReadable
    {
        public int index1;
        public int index2;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(index1));
            reader.Read(out index1);
            reader.ReadProperty(nameof(index2));
            reader.Read(out index2);
            reader.ReadMapEnd();
        }
    }

    public struct UpdateGroupMaskInput<T> : IJsonReadable
        where T : struct, IJsonReadable
    {
        public T[] groupMaskArray;
        public Unity.Entities.Entity entity;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(groupMaskArray));
            new ArrayReader<T>().Read(reader, out groupMaskArray);
            reader.ReadProperty(nameof(entity));
            ValueReaders.Create<Unity.Entities.Entity>().Read(reader, out entity);
            reader.ReadMapEnd();
        }
    }

    public struct LookAtInput : IJsonReadable
    {
        public WorldPosition position;
        public float distance;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(position));
            position.Read(reader);
            reader.ReadProperty(nameof(distance));
            reader.Read(out distance);
            reader.ReadMapEnd();
        }
    }

    public struct UpdateStorageInput : IJsonReadable
    {
        public string key;
        public string value;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(key));
            reader.Read(out key);
            reader.ReadProperty(nameof(value));
            reader.Read(out value);
            reader.ReadMapEnd();
        }
    }

    public struct WorldPosition : IJsonReadable, IJsonWritable
    {
        public float x;

        public float y;

        public float z;

        public void Read(IJsonReader reader)
        {
            reader.ReadMapBegin();
            reader.ReadProperty(nameof(x));
            reader.Read(out x);
            reader.ReadProperty(nameof(y));
            reader.Read(out y);
            reader.ReadProperty(nameof(z));
            reader.Read(out z);
            if (reader.ReadProperty(nameof(key)))
            {
                reader.Read(out string _);
            }
            reader.ReadMapEnd();
        }

        public string key
        {
            get => $"{x.ToString("0.0")},{y.ToString("0.0")},{z.ToString("0.0")}";
        }

        public static implicit operator WorldPosition(float pos) =>
            new WorldPosition
            {
                x = pos,
                y = pos,
                z = pos,
            };

        public static implicit operator WorldPosition(Unity.Mathematics.float3 pos) =>
            new WorldPosition
            {
                x = pos.x,
                y = pos.y,
                z = pos.z,
            };

        public static implicit operator Unity.Mathematics.float3(WorldPosition pos) => new Unity.Mathematics.float3(pos.x, pos.y, pos.z);

        public static implicit operator UnityEngine.Vector3(WorldPosition pos) => new UnityEngine.Vector3(pos.x, pos.y, pos.z);

        public static implicit operator string(WorldPosition pos) => pos.key;

        public override bool Equals(object obj)
        {
            if (obj is not WorldPosition)
            {
                return false;
            }
            return Equals((WorldPosition)obj);
        }

        public bool Equals(WorldPosition other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(WorldPosition).FullName);
            writer.PropertyName("x");
            writer.Write(x);
            writer.PropertyName("y");
            writer.Write(y);
            writer.PropertyName("z");
            writer.Write(z);
            writer.TypeEnd();
        }
    }

    public struct ScreenPoint : System.IEquatable<ScreenPoint>, IJsonWritable
    {
        public int top;

        public int left;

        public ScreenPoint(int topPos, int leftPos)
        {
            left = leftPos;
            top = topPos;
        }

        public ScreenPoint(UnityEngine.Vector3 pos, int screenHeight)
        {
            left = (int)pos.x;
            top = (int)(screenHeight - pos.y);
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(ScreenPoint).FullName);
            writer.PropertyName("top");
            writer.Write(top);
            writer.PropertyName("left");
            writer.Write(left);
            writer.TypeEnd();
        }

        public override bool Equals(object obj)
        {
            if (obj is ScreenPoint other)
            {
                return Equals(other);
            }
            return false;
        }

        public bool Equals(ScreenPoint other)
        {
            return other.top == top && other.left == left;
        }

        public override int GetHashCode() => (top, left).GetHashCode();
    }

    public struct ToolTooltipMessage : IJsonWritable
    {
        public string image;

        public string message;

        public ToolTooltipMessage(string image, string message)
        {
            this.image = image;
            this.message = message;
        }

        public void Write(IJsonWriter writer)
        {
            writer.TypeBegin(typeof(ToolTooltipMessage).FullName);
            writer.PropertyName("image");
            writer.Write(image);
            writer.PropertyName("message");
            writer.Write(message);
            writer.TypeEnd();
        }
    }
}
