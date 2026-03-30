import { useCallback, useContext, useEffect, useRef } from 'react';
import styled from 'styled-components';

import { call } from 'cs2/api';

import { CityConfigurationContext } from '@/context';

import { EdgeGroupMaskOptions } from "@/constants";

import Lane from './lane';
import LinkVariantOff from '@/components/common/icons/link-variant-off';

const Container = styled.div`
  position: fixed;
  transform: translate(-50%);
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

function GetCustomPhaseLane(edge: EdgeInfo, index: number, type: CustomPhaseLaneType): CustomPhaseLane {
  const result: CustomPhaseLane = {
    type: type,
    left: "stop",
    straight: "stop",
    right: "stop",
    uTurn: "stop",
    all: "stop"
  }
  if (type == "carLane") {
    result.left = (edge.m_EdgeGroupMask.m_Car.m_Left.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.left;
    result.straight = (edge.m_EdgeGroupMask.m_Car.m_Straight.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.straight;
    result.right = (edge.m_EdgeGroupMask.m_Car.m_Right.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.right;
    result.uTurn = (edge.m_EdgeGroupMask.m_Car.m_UTurn.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.uTurn;
    result.left = (edge.m_EdgeGroupMask.m_Car.m_Left.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.left;
    result.straight = (edge.m_EdgeGroupMask.m_Car.m_Straight.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.straight;
    result.right = (edge.m_EdgeGroupMask.m_Car.m_Right.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.right;
    result.uTurn = (edge.m_EdgeGroupMask.m_Car.m_UTurn.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.uTurn;
    result.left = edge.m_CarLaneLeftCount <= 0 ? "none" : result.left;
    result.straight = edge.m_CarLaneStraightCount <= 0 ? "none" : result.straight;
    result.right = edge.m_CarLaneRightCount <= 0 ? "none" : result.right;
    result.uTurn = edge.m_CarLaneUTurnCount <= 0 ? "none" : result.uTurn;
  }
  if (type == "publicCarLane") {
    result.left = (edge.m_EdgeGroupMask.m_PublicCar.m_Left.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.left;
    result.straight = (edge.m_EdgeGroupMask.m_PublicCar.m_Straight.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.straight;
    result.right = (edge.m_EdgeGroupMask.m_PublicCar.m_Right.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.right;
    result.uTurn = (edge.m_EdgeGroupMask.m_PublicCar.m_UTurn.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.uTurn;
    result.left = (edge.m_EdgeGroupMask.m_PublicCar.m_Left.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.left;
    result.straight = (edge.m_EdgeGroupMask.m_PublicCar.m_Straight.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.straight;
    result.right = (edge.m_EdgeGroupMask.m_PublicCar.m_Right.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.right;
    result.uTurn = (edge.m_EdgeGroupMask.m_PublicCar.m_UTurn.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.uTurn;
    result.left = edge.m_PublicCarLaneLeftCount <= 0 ? "none" : result.left;
    result.straight = edge.m_PublicCarLaneStraightCount <= 0 ? "none" : result.straight;
    result.right = edge.m_PublicCarLaneRightCount <= 0 ? "none" : result.right;
    result.uTurn = edge.m_PublicCarLaneUTurnCount <= 0 ? "none" : result.uTurn;
  }
  if (type == "trackLane") {
    result.left = (edge.m_EdgeGroupMask.m_Track.m_Left.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.left;
    result.straight = (edge.m_EdgeGroupMask.m_Track.m_Straight.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.straight;
    result.right = (edge.m_EdgeGroupMask.m_Track.m_Right.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.right;
    result.left = (edge.m_EdgeGroupMask.m_Track.m_Left.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.left;
    result.straight = (edge.m_EdgeGroupMask.m_Track.m_Straight.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.straight;
    result.right = (edge.m_EdgeGroupMask.m_Track.m_Right.m_YieldGroupMask & (1 << index)) != 0 ? "yield" : result.right;
    result.left = edge.m_TrackLaneLeftCount <= 0 ? "none" : result.left;
    result.straight = edge.m_TrackLaneStraightCount <= 0 ? "none" : result.straight;
    result.right = edge.m_TrackLaneRightCount <= 0 ? "none" : result.right;
    result.uTurn = "none";
  }
  if (type == "pedestrianLaneStopLine") {
    result.all = (edge.m_EdgeGroupMask.m_PedestrianStopLine.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.all;
  }
  if (type == "pedestrianLaneNonStopLine") {
    result.all = (edge.m_EdgeGroupMask.m_PedestrianNonStopLine.m_GoGroupMask & (1 << index)) != 0 ? "go" : result.all;
  }
  return result;
}

function SetBit(input: number, index: number, value: number) {
    return ((input & (~(1 << index))) | (value << index));
}

export default function EdgePanel(props: {data: EdgeInfo, index: number, position: ScreenPoint}) {
  const clickHandler = useCallback((index: number, type: CustomPhaseLaneType, direction: CustomPhaseLaneDirection, currentSignal: CustomPhaseSignalState) => {
    let newSignal = currentSignal == "stop" ? "go" : (currentSignal == "go" ? "yield" : "stop");
    const newGroupMask: EdgeGroupMask = JSON.parse(JSON.stringify(props.data.m_EdgeGroupMask));
    if (type == "carLane") {
      if (direction == "left") {
        newGroupMask.m_Car.m_Left.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_Left.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_Left.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_Left.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "straight") {
        newGroupMask.m_Car.m_Straight.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_Straight.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_Straight.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_Straight.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "right") {
        newGroupMask.m_Car.m_Right.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_Right.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_Right.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_Right.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "uTurn") {
        newGroupMask.m_Car.m_UTurn.m_GoGroupMask = SetBit(newGroupMask.m_Car.m_UTurn.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_Car.m_UTurn.m_YieldGroupMask = SetBit(newGroupMask.m_Car.m_UTurn.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
    }
    if (type == "publicCarLane") {
      if (direction == "left") {
        newGroupMask.m_PublicCar.m_Left.m_GoGroupMask = SetBit(newGroupMask.m_PublicCar.m_Left.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_PublicCar.m_Left.m_YieldGroupMask = SetBit(newGroupMask.m_PublicCar.m_Left.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "straight") {
        newGroupMask.m_PublicCar.m_Straight.m_GoGroupMask = SetBit(newGroupMask.m_PublicCar.m_Straight.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_PublicCar.m_Straight.m_YieldGroupMask = SetBit(newGroupMask.m_PublicCar.m_Straight.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "right") {
        newGroupMask.m_PublicCar.m_Right.m_GoGroupMask = SetBit(newGroupMask.m_PublicCar.m_Right.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_PublicCar.m_Right.m_YieldGroupMask = SetBit(newGroupMask.m_PublicCar.m_Right.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
      }
      if (direction == "uTurn") {
        newGroupMask.m_PublicCar.m_UTurn.m_GoGroupMask = SetBit(newGroupMask.m_PublicCar.m_UTurn.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
        newGroupMask.m_PublicCar.m_UTurn.m_YieldGroupMask = SetBit(newGroupMask.m_PublicCar.m_UTurn.m_YieldGroupMask, index, newSignal == "yield" ? 1 : 0);
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
      newGroupMask.m_PedestrianStopLine.m_GoGroupMask = SetBit(newGroupMask.m_PedestrianStopLine.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
    }
    if (type == "pedestrianLaneNonStopLine") {
      newSignal = currentSignal == "stop" ? "go" : "stop";
      newGroupMask.m_PedestrianNonStopLine.m_GoGroupMask = SetBit(newGroupMask.m_PedestrianNonStopLine.m_GoGroupMask, index, newSignal != "stop" ? 1 : 0);
    }
    call("C2VM.TLE", "CallUpdateEdgeGroupMask", JSON.stringify([newGroupMask]));
  }, [props.data.m_EdgeGroupMask]);

  const unlinkHandler = useCallback(() => {
    const newGroupMask: EdgeGroupMask = JSON.parse(JSON.stringify(props.data.m_EdgeGroupMask));
    newGroupMask.m_Options |= EdgeGroupMaskOptions.PerLaneSignal;
    call("C2VM.TLE", "CallUpdateEdgeGroupMask", JSON.stringify([newGroupMask]));
  }, [props.data.m_EdgeGroupMask]);

  const cityConfiguration = useContext(CityConfigurationContext);
  const carLaneCount = props.data.m_CarLaneLeftCount + props.data.m_CarLaneStraightCount + props.data.m_CarLaneRightCount + props.data.m_CarLaneUTurnCount;
  const publicCarLaneCount = props.data.m_PublicCarLaneLeftCount + props.data.m_PublicCarLaneStraightCount + props.data.m_PublicCarLaneRightCount + props.data.m_PublicCarLaneUTurnCount;
  const trackLaneCount = props.data.m_TrackLaneLeftCount + props.data.m_TrackLaneStraightCount + props.data.m_TrackLaneRightCount;
  const containerRef = useRef(null);

  useEffect(() => {
    if (containerRef != null && containerRef.current != null && props.position) {
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.leftPX = props.position.left;
      // @ts-expect-error: cohtml specific extension
      containerRef.current.style.topPX = props.position.top;
    }
  }, [containerRef, props.position]);

  if (carLaneCount + publicCarLaneCount + trackLaneCount + props.data.m_PedestrianLaneStopLineCount + props.data.m_PedestrianLaneNonStopLineCount <= 0) {
    return <></>;
  }

  return (
    <Container ref={containerRef}>
      <Content>
        <LaneContainer>
          {carLaneCount > 0 && <>
            <Column>
              <Lane
                data={GetCustomPhaseLane(props.data, props.index, "carLane")}
                index={props.index}
                showIcon={true}
                onClick={clickHandler}
              />
            </Column>
            {publicCarLaneCount + trackLaneCount + props.data.m_PedestrianLaneStopLineCount + props.data.m_PedestrianLaneNonStopLineCount > 0 && <Divider />}
          </>}
          {publicCarLaneCount > 0 && <>
            <Column>
              <Lane
                data={GetCustomPhaseLane(props.data, props.index, "publicCarLane")}
                index={props.index}
                showIcon={true}
                onClick={clickHandler}
              />
            </Column>
            {trackLaneCount + props.data.m_PedestrianLaneStopLineCount + props.data.m_PedestrianLaneNonStopLineCount > 0 && <Divider />}
          </>}
          {trackLaneCount > 0 && <>
            <Column>
              <Lane
                data={GetCustomPhaseLane(props.data, props.index, "trackLane")}
                index={props.index}
                showIcon={true}
                onClick={clickHandler}
              />
            </Column>
            {props.data.m_PedestrianLaneStopLineCount + props.data.m_PedestrianLaneNonStopLineCount > 0 && <Divider />}
          </>}
          {cityConfiguration.leftHandTraffic && <>
            {props.data.m_PedestrianLaneStopLineCount > 0 && <>
              <Column>
                <Lane
                  data={GetCustomPhaseLane(props.data, props.index, "pedestrianLaneStopLine")}
                  index={props.index}
                  showIcon={true}
                  onClick={clickHandler}
                />
              </Column>
              {props.data.m_PedestrianLaneNonStopLineCount > 0 && <Divider />}
            </>}
            {props.data.m_PedestrianLaneNonStopLineCount > 0 && <>
              <Column>
                <Lane
                  data={GetCustomPhaseLane(props.data, props.index, "pedestrianLaneNonStopLine")}
                  index={props.index}
                  showIcon={true}
                  onClick={clickHandler}
                />
              </Column>
            </>}
          </>}
          {!cityConfiguration.leftHandTraffic && <>
            {props.data.m_PedestrianLaneNonStopLineCount > 0 && <>
              <Column>
                <Lane
                  data={GetCustomPhaseLane(props.data, props.index, "pedestrianLaneNonStopLine")}
                  index={props.index}
                  showIcon={true}
                  onClick={clickHandler}
                />
              </Column>
              {props.data.m_PedestrianLaneStopLineCount > 0 && <Divider />}
            </>}
            {props.data.m_PedestrianLaneStopLineCount > 0 && <>
              <Column>
                <Lane
                  data={GetCustomPhaseLane(props.data, props.index, "pedestrianLaneStopLine")}
                  index={props.index}
                  showIcon={true}
                  onClick={clickHandler}
                />
              </Column>
            </>}
          </>}
        </LaneContainer>
        <HorizontalDivider />
        <IconContainer><LinkVariantOff style={IconStyle} onClick={unlinkHandler} /></IconContainer>
      </Content>
    </Container>
  );
}