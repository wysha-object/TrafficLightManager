import { CityConfigurationContext } from 'context'
import React, { DetailedHTMLProps, useCallback, useContext } from 'react'
import styled from 'styled-components'
import LaneItem from './lane-item'
import {
  EdgeGroupMask,
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

export interface EdgeViewerDisplayOptions {
  carLane: {
    left: boolean
    straight: boolean
    right: boolean
    uTurn: boolean
  }
  publicCarLane: {
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
  pedestrianLaneNonStopLine: boolean
}

function GetCustomPhaseLane(
  edgeGroupMask: EdgeGroupMask,
  index: number,
  type: CustomPhaseLaneType,
  displayOptions: EdgeViewerDisplayOptions,
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
      (edgeGroupMask.m_Car.m_Left.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.left
    result.straight =
      (edgeGroupMask.m_Car.m_Straight.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.straight
    result.right =
      (edgeGroupMask.m_Car.m_Right.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.right
    result.uTurn =
      (edgeGroupMask.m_Car.m_UTurn.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.uTurn
    result.left =
      (edgeGroupMask.m_Car.m_Left.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.left
    result.straight =
      (edgeGroupMask.m_Car.m_Straight.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.straight
    result.right =
      (edgeGroupMask.m_Car.m_Right.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.right
    result.uTurn =
      (edgeGroupMask.m_Car.m_UTurn.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.uTurn
    result.left = displayOptions.carLane.left ? result.left : 'none'
    result.straight = displayOptions.carLane.straight ? result.straight : 'none'
    result.right = displayOptions.carLane.right ? result.right : 'none'
    result.uTurn = displayOptions.carLane.uTurn ? result.uTurn : 'none'
  }
  if (type == 'publicCarLane') {
    result.left =
      (edgeGroupMask.m_PublicCar.m_Left.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.left
    result.straight =
      (edgeGroupMask.m_PublicCar.m_Straight.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.straight
    result.right =
      (edgeGroupMask.m_PublicCar.m_Right.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.right
    result.uTurn =
      (edgeGroupMask.m_PublicCar.m_UTurn.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.uTurn
    result.left =
      (edgeGroupMask.m_PublicCar.m_Left.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.left
    result.straight =
      (edgeGroupMask.m_PublicCar.m_Straight.m_YieldGroupMask & (1 << index)) !=
      0
        ? 'yield'
        : result.straight
    result.right =
      (edgeGroupMask.m_PublicCar.m_Right.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.right
    result.uTurn =
      (edgeGroupMask.m_PublicCar.m_UTurn.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.uTurn
    result.left = displayOptions.publicCarLane.left ? result.left : 'none'
    result.straight = displayOptions.publicCarLane.straight
      ? result.straight
      : 'none'
    result.right = displayOptions.publicCarLane.right ? result.right : 'none'
    result.uTurn = displayOptions.publicCarLane.uTurn ? result.uTurn : 'none'
  }
  if (type == 'trackLane') {
    result.left =
      (edgeGroupMask.m_Track.m_Left.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.left
    result.straight =
      (edgeGroupMask.m_Track.m_Straight.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.straight
    result.right =
      (edgeGroupMask.m_Track.m_Right.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.right
    result.left =
      (edgeGroupMask.m_Track.m_Left.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.left
    result.straight =
      (edgeGroupMask.m_Track.m_Straight.m_YieldGroupMask & (1 << index)) != 0
        ? 'yield'
        : result.straight
    result.right =
      (edgeGroupMask.m_Track.m_Right.m_YieldGroupMask & (1 << index)) != 0
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
      (edgeGroupMask.m_PedestrianStopLine.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.all
  }
  if (type == 'pedestrianLaneNonStopLine') {
    result.all =
      (edgeGroupMask.m_PedestrianNonStopLine.m_GoGroupMask & (1 << index)) != 0
        ? 'go'
        : result.all
  }
  return result
}

function SetBit(input: number, index: number, value: number) {
  return (input & ~(1 << index)) | (value << index)
}

export default function EdgeViewer(
  props: {
    data: EdgeGroupMask
    displayOptions: EdgeViewerDisplayOptions
    index: number
    edgeUpdateHandler?: (newGroupMask: EdgeGroupMask) => void
  } & DetailedHTMLProps<React.HTMLAttributes<HTMLDivElement>, HTMLDivElement>,
) {
  const { data, displayOptions, index, edgeUpdateHandler } = props
  const clickHandler = useCallback(
    (
      index: number,
      type: CustomPhaseLaneType,
      direction: CustomPhaseLaneDirection,
      currentSignal: CustomPhaseSignalState,
    ) => {
      if (edgeUpdateHandler) {
        let newSignal =
          currentSignal == 'stop'
            ? 'go'
            : currentSignal == 'go'
              ? 'yield'
              : 'stop'
        const newGroupMask: EdgeGroupMask = JSON.parse(JSON.stringify(data))
        if (type == 'carLane') {
          if (direction == 'left') {
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
        if (type == 'publicCarLane') {
          if (direction == 'left') {
            newGroupMask.m_PublicCar.m_Left.m_GoGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_Left.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_PublicCar.m_Left.m_YieldGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_Left.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'straight') {
            newGroupMask.m_PublicCar.m_Straight.m_GoGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_Straight.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_PublicCar.m_Straight.m_YieldGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_Straight.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'right') {
            newGroupMask.m_PublicCar.m_Right.m_GoGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_Right.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_PublicCar.m_Right.m_YieldGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_Right.m_YieldGroupMask,
              index,
              newSignal == 'yield' ? 1 : 0,
            )
          }
          if (direction == 'uTurn') {
            newGroupMask.m_PublicCar.m_UTurn.m_GoGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_UTurn.m_GoGroupMask,
              index,
              newSignal != 'stop' ? 1 : 0,
            )
            newGroupMask.m_PublicCar.m_UTurn.m_YieldGroupMask = SetBit(
              newGroupMask.m_PublicCar.m_UTurn.m_YieldGroupMask,
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
          newGroupMask.m_PedestrianStopLine.m_GoGroupMask = SetBit(
            newGroupMask.m_PedestrianStopLine.m_GoGroupMask,
            index,
            newSignal != 'stop' ? 1 : 0,
          )
        }
        if (type == 'pedestrianLaneNonStopLine') {
          newSignal = currentSignal == 'stop' ? 'go' : 'stop'
          newGroupMask.m_PedestrianNonStopLine.m_GoGroupMask = SetBit(
            newGroupMask.m_PedestrianNonStopLine.m_GoGroupMask,
            index,
            newSignal != 'stop' ? 1 : 0,
          )
        }
        edgeUpdateHandler(newGroupMask)
      }
    },
    [data],
  )

  const cityConfiguration = useContext(CityConfigurationContext)
  const displayCarLane =
    displayOptions.carLane.left ||
    displayOptions.carLane.straight ||
    displayOptions.carLane.right ||
    displayOptions.carLane.uTurn
  const displayPublicCarLane =
    displayOptions.publicCarLane.left ||
    displayOptions.publicCarLane.straight ||
    displayOptions.publicCarLane.right ||
    displayOptions.publicCarLane.uTurn
  const displayTrackLane =
    displayOptions.trackLane.left ||
    displayOptions.trackLane.straight ||
    displayOptions.trackLane.right

  if (
    !displayCarLane &&
    !displayPublicCarLane &&
    !displayTrackLane &&
    !displayOptions.pedestrianLaneStopLine &&
    !displayOptions.pedestrianLaneNonStopLine
  ) {
    return <></>
  }

  return (
    <LaneContainer {...props}>
      {displayCarLane && (
        <>
          <Column>
            <LaneItem
              data={GetCustomPhaseLane(data, index, 'carLane', displayOptions)}
              index={index}
              showIcon={true}
              onClick={clickHandler}
            />
          </Column>
          {(displayPublicCarLane ||
            displayTrackLane ||
            displayOptions.pedestrianLaneStopLine ||
            displayOptions.pedestrianLaneNonStopLine) && (
            <div className='vertical-divider-with-gap' />
          )}
        </>
      )}
      {displayPublicCarLane && (
        <>
          <Column>
            <LaneItem
              data={GetCustomPhaseLane(
                data,
                index,
                'publicCarLane',
                displayOptions,
              )}
              index={index}
              showIcon={true}
              onClick={clickHandler}
            />
          </Column>
          {(displayTrackLane ||
            displayOptions.pedestrianLaneStopLine ||
            displayOptions.pedestrianLaneNonStopLine) && (
            <div className='vertical-divider-with-gap' />
          )}
        </>
      )}
      {displayTrackLane && (
        <>
          <Column>
            <LaneItem
              data={GetCustomPhaseLane(
                data,
                index,
                'trackLane',
                displayOptions,
              )}
              index={index}
              showIcon={true}
              onClick={clickHandler}
            />
          </Column>
          {(displayOptions.pedestrianLaneStopLine ||
            displayOptions.pedestrianLaneNonStopLine) && (
            <div className='vertical-divider-with-gap' />
          )}
        </>
      )}
      {cityConfiguration.leftHandTraffic && (
        <>
          {displayOptions.pedestrianLaneStopLine && (
            <>
              <Column>
                <LaneItem
                  data={GetCustomPhaseLane(
                    data,
                    index,
                    'pedestrianLaneStopLine',
                    displayOptions,
                  )}
                  index={index}
                  showIcon={true}
                  onClick={clickHandler}
                />
              </Column>
              {displayOptions.pedestrianLaneNonStopLine && (
                <div className='vertical-divider-with-gap' />
              )}
            </>
          )}
          {displayOptions.pedestrianLaneNonStopLine && (
            <>
              <Column>
                <LaneItem
                  data={GetCustomPhaseLane(
                    data,
                    index,
                    'pedestrianLaneNonStopLine',
                    displayOptions,
                  )}
                  index={index}
                  showIcon={true}
                  onClick={clickHandler}
                />
              </Column>
            </>
          )}
        </>
      )}
      {!cityConfiguration.leftHandTraffic && (
        <>
          {displayOptions.pedestrianLaneNonStopLine && (
            <>
              <Column>
                <LaneItem
                  data={GetCustomPhaseLane(
                    data,
                    index,
                    'pedestrianLaneNonStopLine',
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
                    data,
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
        </>
      )}
    </LaneContainer>
  )
}
