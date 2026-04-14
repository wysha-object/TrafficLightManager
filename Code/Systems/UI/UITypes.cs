using System.ComponentModel;
using Colossal.UI.Binding;
using Newtonsoft.Json;

namespace TrafficLightManager.Code.Systems.UI;

public static class UITypes
{
    public struct CustomPhaseItem
    {
        public uint timer;

        public ushort turnsSinceLastRun;

        public ushort lowFlowTimer;

        public float carFlow;

        public ushort carLaneOccupied;

        public ushort publicCarLaneOccupied;

        public ushort trackLaneOccupied;

        public ushort pedestrianLaneOccupied;

        public float weightedWaiting;

        public float targetDuration;

        public int priority;

        public ushort minimumDuration;

        public ushort maximumDuration;

        public float targetDurationMultiplier;

        public float laneOccupiedMultiplier;

        public float intervalExponent;

        public bool prioritiseTrack;

        public bool prioritisePublicCar;

        public bool prioritisePedestrian;

        public bool linkedWithNextPhase;

        public bool endPhasePrematurely;
    }

    public struct UpdateCustomPhaseData
    {
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int index;

        public string key;

        public double value;
    }

    public struct WorldPosition : IJsonWritable
    {
        public float x;

        public float y;

        public float z;

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
            writer.PropertyName("key");
            writer.Write(key);
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
