import { useTranslate } from 'hooks/translate'
import TipArea from 'components/base/tip-area'
import RangeRow from 'components/main-panel/mod/range-row'
import { Button, PanelFoldout, Scrollable } from 'cs2/ui'
import TemplateSection from './mod/template-section'
import { useContext, useMemo } from 'react'
import { ToolState } from 'types'
import {
  setCustomPhaseDataCmd,
  useGetCustomPhaseItemsCmd,
  useGetSettingsCmd,
  useGetToolStateCmd,
  useGetTrafficLightGroupCmd,
} from 'hooks/cmds'
import { CurrentFocusPhaseIndexContext } from 'context'
import styled from 'styled-components'
import classNames from 'classnames'
import TextRow from './mod/text-row'
import CheckboxRow from './mod/checkbox-row'
import TipIcon from 'components/icon/tip-icon'

const PanelContainer = styled.div`
  background-color: var(--sectionBackgroundColor);
  backdrop-filter: var(--panelBlur);
  flex: 1;
  position: relative;
  padding: 0.25em;
`

const TextItem = (props: {
  title: string
  className?: string
  style?: React.CSSProperties
  secondaryText?: string
  tooltip?: React.ReactNode
  inlineTooltip?: boolean
  dim?: boolean
}) => {
  const item = (
    <div style={{ width: '100%', display: 'flex', alignItems: 'center' }}>
      <div
        className={classNames({ dim: props.dim })}
        style={{ flex: '1', display: 'flex' }}
      >
        <div style={{ flex: '1' }}>{props.title}</div>
        {props.secondaryText}
      </div>
      {props.tooltip && props.inlineTooltip && (
        <div>
          <TipArea position='right-start' tooltip={props.tooltip}>
            <TipIcon />
          </TipArea>
        </div>
      )}
    </div>
  )
  return (
    <div className={props.className} style={{ width: '100%', ...props.style }}>
      {props.tooltip && !props.inlineTooltip ? (
        <TipArea position='right' tooltip={props.tooltip}>
          {item}
        </TipArea>
      ) : (
        item
      )}
    </div>
  )
}

const EndPhaseButton = (props: { index: number; disabled?: boolean }) => {
  const clickHandler = () => {
    if (!props.disabled) {
      setCustomPhaseDataCmd({ key: 'EndPhasePrematurely', index: props.index })
    }
  }
  const { t } = useTranslate()
  return (
    <div className='row'>
      <Button
        style={{ width: '100%' }}
        variant='flat'
        disabled={props.disabled}
        onClick={clickHandler}
      >
        {props.disabled
          ? t('CustomPhaseEditor.PhaseEndRequested')
          : t('CustomPhaseEditor.EndPhasePrematurely')}
      </Button>
    </div>
  )
}

