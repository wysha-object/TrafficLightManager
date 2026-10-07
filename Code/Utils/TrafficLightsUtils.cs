using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using TrafficLightManager.Code.Components;

namespace TrafficLightManager.Code.Utils
{
    public static class TrafficLightsUtils
    {
        public static void UpdateLaneSignal(TrafficLights trafficLights, ref LaneSignal laneSignal)
        {
            ExtraLaneSignal extraLaneSignal = new();
            UpdateLaneSignal(trafficLights, ref laneSignal, ref extraLaneSignal);
        }

        public static void UpdateLaneSignal(TrafficLights trafficLights, ref LaneSignal laneSignal, ref ExtraLaneSignal extraLaneSignal)
        {
            int num = 0;
            int num2 = 0;
            if (trafficLights.m_CurrentSignalGroup > 0)
            {
                num |= 1 << trafficLights.m_CurrentSignalGroup - 1;
            }

            if (trafficLights.m_NextSignalGroup > 0)
            {
                num2 |= 1 << trafficLights.m_NextSignalGroup - 1;
            }

            switch (trafficLights.m_State)
            {
                case Game.Net.TrafficLightState.Beginning:
                    if ((laneSignal.m_GroupMask & num2) != 0)
                    {
                        if (laneSignal.m_Signal != LaneSignalType.Go)
                        {
                            laneSignal.m_Signal = LaneSignalType.Yield;
                        }
                    }
                    else
                    {
                        laneSignal.m_Signal = LaneSignalType.Stop;
                    }

                    break;
                case Game.Net.TrafficLightState.Ongoing:
                    if ((laneSignal.m_GroupMask & num) != 0)
                    {
                        laneSignal.m_Signal = LaneSignalType.Go;
                    }
                    else
                    {
                        laneSignal.m_Signal = LaneSignalType.Stop;
                    }

                    break;
                case Game.Net.TrafficLightState.Extending:
                    if ((laneSignal.m_Flags & LaneSignalFlags.CanExtend) != 0)
                    {
                        if ((laneSignal.m_GroupMask & num) != 0)
                        {
                            laneSignal.m_Signal = LaneSignalType.Go;
                        }
                        else
                        {
                            laneSignal.m_Signal = LaneSignalType.Stop;
                        }
                    }
                    else if (laneSignal.m_Signal == LaneSignalType.Go)
                    {
                        if ((laneSignal.m_GroupMask & num2) == 0)
                        {
                            laneSignal.m_Signal = LaneSignalType.SafeStop;
                        }
                    }
                    else
                    {
                        laneSignal.m_Signal = LaneSignalType.Stop;
                    }

                    break;
                case Game.Net.TrafficLightState.Extended:
                    if ((laneSignal.m_Flags & LaneSignalFlags.CanExtend) != 0 && (laneSignal.m_GroupMask & num) != 0)
                    {
                        laneSignal.m_Signal = LaneSignalType.Go;
                    }
                    else
                    {
                        laneSignal.m_Signal = LaneSignalType.Stop;
                    }

                    break;
                case Game.Net.TrafficLightState.Ending:
                    if (laneSignal.m_Signal == LaneSignalType.Go)
                    {
                        if ((laneSignal.m_GroupMask & num2) == 0)
                        {
                            laneSignal.m_Signal = LaneSignalType.SafeStop;
                        }
                    }
                    else
                    {
                        laneSignal.m_Signal = LaneSignalType.Stop;
                    }

                    break;
                case Game.Net.TrafficLightState.Changing:
                    if (laneSignal.m_Signal != LaneSignalType.Go || (laneSignal.m_GroupMask & num2) == 0)
                    {
                        laneSignal.m_Signal = LaneSignalType.Stop;
                    }

                    break;
                default:
                    laneSignal.m_Signal = LaneSignalType.None;
                    break;
            }
        }
    }
}
