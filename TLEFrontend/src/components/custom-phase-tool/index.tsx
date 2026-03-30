import { useEffect } from "react";

import { bindValue, call, useValue } from "cs2/api";

import { EdgeGroupMaskOptions } from "@/constants";

import EdgePanel from "./edge-panel";
import SubLanePanel from "./sublane-panel";

export default function CustomPhaseTool() {
  const activeEditingCustomPhaseIndex = useValue(bindValue("C2VM.TLE", "GetActiveEditingCustomPhaseIndex", -1));
  const edgeInfoList = useValue(bindValue<EdgeInfo[]>("C2VM.TLE", "GetEdgeInfo", []));

  useEffect(() => {
    const edgePositionArray = JSON.stringify(edgeInfoList.filter(edge => (edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) == 0).map(item => item.m_Position));
    const subLanePositionArray = JSON.stringify(edgeInfoList.filter(edge => (edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) != 0).map(item => item.m_SubLaneInfoList.map(subLane => subLane.m_Position)).flat());
    call("C2VM.TLE", "CallAddWorldPosition", edgePositionArray);
    call("C2VM.TLE", "CallAddWorldPosition", subLanePositionArray);
    return () => {
      call("C2VM.TLE", "CallRemoveWorldPosition", edgePositionArray);
      call("C2VM.TLE", "CallRemoveWorldPosition", subLanePositionArray);
    };
  }, [edgeInfoList]);

  const screenPointMap = useValue<ScreenPointMap>(bindValue("C2VM.TLE", "GetScreenPoint", {}));

  return (
    <>
      {activeEditingCustomPhaseIndex >= 0 && edgeInfoList.filter(edge => (edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) == 0).map(edge => <EdgePanel data={edge} index={activeEditingCustomPhaseIndex} position={screenPointMap[edge.m_Position.key]} />)}
      {activeEditingCustomPhaseIndex >= 0 && edgeInfoList.filter(edge => (edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) != 0).map(edge => edge.m_SubLaneInfoList.map(subLane => <SubLanePanel edge={edge} subLane={subLane} index={activeEditingCustomPhaseIndex} position={screenPointMap[subLane.m_Position.key]} />).flat())}
    </>
  );
}