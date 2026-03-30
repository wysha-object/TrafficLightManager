import { useCallback, useEffect, useRef } from 'react';
import styled from 'styled-components';

import { call } from 'cs2/api';

import { EdgeGroupMaskOptions } from "@/constants";

import Lane from './lane';
import LinkVariant from '@/components/common/icons/link-variant';

const Container = styled.div<{translateY: string}>`
  position: fixed;
  transform: translate(-50%, ${props => props.translateY});
  margin: -10rem 0 0 -10rem;
  z-index: -1;
`;

const Content = styled.div`
  border-radius: 4rem;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 6rem;
`;

const LaneContainer = styled.div`
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: flex-start;
  align-content: center;
  margin: 0 0 6rem 0;
`;

const Column = styled.div`
  padding: 0rem;
  display: flex;
  flex: 0 1 auto;
`;

const Divider = styled.div`
  height: 100%;
  width: 0rem;
  border: 1rem solid rgba(255, 255, 255, 0.1);
  margin: 0 6rem;
`;

const HorizontalDivider = styled.div`
  height: 2px;
  width: auto;
  border: 2px solid rgba(255, 255, 255, 0.1);
  margin: 6rem -6rem;
`;

const IconContainer = styled.div`
  &:hover {
    filter: brightness(1.2) contrast(1.2);
  }
`;

const IconStyle = {
  color: "var(--textColorDim)",
  width: "24rem",
  height: "24rem",
  margin: "3rem"
};

