import { useContext, useEffect } from 'react'
import { removeWorldPosition } from 'hooks/cmds'
import EdgePanel from '../components/custom-phase-tool/edge-panel'
import SubLanePanel from '../components/custom-phase-tool/sublane-panel'
import { EdgeGroupMaskOptions, ToolState } from 'types'
import { CurrentFocusPhaseIndexContext } from 'context'
import {
  addWorldPosition,
  useGetEdgeInfoCmd,
  useGetScreenPointCmd,
  useGetToolStateCmd,
} from 'hooks/cmds'

export default function CustomPhaseTool() {
  const [currrentFocusPhaseIndex] = useContext(CurrentFocusPhaseIndexContext)
  const edgeInfoList = useGetEdgeInfoCmd()
  const toolState = useGetToolStateCmd()

  useEffect(() => {
    const edgePositionArray = edgeInfoList
      .filter(
        (edge) =>
          (edge.m_EdgeGroupMask.m_Options &
            EdgeGroupMaskOptions.PerLaneSignal) ==
          0,
      )
      .map((item) => item.m_Position)
    const subLanePositionArray = edgeInfoList
      .filter(
        (edge) =>
          (edge.m_EdgeGroupMask.m_Options &
            EdgeGroupMaskOptions.PerLaneSignal) !=
          0,
      )
      .map((item) =>
        item.m_SubLaneInfoList.map((subLane) => subLane.m_Position),
      )
      .flat()
    addWorldPosition(edgePositionArray)
    addWorldPosition(subLanePositionArray)
    return () => {
      removeWorldPosition(edgePositionArray)
      removeWorldPosition(subLanePositionArray)
    }
  }, [edgeInfoList])

  const screenPointMap = useGetScreenPointCmd()

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
                data={edge}
                index={currrentFocusPhaseIndex}
                position={screenPointMap[edge.m_Position.key]}
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
                    edge={edge}
                    subLane={subLane}
                    index={currrentFocusPhaseIndex}
                    position={screenPointMap[subLane.m_Position.key]}
                  />
                ))
                .flat(),
            )}
        </>
      )}
    </>
  )
}
