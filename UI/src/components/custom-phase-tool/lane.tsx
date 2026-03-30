import { useContext } from 'react';
import styled from 'styled-components';

import { CityConfigurationContext } from '@/context';

import TrafficSignButton from '@/components/common/traffic-sign-button';
import Tooltip from '@/components/common/tooltip';

import Car from '@/components/common/icons/car';
import Train from '@/components/common/icons/train';
import Walk from '@/components/common/icons/walk';
import BusSide from '@/components/common/icons/bus-side';

import TrafficSignTooltip from './tooltips/traffic-sign';

const Container = styled.div`
  flex: 1;
  display: flex;
  justify-content: center;
  align-content: center;
  flex-direction: column;
  align-items: center;
  margin: 0 0 -6rem 0;
`;

const Filler = styled.div`
  flex: 1;
`;

const IconContainer = styled.div`
  width: 30rem;
  height: 30rem;
`;

const Box = (props: {state?: CustomPhaseSignalState, children?: React.ReactNode}) => {
  const BoxContainer = styled.div`
    flex: 1;
    display: flex;
    justify-content: center;
    align-content: center;
    flex-direction: column;
    align-items: center;
    margin: 0 0 6rem 0;
  `;
  if (props.state) {
    return (
      <Tooltip position="right" tooltip={<TrafficSignTooltip state={props.state} />} tooltipStyle={{marginTop: "-3rem"}}>
        <BoxContainer>{props.children}</BoxContainer>
      </Tooltip>
    );
  }
  return <BoxContainer>{props.children}</BoxContainer>;
};

export default function Lane(props: {
  data: CustomPhaseLane,
  index: number,
  showIcon: boolean,
  onClick: (index: number, type: CustomPhaseLaneType, direction: CustomPhaseLaneDirection, currentSignal: CustomPhaseSignalState) => void
}) {
  const cityConfiguration = useContext(CityConfigurationContext);
  return (
    <Container>
      {["pedestrianLaneStopLine", "pedestrianLaneNonStopLine"].includes(props.data.type) ? <>
        {props.data.type == "pedestrianLaneStopLine" && props.showIcon && <Box><IconContainer><Walk stopLine={true} style={{color: "var(--textColor)"}} /></IconContainer></Box>}
        {props.data.type == "pedestrianLaneNonStopLine" && props.showIcon && <Box><IconContainer><Walk stopLine={false} style={{color: "var(--textColor)"}} /></IconContainer></Box>}
        <Box state={props.data.all}>
          <TrafficSignButton
            allow={false}
            variant="pedestrian"
            sign="↑"
            state={props.data.all}
            onClick={() => props.onClick(props.index, props.data.type, "all", props.data.all)}
          />
        </Box>
        <Filler />
      </> : <>
        {props.data.type == "carLane" && props.showIcon && <Box><IconContainer><Car style={{color: "var(--textColor)"}} /></IconContainer></Box>}
        {props.data.type == "publicCarLane" && props.showIcon && <Box><IconContainer><BusSide style={{color: "var(--textColor)"}} /></IconContainer></Box>}
        {props.data.type == "trackLane" && props.showIcon && <Box><IconContainer><Train style={{color: "var(--textColor)"}} /></IconContainer></Box>}
        {props.data.left != "none" && <>
          <Box state={props.data.left}>
            <TrafficSignButton
              allow={false}
              variant="traffic-light"
              sign="←"
              state={props.data.left}
              onClick={() => props.onClick(props.index, props.data.type, "left", props.data.left)}
            />
          </Box>
        </>}
        {props.data.straight != "none" && <>
          <Box state={props.data.straight}>
            <TrafficSignButton
              allow={false}
              variant="traffic-light"
              sign="↑"
              state={props.data.straight}
              onClick={() => props.onClick(props.index, props.data.type, "straight", props.data.straight)}
            />
          </Box>
        </>}
        {props.data.right != "none" && <>
          <Box state={props.data.right}>
            <TrafficSignButton
              allow={false}
              variant="traffic-light"
              sign="→"
              state={props.data.right}
              onClick={() => props.onClick(props.index, props.data.type, "right", props.data.right)}
            />
          </Box>
        </>}
        {props.data.uTurn != "none" && <>
          <Box state={props.data.uTurn}>
            <TrafficSignButton
              allow={false}
              variant="traffic-light"
              sign={cityConfiguration.leftHandTraffic ? "↷" : "↶"}
              state={props.data.uTurn}
              onClick={() => props.onClick(props.index, props.data.type, "uTurn", props.data.uTurn)}
            />
          </Box>
        </>}
      </>}
    </Container>
  );
}