export default function PhaseDetailsPanel() {
  const { t } = useTranslate()

  const [currentFocusPhaseIndex, _] = useContext(CurrentFocusPhaseIndexContext)

  const settings = useGetSettingsCmd()
  const trafficLightGroup = useGetTrafficLightGroupCmd()
  const customPhaseItems = useGetCustomPhaseItemsCmd()
  const toolState = useGetToolStateCmd()

  const index = useMemo(() => {
    if (currentFocusPhaseIndex < 0) {
      return trafficLightGroup.currentPhaseIndex
    }
    return currentFocusPhaseIndex
  }, [currentFocusPhaseIndex, trafficLightGroup.currentPhaseIndex])
  const customPhaseItem = useMemo(() => {
    if (index < 0 || index >= customPhaseItems.length) {
      return undefined
    }
    return customPhaseItems[index]
  }, [customPhaseItems, customPhaseItems.length, index])
  const statisticsOnly = useMemo(
    () => toolState !== ToolState.Editing,
    [toolState],
  )

  if (!customPhaseItem) {
    return <></>
  }

  return (
    <PanelContainer>
      <Scrollable>
        <div
          style={{
            width: '17em',
            maxWidth: '17em',
          }}
        >
          {!statisticsOnly && (
            <>
              <PanelFoldout
                initialExpanded={true}
                header={
                  <TextItem
                    style={{ padding: '0.2em 0.5em', fontSize: '1.2em' }}
                    title={t('CustomPhaseEditor.Template.Title')}
                    tooltip={t('CustomPhaseEditor.Template.Tooltip')}
                    inlineTooltip={true}
                  />
                }
              >
                <TemplateSection
                  item={customPhaseItem}
                  itemIndex={index}
                ></TemplateSection>
              </PanelFoldout>
              <div className='horizontal-divider' />
              <PanelFoldout
                initialExpanded={true}
                header={
                  <TextItem
                    style={{ padding: '0.2em 0.5em', fontSize: '1.2em' }}
                    title={t('CustomPhaseEditor.Options.Title')}
                    tooltip={t('CustomPhaseEditor.Options.Tooltip')}
                    inlineTooltip={true}
                  />
                }
              >
                <CheckboxRow
                  isChecked={customPhaseItem.prioritiseTrack}
                  label={t('CustomPhaseEditor.Options.PrioritiseTrack')}
                  tooltip={t(
                    'CustomPhaseEditor.Options.PrioritiseTrack.Tooltip',
                  )}
                  onClick={() => {
                    setCustomPhaseDataCmd({
                      key: 'PrioritiseTrack',
                      index: index,
                      value: !customPhaseItem?.prioritiseTrack,
                    })
                  }}
                />
                <CheckboxRow
                  isChecked={customPhaseItem.prioritisePublicCar}
                  label={t('CustomPhaseEditor.Options.PrioritisePublicCar')}
                  tooltip={t(
                    'CustomPhaseEditor.Options.PrioritisePublicCar.Tooltip',
                  )}
                  onClick={() => {
                    setCustomPhaseDataCmd({
                      key: 'PrioritisePublicCar',
                      index: index,
                      value: !customPhaseItem?.prioritisePublicCar,
                    })
                  }}
                />
                <CheckboxRow
                  isChecked={customPhaseItem.prioritisePedestrian}
                  label={t('CustomPhaseEditor.Options.PrioritisePedestrian')}
                  tooltip={t(
                    'CustomPhaseEditor.Options.PrioritisePedestrian.Tooltip',
                  )}
                  onClick={() => {
                    setCustomPhaseDataCmd({
                      key: 'PrioritisePedestrian',
                      index: index,
                      value: !customPhaseItem?.prioritisePedestrian,
                    })
                  }}
                />
              </PanelFoldout>
              <div className='horizontal-divider' />
              <PanelFoldout
                initialExpanded={true}
                header={
                  <TextItem
                    style={{ padding: '0.2em 0.5em', fontSize: '1.2em' }}
                    title={t('CustomPhaseEditor.Adjustments.Title')}
                    inlineTooltip={true}
                  />
                }
              >
                <RangeRow
                  onChange={(value) => {
                    setCustomPhaseDataCmd({
                      key: 'MinimumDuration',
                      value: value,
                      index: index,
                    })
                  }}
                  label={t('CustomPhaseEditor.Adjustments.MinimumDuration')}
                  value={customPhaseItem.minimumDuration}
                  valuePrefix={''}
                  valueSuffix={'s'}
                  min={0}
                  max={30}
                  step={1}
                  defaultValue={
                    settings.defaultCustomPhaseTemplate.m_MinimumDuration
                  }
                  enableTextField={true}
                  textFieldRegExp={'^\\d{0,4}$'}
                />
                <RangeRow
                  onChange={(value) => {
                    setCustomPhaseDataCmd({
                      key: 'MaximumDuration',
                      value: value,
                      index: index,
                    })
                  }}
                  label={t('CustomPhaseEditor.Adjustments.MaximumDuration')}
                  value={customPhaseItem.maximumDuration}
                  valuePrefix={''}
                  valueSuffix={'s'}
                  min={5}
                  max={300}
                  step={5}
                  defaultValue={
                    settings.defaultCustomPhaseTemplate.m_MaximumDuration
                  }
                  enableTextField={true}
                  textFieldRegExp={'^\\d{0,4}$'}
                />
                <RangeRow
                  onChange={(value) => {
                    setCustomPhaseDataCmd({
                      key: 'TargetDurationMultiplier',
                      value: value,
                      index: index,
                    })
                  }}
                  label={t(
                    'CustomPhaseEditor.Adjustments.TargetDurationMultiplier',
                  )}
                  value={customPhaseItem.targetDurationMultiplier}
                  valuePrefix={''}
                  valueSuffix={t('CustomPedestrianDurationMultiplierSuffix')}
                  min={0.1}
                  max={10}
                  step={0.1}
                  defaultValue={
                    settings.defaultCustomPhaseTemplate
                      .m_TargetDurationMultiplier
                  }
                  enableTextField={true}
                  textFieldRegExp={'^\\d{0,4}(\\.\\d{0,2})?$'}
                />
                <RangeRow
                  onChange={(value) => {
                    setCustomPhaseDataCmd({
                      key: 'LaneOccupiedMultiplier',
                      value: value,
                      index: index,
                    })
                  }}
                  label={t(
                    'CustomPhaseEditor.Adjustments.LaneOccupiedMultiplier',
                  )}
                  value={customPhaseItem.laneOccupiedMultiplier}
                  valuePrefix={''}
                  valueSuffix={t('CustomPedestrianDurationMultiplierSuffix')}
                  min={0.1}
                  max={10}
                  step={0.1}
                  defaultValue={
                    settings.defaultCustomPhaseTemplate.m_LaneOccupiedMultiplier
                  }
                  enableTextField={true}
                  textFieldRegExp={'^\\d{0,4}(\\.\\d{0,2})?$'}
                  tooltip={t(
                    'CustomPhaseEditor.Adjustments.LaneOccupiedMultiplier.Tooltip',
                  )}
                />
                <RangeRow
                  onChange={(value) => {
                    setCustomPhaseDataCmd({
                      key: 'IntervalExponent',
                      value: value,
                      index: index,
                    })
                  }}
                  label={t('CustomPhaseEditor.Adjustments.IntervalExponent')}
                  value={customPhaseItem.intervalExponent}
                  valuePrefix={''}
                  valueSuffix={''}
                  min={0.1}
                  max={10}
                  step={0.1}
                  defaultValue={
                    settings.defaultCustomPhaseTemplate.m_IntervalExponent
                  }
                  enableTextField={true}
                  textFieldRegExp={'^\\d{0,4}(\\.\\d{0,2})?$'}
                  tooltip={t(
                    'CustomPhaseEditor.Adjustments.IntervalExponent.Tooltip',
                  )}
                />
              </PanelFoldout>
              <div className='horizontal-divider' />
            </>
          )}
          <PanelFoldout
            initialExpanded={true}
            header={
              <TextItem
                style={{ padding: '0.2em 0.5em', fontSize: '1.2em' }}
                title={t('CustomPhaseEditor.Statistics.Title')}
                tooltip={t('CustomPhaseEditor.Statistics.Tooltip')}
                inlineTooltip={true}
              />
            }
          >
            <TextRow
              title={t('CustomPhaseEditor.Statistics.Timer')}
              secondaryText={`
                ${trafficLightGroup.currentPhaseIndex == index ? trafficLightGroup.timer : 0}
                / 
                ${(trafficLightGroup.currentPhaseIndex == index
                  ? Math.min(
                      Math.max(
                        trafficLightGroup.targetDuration,
                        customPhaseItem.minimumDuration,
                      ),
                      customPhaseItem.maximumDuration,
                    )
                  : customPhaseItem.minimumDuration
                ).toFixed(2)}s
              `}
              dim={true}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.Priority')}
              secondaryText={`${customPhaseItem.priority}`}
              dim={true}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.LastRun')}
              secondaryText={`${customPhaseItem.turnsSinceLastRun}`}
              dim={true}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.CarFlow')}
              secondaryText={`${customPhaseItem.carFlow.toFixed(8)}`}
              dim={true}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.CarLaneOccupied')}
              secondaryText={`${customPhaseItem.carLaneOccupied}`}
              dim={true}
              tooltip={t(
                'CustomPhaseEditor.Statistics.CarLaneOccupied.Tooltip',
              )}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.PublicCarLaneOccupied')}
              secondaryText={`${customPhaseItem.publicCarLaneOccupied}`}
              dim={true}
              tooltip={t(
                'CustomPhaseEditor.Statistics.PublicCarLaneOccupied.Tooltip',
              )}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.TrackLaneOccupied')}
              secondaryText={`${customPhaseItem.trackLaneOccupied}`}
              dim={true}
              tooltip={t(
                'CustomPhaseEditor.Statistics.TrackLaneOccupied.Tooltip',
              )}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.CrossWalkOccupied')}
              secondaryText={`${customPhaseItem.pedestrianLaneOccupied}`}
              dim={true}
              tooltip={t(
                'CustomPhaseEditor.Statistics.CrossWalkOccupied.Tooltip',
              )}
            />
            <TextRow
              title={t('CustomPhaseEditor.Statistics.WeightedWaiting')}
              secondaryText={`${customPhaseItem.weightedWaiting.toFixed(2)}`}
              dim={true}
            />
            {trafficLightGroup.manualPhaseIndex < 0 &&
              trafficLightGroup.currentPhaseIndex == index && (
                <EndPhaseButton
                  index={index}
                  disabled={customPhaseItem.endPhasePrematurely}
                />
              )}
          </PanelFoldout>
        </div>
      </Scrollable>
    </PanelContainer>
  )
}
