import { useContext, useEffect, useState } from 'react'
import EdgePanel from '../components/custom-phase-tool/edge-panel'
import SubLanePanel from '../components/custom-phase-tool/sublane-panel'
import { EdgeGroupMaskOptions, ToolState } from 'types'
import { CurrentFocusPhaseIndexContext } from 'context'
import {
  useGetCameraCmd,
  useGetEdgeInfoCmd,
  useGetToolStateCmd,
  worldToScreenPoint,
} from 'hooks/cmds'

export default function CustomPhaseTool() {
  const [currrentFocusPhaseIndex] = useContext(CurrentFocusPhaseIndexContext)
  const edgeInfoList = useGetEdgeInfoCmd()
  const toolState = useGetToolStateCmd()
  const camera = useGetCameraCmd()

  const [screenPointMap, setScreenPointMap] = useState(new Map<string, { top: number; left: number }>())
  useEffect(() => {
    let cancelled = false
    const fetch = async () => {
      const map = new Map<string, { top: number; left: number }>()
      for (const edge of edgeInfoList) {
        if ((edge.m_EdgeGroupMask.m_Options & EdgeGroupMaskOptions.PerLaneSignal) == 0) {
          const screenPoint = await worldToScreenPoint(edge.m_Position)
          map.set(JSON.stringify(edge.m_Position), screenPoint)
        } else {
          for (const subLane of edge.m_SubLaneInfoList) {
            const screenPoint = await worldToScreenPoint(subLane.m_Position)
            map.set(JSON.stringify(subLane.m_Position), screenPoint)
          }
        }
      }
      if (cancelled) return
      setScreenPointMap(map)
    }
    fetch()
    return () => {
      cancelled = true
    }
  }, [edgeInfoList, JSON.stringify(camera)]);

  return (
    <>
      {toolState === ToolState.Editing && (
        <>
          {edgeInfoList
            .filter(
              (edge) =>
                (edge.m_EdgeGroupMask.m_Options &
                  EdgeGroupMaskOptions.PerLaneSignal) ==
                0,
            )
            .map((edge) => (
              <EdgePanel
                key={`${edge.m_Position.x}-${edge.m_Position.y}-${edge.m_Position.z}`}
                data={edge}
                index={currrentFocusPhaseIndex}
                position={screenPointMap.get(JSON.stringify(edge.m_Position)) ?? { top: 0, left: 0 }}
              />
            ))}
          {edgeInfoList
            .filter(
              (edge) =>
                (edge.m_EdgeGroupMask.m_Options &
                  EdgeGroupMaskOptions.PerLaneSignal) !=
                0,
            )
            .map((edge) =>
              edge.m_SubLaneInfoList
                .map((subLane) => (
                  <SubLanePanel
                    key={`${subLane.m_Position.x}-${subLane.m_Position.y}-${subLane.m_Position.z}`}
                    edge={edge}
                    subLane={subLane}
                    index={currrentFocusPhaseIndex}
                    position={screenPointMap.get(JSON.stringify(subLane.m_Position)) ?? { top: 0, left: 0 }}
                  />
                ))
                .flat(),
            )}
        </>
      )}
    </>
  )
}
