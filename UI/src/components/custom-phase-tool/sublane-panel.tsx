import LinkVariantSvg from 'assets/images/link-variant.svg'
import CopySvg from '../../assets/images/copy.svg'
import PasteSvg from '../../assets/images/paste.svg'
import { useCallback, useContext, useEffect, useRef } from 'react'
import styled from 'styled-components'
import { updateEdgeGroupMaskCmd, updateSubLaneGroupMaskCmd } from 'hooks/cmds'
import { SubLaneGroupMaskContextClipboard } from 'context'
import SublaneViewer from '../shared/sublane-viewer'
import {
  EdgeGroupMask,
  EdgeGroupMaskOptions,
  EdgeInfo,
  ScreenPoint,
  SubLaneGroupMask,
  SubLaneInfo,
} from 'types'
import { Button } from 'cs2/ui'

const Container = styled.div<{ translateY: string }>`
  position: fixed;
  transform: translate(-50%, ${(props) => props.translateY});
  margin: -10rem 0 0 -10rem;
  z-index: -1;
`

const Content = styled.div`
  border-radius: 4rem;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 6rem;
  display: flex;
  flex-direction: column;
  align-items: center;
`

const HorizontalDivider = styled.div`
  height: 2rem;
  width: 100%;
  border: 2rem solid rgba(255, 255, 255, 0.1);
  margin: 6rem -6rem;
`

export default function SubLanePanel(props: {
  edge: EdgeInfo
  subLane: SubLaneInfo
  index: number
  position: ScreenPoint
}) {
  const clipboard = useContext(SubLaneGroupMaskContextClipboard.context)

  const linkHandler = useCallback(() => {
    const newGroupMask: EdgeGroupMask = JSON.parse(
      JSON.stringify(props.edge.m_EdgeGroupMask),
    )
    newGroupMask.m_Options &= ~EdgeGroupMaskOptions.PerLaneSignal
    updateEdgeGroupMaskCmd([newGroupMask], props.edge.m_TrafficLightsEntity)
  }, [props.edge.m_EdgeGroupMask])

  const carLaneCount =
    props.subLane.m_CarLaneLeftCount +
    props.subLane.m_CarLaneStraightCount +
    props.subLane.m_CarLaneRightCount +
    props.subLane.m_CarLaneUTurnCount
  const trackLaneCount =
    props.subLane.m_TrackLaneLeftCount +
    props.subLane.m_TrackLaneStraightCount +
    props.subLane.m_TrackLaneRightCount
  const containerRef = useRef(null)

  useEffect(() => {
    if (
      containerRef != null &&
      containerRef.current != null &&
      props.position
    ) {
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.leftPX = props.position.left
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.topPX = props.position.top
    }
  }, [containerRef, props.position])

  return (
    <Container
      ref={containerRef}
      translateY={carLaneCount + trackLaneCount > 0 ? '0' : '-100%'}
    >
      <Content>
        <div>
          <SublaneViewer
            subLane={props.subLane.m_SubLaneGroupMask}
            displayOptions={{
              carLane: {
                left: props.subLane.m_CarLaneLeftCount > 0,
                straight: props.subLane.m_CarLaneStraightCount > 0,
                right: props.subLane.m_CarLaneRightCount > 0,
                uTurn: props.subLane.m_CarLaneUTurnCount > 0,
              },
              trackLane: {
                left: props.subLane.m_TrackLaneLeftCount > 0,
                straight: props.subLane.m_TrackLaneStraightCount > 0,
                right: props.subLane.m_TrackLaneRightCount > 0,
              },
              pedestrianLaneStopLine: props.subLane.m_PedestrianLaneCount > 0,
            }}
            index={props.index}
            subLaneUpdateHandler={(newGroupMask) =>
              updateSubLaneGroupMaskCmd(
                [newGroupMask],
                props.edge.m_TrafficLightsEntity,
              )
            }
          />
        </div>
        <HorizontalDivider />
        <Button variant='round' onClick={linkHandler}>
          <LinkVariantSvg className='big-icon' />
        </Button>
        <div
          style={{
            display: 'flex',
            flexDirection: 'row',
            justifyContent: 'center',
            alignItems: 'center',
          }}
        >
          <Button
            variant='round'
            onClick={() => {
              clipboard.save(props.subLane.m_SubLaneGroupMask)
            }}
          >
            <CopySvg />
          </Button>
          <Button
            variant='round'
            disabled={clipboard.selectedIndex < 0}
            onClick={() => {
              if (clipboard.selectedIndex >= 0) {
                const selectedItem = clipboard.history[clipboard.selectedIndex]
                const newGroupMask: SubLaneGroupMask = JSON.parse(
                  JSON.stringify(props.subLane.m_SubLaneGroupMask),
                )
                newGroupMask.m_Car = selectedItem.value.m_Car
                newGroupMask.m_Track = selectedItem.value.m_Track
                newGroupMask.m_Pedestrian = selectedItem.value.m_Pedestrian
                updateSubLaneGroupMaskCmd(
                  [newGroupMask],
                  props.edge.m_TrafficLightsEntity,
                )
              }
            }}
          >
            <PasteSvg />
          </Button>
        </div>
      </Content>
    </Container>
  )
}