function GetCustomPhaseLane(subLane: SubLaneInfo, index: number, type: CustomPhaseLaneType): CustomPhaseLane {
  const result: CustomPhaseLane = {
    type: type,
    left: "stop",
    straight: "stop",
    right: "stop",
    uTurn: "stop",
    all: "stop"
  }
  if (type == "carLane") {
    result.left = (subLane.m_SubLaneGroupMask.m_Car.m_Left.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.left;
    result.straight = (subLane.m_SubLaneGroupMask.m_Car.m_Straight.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.straight;
    result.right = (subLane.m_SubLaneGroupMask.m_Car.m_Right.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.right;
    result.uTurn = (subLane.m_SubLaneGroupMask.m_Car.m_UTurn.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.uTurn;
    result.left = (subLane.m_SubLaneGroupMask.m_Car.m_Left.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.left;
    result.straight = (subLane.m_SubLaneGroupMask.m_Car.m_Straight.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.straight;
    result.right = (subLane.m_SubLaneGroupMask.m_Car.m_Right.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.right;
    result.uTurn = (subLane.m_SubLaneGroupMask.m_Car.m_UTurn.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.uTurn;
    result.left = subLane.m_CarLaneLeftCount + subLane.m_TrackLaneLeftCount <= 0 ? "none" : result.left;
    result.straight = subLane.m_CarLaneStraightCount + subLane.m_TrackLaneStraightCount <= 0 ? "none" : result.straight;
    result.right = subLane.m_CarLaneRightCount + subLane.m_TrackLaneRightCount <= 0 ? "none" : result.right;
    result.uTurn = subLane.m_CarLaneUTurnCount <= 0 ? "none" : result.uTurn;
  }
  if (type == "trackLane") {
    result.left = (subLane.m_SubLaneGroupMask.m_Track.m_Left.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.left;
    result.straight = (subLane.m_SubLaneGroupMask.m_Track.m_Straight.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.straight;
    result.right = (subLane.m_SubLaneGroupMask.m_Track.m_Right.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.right;
    result.left = (subLane.m_SubLaneGroupMask.m_Track.m_Left.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.left;
    result.straight = (subLane.m_SubLaneGroupMask.m_Track.m_Straight.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.straight;
    result.right = (subLane.m_SubLaneGroupMask.m_Track.m_Right.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.right;
    result.left = subLane.m_TrackLaneLeftCount <= 0 ? "none" : result.left;
    result.straight = subLane.m_TrackLaneStraightCount <= 0 ? "none" : result.straight;
    result.right = subLane.m_TrackLaneRightCount <= 0 ? "none" : result.right;
    result.uTurn = "none";
  }
  if (type == "pedestrianLaneStopLine") {
    result.all = (subLane.m_SubLaneGroupMask.m_Pedestrian.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.all;
  }
  return result;
}

function SetBit(input: number, index: number, value: number) {
    return ((input & (~(1 << index))) | (value << index));
}

export default function SubLanePanel(props: {edge: EdgeInfo, subLane: SubLaneInfo, index: number, position: ScreenPoint}) {
  const clickHandler = useCallback((index: number, type: CustomPhaseLaneType, direction: CustomPhaseLaneDirection, currentSignal: CustomPhaseSignalState) => {
    let newSignal = currentSignal == "stop" ? "go" : (currentSignal == "go" ? "yield" : "stop");
    const newGroupMask: SubLaneGroupMask = JSON.parse(JSON.stringify(props.subLane.m_SubLaneGroupMask));
    if (type == "carLane") {
      if (direction == "left") {
        if (props.subLane.m_CarLaneLeftCount == 0) {
          newSignal = currentSignal == "stop" ? "go" : "stop";
        }
        newGroupMask.m_Car.m_Left.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_Left.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_Left.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_Left.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "straight") {
        if (props.subLane.m_CarLaneStraightCount == 0) {
          newSignal = currentSignal == "stop" ? "go" : "stop";
        }
        newGroupMask.m_Car.m_Straight.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_Straight.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_Straight.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_Straight.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "right") {
        if (props.subLane.m_CarLaneRightCount == 0) {
          newSignal = currentSignal == "stop" ? "go" : "stop";
        }
        newGroupMask.m_Car.m_Right.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_Right.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_Right.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_Right.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "uTurn") {
        newGroupMask.m_Car.m_UTurn.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_UTurn.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_UTurn.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_UTurn.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
    }
    if (type == "trackLane") {
      newSignal = currentSignal == "stop" ? "go" : "stop";
      if (direction == "left") {
        newGroupMask.m_Track.m_Left.m_GoGroupMask = SetBit(newGroupMask.m_Track.m_Left.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Track.m_Left.m_YieldGroupMask = SetBit(newGroupMask.m_Track.m_Left.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "straight") {
        newGroupMask.m_Track.m_Straight.m_GoGroupMask = SetBit(newGroupMask.m_Track.m_Straight.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Track.m_Straight.m_YieldGroupMask = SetBit(newGroupMask.m_Track.m_Straight.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "right") {
        newGroupMask.m_Track.m_Right.m_GoGroupMask = SetBit(newGroupMask.m_Track.m_Right.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Track.m_Right.m_YieldGroupMask = SetBit(newGroupMask.m_Track.m_Right.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
    }
    if (type == "pedestrianLaneStopLine") {
      newSignal = currentSignal == "stop" ? "go" : "stop";
      newGroupMask.m_Pedestrian.m_GoGroupMask = SetBit(newGroupMask.m_Pedestrian.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
    }
    call("C2VM.TLE", "CallUpdateSubLaneGroupMask", JSON.stringify([newGroupMask]));
  }, [props.subLane]);

  const linkHandler = useCallback(() => {
    const newGroupMask: EdgeGroupMask = JSON.parse(JSON.stringify(props.edge.m_EdgeGroupMask));
    newGroupMask.m_Options &= ~EdgeGroupMaskOptions.PerLaneSignal;
    call("C2VM.TLE", "CallUpdateEdgeGroupMask", JSON.stringify([newGroupMask]));
  }, [props.edge.m_EdgeGroupMask]);

  const carLaneCount = props.subLane.m_CarLaneLeftCount + props.subLane.m_CarLaneStraightCount + props.subLane.m_CarLaneRightCount + props.subLane.m_CarLaneUTurnCount;
  const trackLaneCount = props.subLane.m_TrackLaneLeftCount + props.subLane.m_TrackLaneStraightCount + props.subLane.m_TrackLaneRightCount;
  const containerRef = useRef(null);

  useEffect(() => {
    if (containerRef != null && containerRef.current != null && props.position) {
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.leftPX = props.position.left;
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.topPX = props.position.top;
    }
  }, [containerRef, props.position]);

  return (
    <Container ref={containerRef} translateY={carLaneCount + trackLaneCount > 0 ? "0" : "-100%"}>
      <Content>
        <LaneContainer>
          {carLaneCount > 0 && <>
            <Column>
              <Lane
                data={GetCustomPhaseLane(props.subLane, props.index, "carLane")}
                index={props.index}
                showIcon={true}
                onClick={clickHandler}
              />
            </Column>
            {trackLaneCount > 0 && <Divider />}
          </>}
          {trackLaneCount > 0 && <>
            <Column>
              <Lane
                data={GetCustomPhaseLane(props.subLane, props.index, "trackLane")}
                index={props.index}
                showIcon={true}
                onClick={clickHandler}
              />
            </Column>
          </>}
          {props.subLane.m_PedestrianLaneCount > 0 && <>
            <Column>
              <Lane
                data={GetCustomPhaseLane(props.subLane, props.index, "pedestrianLaneStopLine")}
                index={props.index}
                showIcon={false}
                onClick={clickHandler}
              />
            </Column>
          </>}
        </LaneContainer>
        <HorizontalDivider />
        <IconContainer><LinkVariant style={IconStyle} onClick={linkHandler} /></IconContainer>
      </Content>
    </Container>
  );
}