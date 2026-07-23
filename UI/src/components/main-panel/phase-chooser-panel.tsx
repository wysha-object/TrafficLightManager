import { Button, Scrollable } from 'cs2/ui'
import { useTranslate } from 'hooks/translate'
import { useState } from 'react'
import styled from 'styled-components'
import PhaseRow from './mod/phase-row'
import {
  addCustomPhaseCmd,
  setManualPhaseIndexCmd,
  useGetCustomPhaseItemsCmd,
  useGetTrafficLightGroupCmd,
} from 'hooks/cmds'
import Radio from 'components/base/radio'

const PanelContainer = styled.div`
  width: 15em;
  max-width: 15em;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 0.25em;
`

const Label = styled.div<{ dim?: boolean }>`
  color: ${(props) => (props.dim ? 'var(--textColorDim)' : 'var(--textColor)')};
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  display: inline;
`

export default function PhaseChooserPanel() {
  let [manualControl, setManualControl] = useState(false)

  const trafficLightGroup = useGetTrafficLightGroupCmd()
  const customPhaseItems = useGetCustomPhaseItemsCmd()

  const { t } = useTranslate()

  return (
    <PanelContainer>
      {!manualControl && (
        <>
          <Scrollable style={{ flex: 1 }}>
            {customPhaseItems.map((_, index) => (
              <PhaseRow
                key={index}
                itemIndex={index}
                itemCount={customPhaseItems.length}
                trafficLightGroup={trafficLightGroup}
              />
            ))}
          </Scrollable>
          {customPhaseItems.length > 0 && (
            <>
              <div className='horizontal-divider-with-gap' />
              <div className='row'>
                <Button
                  style={{ width: '100%' }}
                  variant='flat'
                  onClick={() => setManualControl(true)}
                >
                  {t('CustomPhaseEditor.ManualControl')}
                </Button>
              </div>
            </>
          )}
          {customPhaseItems.length < 16 && (
            <div className='row'>
              <Button
                style={{ width: '100%' }}
                variant='flat'
                onClick={() => {
                  addCustomPhaseCmd()
                }}
              >
                {t('CustomPhaseEditor.Add')}
              </Button>
            </div>
          )}
        </>
      )}
      {manualControl && (
        <>
          <Scrollable style={{ flex: 1 }}>
            <div className='row'>
              <Label dim={false}>{t('CustomPhaseEditor.ManualControl')}</Label>
            </div>
            {customPhaseItems.map((_, index) => (
              <div
                className='row-with-hover-effect'
                onClick={() => setManualPhaseIndexCmd(index)}
              >
                <Radio
                  isChecked={trafficLightGroup.manualPhaseIndex == index}
                />
                <Label dim={true}>{t('Phase') + ' #' + (index + 1)}</Label>
              </div>
            ))}
          </Scrollable>
          <div className='horizontal-divider-with-gap' />
          <div
            className='row'
            onClick={() => {
              setManualPhaseIndexCmd(-1)
              setManualControl(false)
            }}
          >
            <Button variant='flat' style={{ width: '100%' }}>
              {t('CustomPhaseEditor.Back')}
            </Button>
          </div>
        </>
      )}
    </PanelContainer>
  )
}
