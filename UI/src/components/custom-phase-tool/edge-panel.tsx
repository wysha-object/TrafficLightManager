import LinkVariantOffSvg from 'assets/images/link-variant-off.svg'
import CopySvg from 'assets/images/copy.svg'
import PasteSvg from 'assets/images/paste.svg'
import { useCallback, useContext, useEffect, useRef } from 'react'
import styled from 'styled-components'
import { EdgeGroupMaskContextClipboard } from 'context'
import EdgeViewer from 'components/shared/edge-viewer'
import {
  EdgeGroupMask,
  EdgeGroupMaskOptions,
  EdgeInfo,
  ScreenPoint,
} from 'types'
import { Button } from 'cs2/ui'
import { updateEdgeGroupMaskCmd } from 'hooks/cmds'

const Container = styled.div`
  position: fixed;
  transform: translate(-50%);
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

export default function EdgePanel(props: {
  data: EdgeInfo
  index: number
  position: ScreenPoint
}) {
  const { data, index, position } = props
  const clipboard = useContext(EdgeGroupMaskContextClipboard.context)

  const unlinkHandler = useCallback(() => {
    const newGroupMask: EdgeGroupMask = JSON.parse(
      JSON.stringify(props.data.m_EdgeGroupMask),
    )
    newGroupMask.m_Options |= EdgeGroupMaskOptions.PerLaneSignal
    updateEdgeGroupMaskCmd([newGroupMask], props.data.m_TrafficLightsEntity)
  }, [props.data.m_EdgeGroupMask])

  const carLaneCount =
    props.data.m_CarLaneLeftCount +
    props.data.m_CarLaneStraightCount +
    props.data.m_CarLaneRightCount +
    props.data.m_CarLaneUTurnCount
  const publicCarLaneCount =
    props.data.m_PublicCarLaneLeftCount +
    props.data.m_PublicCarLaneStraightCount +
    props.data.m_PublicCarLaneRightCount +
    props.data.m_PublicCarLaneUTurnCount
  const trackLaneCount =
    props.data.m_TrackLaneLeftCount +
    props.data.m_TrackLaneStraightCount +
    props.data.m_TrackLaneRightCount
  const containerRef = useRef(null)

  useEffect(() => {
    if (containerRef != null && containerRef.current != null && position) {
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.leftPX = position.left
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.topPX = position.top
    }
  }, [containerRef, position])

  if (
    carLaneCount +
      publicCarLaneCount +
      trackLaneCount +
      data.m_PedestrianLaneStopLineCount +
      data.m_PedestrianLaneNonStopLineCount <=
    0
  ) {
    return <></>
  }

  return (
    <Container ref={containerRef}>
      <Content>
        <div>
          <EdgeViewer
            data={data.m_EdgeGroupMask}
            displayOptions={{
              carLane: {
                left: data.m_CarLaneLeftCount > 0,
                straight: data.m_CarLaneStraightCount > 0,
                right: data.m_CarLaneRightCount > 0,
                uTurn: data.m_CarLaneUTurnCount > 0,
              },
              publicCarLane: {
                left: data.m_PublicCarLaneLeftCount > 0,
                straight: data.m_PublicCarLaneStraightCount > 0,
                right: data.m_PublicCarLaneRightCount > 0,
                uTurn: data.m_PublicCarLaneUTurnCount > 0,
              },
              trackLane: {
                left: data.m_TrackLaneLeftCount > 0,
                straight: data.m_TrackLaneStraightCount > 0,
                right: data.m_TrackLaneRightCount > 0,
              },
              pedestrianLaneStopLine: data.m_PedestrianLaneStopLineCount > 0,
              pedestrianLaneNonStopLine:
                data.m_PedestrianLaneNonStopLineCount > 0,
            }}
            index={index}
            edgeUpdateHandler={(newGroupMask) =>
              updateEdgeGroupMaskCmd([newGroupMask], data.m_TrafficLightsEntity)
            }
          />
        </div>
        <HorizontalDivider />
        <LinkVariantOffSvg className='big-icon' onClick={unlinkHandler} />
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
              clipboard.save(data.m_EdgeGroupMask)
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
                const newGroupMask: EdgeGroupMask = JSON.parse(
                  JSON.stringify(data.m_EdgeGroupMask),
                )
                newGroupMask.m_Car = selectedItem.value.m_Car
                newGroupMask.m_PublicCar = selectedItem.value.m_PublicCar
                newGroupMask.m_Track = selectedItem.value.m_Track
                newGroupMask.m_PedestrianStopLine =
                  selectedItem.value.m_PedestrianStopLine
                newGroupMask.m_PedestrianNonStopLine =
                  selectedItem.value.m_PedestrianNonStopLine
                updateEdgeGroupMaskCmd(
                  [newGroupMask],
                  data.m_TrafficLightsEntity,
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
