import { useEffect } from "react";

import { bindValue, call, useValue } from "cs2/api";

import { EdgeGroupMaskOptions } from "@/constants";

import EdgePanel from "./edge-panel";
import SubLanePanel from "./sublane-panel";
import ClipboardPanel from "./clipboard-panel";

export default function CustomPhaseTool() {
  const editingPhaseIndex: number = useValue(bindValue("TrafficLightManager", "GetEditingPhaseIndex"));
  const edgeInfoList: EdgeInfo[] = useValue(bindValue("TrafficLightManager", "GetEdgeInfo"));

  useEffect(() => {
    const edgePositionArray = JSON.stringify(edgeInfoList.filter(edge => (edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) == 0).map(item => item.m_Position));
    const subLanePositionArray = JSON.stringify(edgeInfoList.filter(edge => (edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) != 0).map(item => item.m_SubLaneInfoList.map(subLane => subLane.m_Position)).flat());
    call("TrafficLightManager", "CallAddWorldPosition", edgePositionArray);
    call("TrafficLightManager", "CallAddWorldPosition", subLanePositionArray);
    return () => {
      call("TrafficLightManager", "CallRemoveWorldPosition", edgePositionArray);
      call("TrafficLightManager", "CallRemoveWorldPosition", subLanePositionArray);
    };
  }, [edgeInfoList]);

  const screenPointMap = useValue<ScreenPointMap>(bindValue("TrafficLightManager", "GetScreenPoint", {}));

  return (
    <>
      {
        editingPhaseIndex >= 0 &&
        <>
          {
            edgeInfoList.filter(
              edge => (
                edge.m_EdgeGroupMask.m_Options &
                EdgeGroupMaskOptions.PerLaneSignal
              ) == 0
            ).map(
              edge => <EdgePanel
                data={edge}
                index={editingPhaseIndex}
                position={screenPointMap[edge.m_Position.key]}
              />
            )
          }
          {
            edgeInfoList.filter(
              edge => (
                edge.m_EdgeGroupMask.m_Options &
                EdgeGroupMaskOptions.PerLaneSignal) != 0
            ).map(
              edge => edge.m_SubLaneInfoList.map(
                subLane => <SubLanePanel
                  edge={edge}
                  subLane={subLane}
                  index={editingPhaseIndex}
                  position={screenPointMap[subLane.m_Position.key]}
                />
              ).flat()
            )
          }
        </>
      }
    </>
  );
}