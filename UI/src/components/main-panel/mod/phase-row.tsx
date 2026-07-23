import CheckSvg from 'assets/images/check.svg'
import ChevronSvg from 'assets/images/chevron-down.svg'
import ChevronUpSvg from 'assets/images/chevron-up.svg'
import SolidCircleSvg from 'assets/images/solid-circle.svg'
import DeleteSvg from 'assets/images/delete.svg'
import TuneSvg from 'assets/images/tune.svg'
import VisibilitySvg from 'assets/images/visibility.svg'
import VisibilityOffSvg from 'assets/images/visibility-off.svg'
import CopySvg from 'assets/images/copy.svg'
import LinkVariantSvg from 'assets/images/link-variant.svg'
import LinkVariantOffSvg from 'assets/images/link-variant-off.svg'
import { useCallback, useContext, useEffect, useMemo, useState } from 'react'
import styled from 'styled-components'
import InputField from 'components/base/text-input'
import { Button } from 'cs2/ui'
import { ToolState, TrafficLightGroup } from 'types'
import {
  swapCustomPhaseCmd,
  useGetCustomPhaseItemsCmd,
  useGetToolStateCmd,
  setToolStateCmd,
  setCustomPhaseDataCmd,
  removeCustomPhaseCmd,
  copyPhaseCmd,
} from 'hooks/cmds'
import { CurrentFocusPhaseIndexContext } from 'context'
import { useTranslate } from 'hooks/translate'
import TipArea from 'components/base/tip-area'

const Container = styled.div`
  display: flex;
  align-items: center;
`

const dividerContainerStyle = {
  flexGrow: 1,
  flexShrink: 1,
  flexBasis: 'auto',
}

const IconBarContainer = styled.div`
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  flex-direction: row;
  display: flex;
  justify-content: flex-end;
`

function PhaseItemDivider(props: { index: number; linked: boolean }) {
  const { t } = useTranslate()

  const clickHandler = () => {
    setCustomPhaseDataCmd({
      key: 'LinkedWithNextPhase',
      index: props.index,
      value: !props.linked,
    })
  }
  return (
    <Container>
      <div style={dividerContainerStyle}>
        {!props.linked && <div className='horizontal-divider' />}
      </div>
      <div>
        <TipArea
          position='right'
          tooltip={props.linked ? t('UnlinkPhase') : t('LinkPhase')}
        >
          <Button variant='round' onClick={clickHandler}>
            {props.linked && <LinkVariantOffSvg />}
            {!props.linked && <LinkVariantSvg />}
          </Button>
        </TipArea>
      </div>
      <div style={dividerContainerStyle}>
        {!props.linked && <div className='horizontal-divider' />}
      </div>
    </Container>
  )
}

export default function PhaseRow(props: {
  itemIndex: number
  itemCount: number
  trafficLightGroup: TrafficLightGroup
}) {
  const [currentFocusPhaseIndex, setCurrentFocusPhaseIndex] = useContext(
    CurrentFocusPhaseIndexContext,
  )
  const [isActiveLabel, setIsActiveLabel] = useState(false)

  const toolState = useGetToolStateCmd()
  const customPhaseItems = useGetCustomPhaseItemsCmd()

  const customPhaseData = useMemo(
    () => customPhaseItems[props.itemIndex],
    [customPhaseItems, props.itemIndex],
  )

  const swapItem = useCallback(
    (index1: number, index2: number) => {
      swapCustomPhaseCmd(index1, index2)

      if (index1 === currentFocusPhaseIndex) {
        setCurrentFocusPhaseIndex(index2)
      } else if (index2 === currentFocusPhaseIndex) {
        setCurrentFocusPhaseIndex(index1)
      }
    },
    [currentFocusPhaseIndex, setCurrentFocusPhaseIndex],
  )

  useEffect(() => {
    if (currentFocusPhaseIndex === props.itemIndex) {
      setIsActiveLabel(true)
    } else {
      setIsActiveLabel(false)
    }
  }, [currentFocusPhaseIndex, props.itemIndex, setIsActiveLabel])
  return (
    <>
      <div className='row' style={{ padding: '0.25em' }}>
        <div style={{ width: '1em', paddingLeft: '0.3em' }}>
          {props.trafficLightGroup.currentPhaseIndex === props.itemIndex && (
            <SolidCircleSvg
              style={{ height: '0.3em', width: '0.3em', fill: 'lightgreen' }}
            />
          )}
        </div>
        <div
          style={{
            width: '6em',
            display: 'flex',
            alignItems: 'center',
            color: !isActiveLabel ? 'var(--textColorDim)' : 'var(--textColor)',
          }}
        >
          <InputField
            onChange={(value) =>
              setCustomPhaseDataCmd({
                key: 'Name',
                value: value,
                index: props.itemIndex,
              })
            }
            value={customPhaseData.name}
            displayWhenEmpty={'Phase #' + (props.itemIndex + 1)}
          />
        </div>
        <IconBarContainer>
          {currentFocusPhaseIndex === props.itemIndex &&
          toolState === ToolState.Editing ? (
            <>
              <Button
                variant='round'
                onClick={() => {
                  removeCustomPhaseCmd(props.itemIndex)
                  if (currentFocusPhaseIndex === props.itemIndex) {
                    setCurrentFocusPhaseIndex(-1)
                    setToolStateCmd(ToolState.Choosed)
                  }
                }}
              >
                <DeleteSvg />
              </Button>
              <Button
                variant='round'
                onClick={() => {
                  setCurrentFocusPhaseIndex(-1)
                  setToolStateCmd(ToolState.Choosed)
                }}
              >
                <CheckSvg />
              </Button>
              <Button
                variant='round'
                onClick={() =>
                  props.itemIndex - 1 >= 0 &&
                  swapItem(props.itemIndex, props.itemIndex - 1)
                }
                disabled={props.itemIndex - 1 < 0}
              >
                <ChevronUpSvg />
              </Button>
              <Button
                variant='round'
                onClick={() =>
                  props.itemIndex + 1 < props.itemCount &&
                  swapItem(props.itemIndex, props.itemIndex + 1)
                }
                disabled={props.itemIndex + 1 >= props.itemCount}
              >
                <ChevronSvg />
              </Button>
            </>
          ) : (
            <>
              {currentFocusPhaseIndex === props.itemIndex ? (
                <Button
                  variant='round'
                  onClick={() => {
                    setCurrentFocusPhaseIndex(-1)
                    setToolStateCmd(ToolState.Choosed)
                  }}
                >
                  <VisibilityOffSvg />
                </Button>
              ) : (
                <Button
                  variant='round'
                  onClick={() => {
                    setCurrentFocusPhaseIndex(props.itemIndex)
                    setToolStateCmd(ToolState.Choosed)
                  }}
                >
                  <VisibilitySvg />
                </Button>
              )}
              <Button
                variant='round'
                onClick={() => {
                  setCurrentFocusPhaseIndex(props.itemIndex)
                  setToolStateCmd(ToolState.Editing)
                }}
              >
                <TuneSvg />
              </Button>
              <Button
                variant='round'
                onClick={async () => {
                  let newIndex: number = await copyPhaseCmd(props.itemIndex)
                  setCurrentFocusPhaseIndex(newIndex)
                  setToolStateCmd(ToolState.Choosed)
                }}
              >
                <CopySvg />
              </Button>
            </>
          )}
        </IconBarContainer>
      </div>
      {
        <PhaseItemDivider
          index={props.itemIndex}
          linked={customPhaseData.linkedWithNextPhase}
        />
      }
    </>
  )
}
