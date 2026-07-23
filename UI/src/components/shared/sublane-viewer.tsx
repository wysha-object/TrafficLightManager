import { DetailedHTMLProps, useCallback } from 'react'
import styled from 'styled-components'
import LaneItem from '../custom-phase-tool/lane-item'
import {
  SubLaneGroupMask,
  CustomPhaseLaneType,
  CustomPhaseLane,
  CustomPhaseLaneDirection,
  CustomPhaseSignalState,
} from 'types'

const LaneContainer = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: flex-start;
  align-content: center;
`

const Column = styled.div`
  padding: 0rem;
  display: flex;
  flex: 0 1 auto;
`

export interface SublaneViewerDisplayOptions {
  carLane: {
    left: boolean
    straight: boolean
    right: boolean
    uTurn: boolean
  }
  trackLane: {
    left: boolean
    straight: boolean
    right: boolean
  }
  pedestrianLaneStopLine: boolean
}

function GetCustomPhaseLane(
  subLaneGroupMask: SubLaneGroupMask,
  index: number,
  type: CustomPhaseLaneType,
  displayOptions: SublaneViewerDisplayOptions,
): CustomPhaseLane {
  const result: CustomPhaseLane = {
    type: type,
    left: 'stop',
    straight: 'stop',
    right: 'stop',
    uTurn: 'stop',
    all: 'stop',
  }
  if (type == 'carLane') {
    result.left =
      (subLaneGroupMask.m_Car.m_Left.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.left
    result.straight =
      (subLaneGroupMask.m_Car.m_Straight.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.straight
    result.right =
      (subLaneGroupMask.m_Car.m_Right.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.right
    result.uTurn =
      (subLaneGroupMask.m_Car.m_UTurn.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.uTurn
    result.left =
      (subLaneGroupMask.m_Car.m_Left.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.left
    result.straight =
      (subLaneGroupMask.m_Car.m_Straight.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.straight
    result.right =
      (subLaneGroupMask.m_Car.m_Right.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.right
    result.uTurn =
      (subLaneGroupMask.m_Car.m_UTurn.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.uTurn
    result.left = displayOptions.carLane.left ? result.left : 'none'
    result.straight = displayOptions.carLane.straight ? result.straight : 'none'
    result.right = displayOptions.carLane.right ? result.right : 'none'
    result.uTurn = displayOptions.carLane.uTurn ? result.uTurn : 'none'
  }
  if (type == 'trackLane') {
    result.left =
      (subLaneGroupMask.m_Track.m_Left.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.left
    result.straight =
      (subLaneGroupMask.m_Track.m_Straight.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.straight
    result.right =
      (subLaneGroupMask.m_Track.m_Right.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.right
    result.left =
      (subLaneGroupMask.m_Track.m_Left.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.left
    result.straight =
      (subLaneGroupMask.m_Track.m_Straight.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.straight
    result.right =
      (subLaneGroupMask.m_Track.m_Right.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.right
    result.left = displayOptions.trackLane.left ? result.left : 'none'
    result.straight = displayOptions.trackLane.straight
      ? result.straight
      : 'none'
    result.right = displayOptions.trackLane.right ? result.right : 'none'
    result.uTurn = 'none'
  }
  if (type == 'pedestrianLaneStopLine') {
    result.all =
      (subLaneGroupMask.m_Pedestrian.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.all
  }
  return result
}

function SetBit(input: number, index: number, value: number) {
  return (input & ~(1 << index)) | (value << index)
}

export default function SublaneViewer(
  props: {
    subLane: SubLaneGroupMask
    displayOptions: SublaneViewerDisplayOptions
    index: number
    subLaneUpdateHandler?: (newGroupMask: SubLaneGroupMask) => void
  } & DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement>,
) {
  const { subLane, displayOptions, index, subLaneUpdateHandler } = props
  const clickHandler = useCallback(
    (
      index: number,
      type: CustomPhaseLaneType,
      direction: CustomPhaseLaneDirection,
      currentSignal: CustomPhaseSignalState,
    ) => {
      if (subLaneUpdateHandler) {
        let newSignal =
          currentSignal == 'stop'
            ? 'go'
            : currentSignal == 'go'
              ? 'yield'
              : 'stop'
        const newGroupMask: SubLaneGroupMask = JSON.parse(
          JSON.stringify(subLane),
        )
        if (type == 'carLane') {
          if (direction == 'left') {
            if (!displayOptions.carLane.left) {
              newSignal = currentSignal == 'stop' ? 'go' : 'stop'
            }
            newGroupMask.m_Car.m_Left.m_GoGroupMask = SetBit(
              newGroupMask.m_Car.m_Left.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Car.m_Left.m_YieldGroupMask = SetBit(
              newGroupMask.m_Car.m_Left.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'straight') {
            if (!displayOptions.carLane.straight) {
              newSignal = currentSignal == 'stop' ? 'go' : 'stop'
            }
            newGroupMask.m_Car.m_Straight.m_GoGroupMask = SetBit(
              newGroupMask.m_Car.m_Straight.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Car.m_Straight.m_YieldGroupMask = SetBit(
              newGroupMask.m_Car.m_Straight.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'right') {
            if (!displayOptions.carLane.right) {
              newSignal = currentSignal == 'stop' ? 'go' : 'stop'
            }
            newGroupMask.m_Car.m_Right.m_GoGroupMask = SetBit(
              newGroupMask.m_Car.m_Right.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Car.m_Right.m_YieldGroupMask = SetBit(
              newGroupMask.m_Car.m_Right.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'uTurn') {
            newGroupMask.m_Car.m_UTurn.m_GoGroupMask = SetBit(
              newGroupMask.m_Car.m_UTurn.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Car.m_UTurn.m_YieldGroupMask = SetBit(
              newGroupMask.m_Car.m_UTurn.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
        }
        if (type == 'trackLane') {
          newSignal = currentSignal == 'stop' ? 'go' : 'stop'
          if (direction == 'left') {
            newGroupMask.m_Track.m_Left.m_GoGroupMask = SetBit(
              newGroupMask.m_Track.m_Left.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Track.m_Left.m_YieldGroupMask = SetBit(
              newGroupMask.m_Track.m_Left.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'straight') {
            newGroupMask.m_Track.m_Straight.m_GoGroupMask = SetBit(
              newGroupMask.m_Track.m_Straight.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Track.m_Straight.m_YieldGroupMask = SetBit(
              newGroupMask.m_Track.m_Straight.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'right') {
            newGroupMask.m_Track.m_Right.m_GoGroupMask = SetBit(
              newGroupMask.m_Track.m_Right.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_Track.m_Right.m_YieldGroupMask = SetBit(
              newGroupMask.m_Track.m_Right.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
        }
        if (type == 'pedestrianLaneStopLine') {
          newSignal = currentSignal == 'stop' ? 'go' : 'stop'
          newGroupMask.m_Pedestrian.m_GoGroupMask = SetBit(
            newGroupMask.m_Pedestrian.m_GoGroupMask,
            index,
            newSignal != 'stop' ? 1 : 0,
          )
        }
        subLaneUpdateHandler(newGroupMask)
      }
    },
    [subLane, displayOptions, subLaneUpdateHandler],
  )

  const displayCarLane =
    displayOptions.carLane.left ||
    displayOptions.carLane.straight ||
    displayOptions.carLane.right ||
    displayOptions.carLane.uTurn
  const displayTrackLane =
    displayOptions.trackLane.left ||
    displayOptions.trackLane.straight ||
    displayOptions.trackLane.right

  if (
    !displayCarLane &&
    !displayTrackLane &&
    !displayOptions.pedestrianLaneStopLine
  ) {
    return <></>
  }

  return (
    <LaneContainer {...props}>
      {displayCarLane && (
        <>
          <Column>
            <LaneItem
              data={GetCustomPhaseLane(
                subLane,
                index,
                'carLane',
                displayOptions,
              )}
              index={index}
              showIcon={true}
              onClick={clickHandler}
            />
          </Column>
          {(displayTrackLane || displayOptions.pedestrianLaneStopLine) && (
            <div className='vertical-divider-with-gap' />
          )}
        </>
      )}
      {displayTrackLane && (
        <>
          <Column>
            <LaneItem
              data={GetCustomPhaseLane(
                subLane,
                index,
                'trackLane',
                displayOptions,
              )}
              index={index}
              showIcon={true}
              onClick={clickHandler}
            />
          </Column>
          {displayOptions.pedestrianLaneStopLine && (
            <div className='vertical-divider-with-gap' />
          )}
        </>
      )}
      {displayOptions.pedestrianLaneStopLine && (
        <>
          <Column>
            <LaneItem
              data={GetCustomPhaseLane(
                subLane,
                index,
                'pedestrianLaneStopLine',
                displayOptions,
              )}
              index={index}
              showIcon={true}
              onClick={clickHandler}
            />
          </Column>
        </>
      )}
    </LaneContainer>
  )
}
