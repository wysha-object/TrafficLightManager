import { useCallback, useContext, useEffect, useRef } from 'react';
import styled from 'styled-components';

import { call } from 'cs2/api';

import { EdgeGroupMaskOptions } from "@/constants";

import LinkVariant from '@/components/common/icons/link-variant';
import { SubLaneGroupMaskContextClipboard } from '@/context';
import Copy from '../common/icons/copy';
import Paste from '../common/icons/paste';
import SublaneViewer from '../common/sublane-viewer';

const Container = styled.div<{ translateY: string }>`
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
  display: flex;
  flex-direction: column;
  align-items: center;
`;

const HorizontalDivider = styled.div`
  height: 2px;
  width: 100%;
  border: 2px solid rgba(255, 255, 255, 0.1);
  margin: 6rem -6rem;
`;

const IconContainer = styled.div`
  &:hover {
    filter: brightness(1.2) contrast(1.2);
  }
`;

const BigIconStyle = {
  color: "var(--textColorDim)",
  width: "30rem",
  height: "30rem",
  margin: "3rem"
};

const SmallIconStyle = {
  color: "var(--textColorDim)",
  width: "20rem",
  height: "20rem",
  margin: "2rem"
};

export default function SubLanePanel(props: { edge: EdgeInfo, subLane: SubLaneInfo, index: number, position: ScreenPoint }) {
  const clipboard = useContext(SubLaneGroupMaskContextClipboard.context);

  const linkHandler = useCallback(() => {
    const newGroupMask: EdgeGroupMask = JSON.parse(JSON.stringify(props.edge.m_EdgeGroupMask));
    newGroupMask.m_Options &= ~EdgeGroupMaskOptions.PerLaneSignal;
    call("TrafficLightManager", "CallUpdateEdgeGroupMask", JSON.stringify({ groupMaskArray: [newGroupMask], entity: props.edge.m_TrafficLightsEntity }));
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
              right: props.subLane.m_TrackLaneRightCount > 0
            },
            pedestrianLaneStopLine: props.subLane.m_PedestrianLaneCount > 0
          }}
          index={props.index}
          subLaneUpdateHandler={
            (newGroupMask) => call("TrafficLightManager", "CallUpdateSubLaneGroupMask", JSON.stringify({ groupMaskArray: [newGroupMask], entity: props.edge.m_TrafficLightsEntity }))
          }
        />
        <HorizontalDivider />
        <IconContainer><LinkVariant style={BigIconStyle} onClick={linkHandler} /></IconContainer>
        <div style={{ display: "flex", flexDirection: "row", justifyContent: "center", alignItems: "center" }}>
          <IconContainer>
            <Copy
              style={SmallIconStyle}
              onClick={() => {
                clipboard.save(props.subLane.m_SubLaneGroupMask);
              }}
            />
          </IconContainer>
          <IconContainer>
            <Paste
              style={{ ...SmallIconStyle, "opacity": clipboard.selectedIndex >= 0 ? 1 : 0.5 }}
              onClick={() => {
                if (clipboard.selectedIndex >= 0) {
                  const selectedData = clipboard.history[clipboard.selectedIndex];
                  const newGroupMask: SubLaneGroupMask = JSON.parse(JSON.stringify(props.subLane.m_SubLaneGroupMask));
                  newGroupMask.m_Car = selectedData.m_Car;
                  newGroupMask.m_Track = selectedData.m_Track;
                  newGroupMask.m_Pedestrian = selectedData.m_Pedestrian;
                  call("TrafficLightManager", "CallUpdateEdgeGroupMask", JSON.stringify({ groupMaskArray: [newGroupMask], entity: props.edge.m_TrafficLightsEntity }));
                }
              }}
            />
          </IconContainer>
        </div>
      </Content>
    </Container>
  );
}