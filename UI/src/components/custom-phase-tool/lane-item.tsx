import CarSvg from 'assets/images/car.svg'
import TrainSvg from 'assets/images/train.svg'
import WalkSvg from 'assets/images/walk.svg'
import WalkUnderlineSvg from 'assets/images/walk-underline.svg'
import BusSideSvg from 'assets/images/bus-side.svg'
import { useContext } from 'react'
import styled from 'styled-components'
import { CityConfigurationContext } from 'context'
import TrafficSignButton from 'components/custom-phase-tool/traffic-sign-button'
import TipArea from 'components/base/tip-area'
import TrafficSignTooltip from './traffic-sign'
import {
  CustomPhaseSignalState,
  CustomPhaseLane,
  CustomPhaseLaneType,
  CustomPhaseLaneDirection,
} from 'types'

const Container = styled.div`
  flex: 1;
  display: flex;
  justify-content: center;
  align-content: center;
  flex-direction: column;
  align-items: center;
  margin: 0 0 -6rem 0;
`

const Filler = styled.div`
  flex: 1;
`

const Box = (props: {
  state?: CustomPhaseSignalState
  children?: React.ReactNode
}) => {
  const BoxContainer = styled.div`
    flex: 1;
    display: flex;
    justify-content: center;
    align-content: center;
    flex-direction: column;
    align-items: center;
    margin: 0 0 6rem 0;
  `
  if (props.state) {
    return (
      <TipArea
        position='right'
        tooltip={<TrafficSignTooltip state={props.state} />}
      >
        <BoxContainer>{props.children}</BoxContainer>
      </TipArea>
    )
  }
  return <BoxContainer>{props.children}</BoxContainer>
}

export default function LaneItem(props: {
  data: CustomPhaseLane
  index: number
  showIcon: boolean
  onClick: (
    index: number,
    type: CustomPhaseLaneType,
    direction: CustomPhaseLaneDirection,
    currentSignal: CustomPhaseSignalState,
  ) => void
}) {
  const cityConfiguration = useContext(CityConfigurationContext)
  return (
    <Container>
      {['pedestrianLaneStopLine', 'pedestrianLaneNonStopLine'].includes(
        props.data.type,
      ) ? (
        <>
          {props.data.type == 'pedestrianLaneStopLine' && props.showIcon && (
            <Box>
              <WalkSvg />
            </Box>
          )}
          {props.data.type == 'pedestrianLaneNonStopLine' && props.showIcon && (
            <Box>
              <WalkUnderlineSvg />
            </Box>
          )}
          <Box state={props.data.all}>
            <TrafficSignButton
              allow={false}
              variant='pedestrian'
              sign='↑'
              state={props.data.all}
              onClick={() =>
                props.onClick(
                  props.index,
                  props.data.type,
                  'all',
                  props.data.all,
                )
              }
            />
          </Box>
          <Filler />
        </>
      ) : (
        <>
          {props.data.type == 'carLane' && props.showIcon && (
            <Box>
              <CarSvg />
            </Box>
          )}
          {props.data.type == 'publicCarLane' && props.showIcon && (
            <Box>
              <BusSideSvg />
            </Box>
          )}
          {props.data.type == 'trackLane' && props.showIcon && (
            <Box>
              <TrainSvg />
            </Box>
          )}
          {props.data.left != 'none' && (
            <>
              <Box state={props.data.left}>
                <TrafficSignButton
                  allow={false}
                  variant='traffic-light'
                  sign='←'
                  state={props.data.left}
                  onClick={() =>
                    props.onClick(
                      props.index,
                      props.data.type,
                      'left',
                      props.data.left,
                    )
                  }
                />
              </Box>
            </>
          )}
          {props.data.straight != 'none' && (
            <>
              <Box state={props.data.straight}>
                <TrafficSignButton
                  allow={false}
                  variant='traffic-light'
                  sign='↑'
                  state={props.data.straight}
                  onClick={() =>
                    props.onClick(
                      props.index,
                      props.data.type,
                      'straight',
                      props.data.straight,
                    )
                  }
                />
              </Box>
            </>
          )}
          {props.data.right != 'none' && (
            <>
              <Box state={props.data.right}>
                <TrafficSignButton
                  allow={false}
                  variant='traffic-light'
                  sign='→'
                  state={props.data.right}
                  onClick={() =>
                    props.onClick(
                      props.index,
                      props.data.type,
                      'right',
                      props.data.right,
                    )
                  }
                />
              </Box>
            </>
          )}
          {props.data.uTurn != 'none' && (
            <>
              <Box state={props.data.uTurn}>
                <TrafficSignButton
                  allow={false}
                  variant='traffic-light'
                  sign={cityConfiguration.leftHandTraffic ? '↷' : '↶'}
                  state={props.data.uTurn}
                  onClick={() =>
                    props.onClick(
                      props.index,
                      props.data.type,
                      'uTurn',
                      props.data.uTurn,
                    )
                  }
                />
              </Box>
            </>
          )}
        </>
      )}
    </Container>
  )
}
