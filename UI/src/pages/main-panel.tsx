import { useCallback, useEffect, useMemo, useState } from 'react'
import styled from 'styled-components'
import { useTranslate } from 'hooks/translate'
import { Button, Tooltip } from 'cs2/ui'
import { ToolState } from 'types'
import PhaseChooserPanel from 'components/main-panel/phase-chooser-panel'
import PhaseDetailsPanel from 'components/main-panel/phase-details-panel'
import TrafficLightMembersPanel from 'components/main-panel/traffic-light-members-panel'
import LogoIcon from 'components/icon/logo-icon'
import BasePage from 'components/base/base-page'
import { setToolStateCmd, useGetToolStateCmd } from 'hooks/cmds'

const EditorContainer = styled.div`
  display: flex;
  flex-direction: row;
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
`

const LabelContainer = styled.div`
  width: 20em;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 0.25em;
  overflow-y: scroll;
`

const BackButton = () => {
  const { t } = useTranslate()
  return (
    <div className='row'>
      <Button
        style={{ width: '100%' }}
        variant='flat'
        onClick={() => setToolStateCmd(ToolState.Choosed)}
      >
        {t('CustomPhaseEditor.Back')}
      </Button>
    </div>
  )
}

export default function MainPanel() {
  const [showPanel, setShowPanel] = useState(false)

  const [top, setTop] = useState(-1000)
  const [left, setLeft] = useState(-1000)
  const [dragging, setDragging] = useState(false)
  const [recalc, setRecalc] = useState({})

  const [container, setContainer] = useState<Element | null>(null)
  const [toolSideColumn, setToolSideColumn] = useState<Element | null>(null)

  const toolState = useGetToolStateCmd()

  const containerRef = useCallback((el: Element | null) => setContainer(el), [])

  useEffect(() => {
    setShowPanel(toolState !== ToolState.Disabled)
  }, [toolState])

  useEffect(() => {
    setToolSideColumn(document.querySelector('.tool-side-column_l9i'))
    if (container && showPanel) {
      const resizeObserver = new ResizeObserver(() => setRecalc({}))
      resizeObserver.observe(container)
      resizeObserver.observe(document.body)
      return () => resizeObserver.disconnect()
    }
  }, [container, showPanel])

  const floatingButtonClickHandler = useCallback(() => {
    if (toolState !== ToolState.Disabled) {
      setToolStateCmd(ToolState.Disabled)
    } else {
      setToolStateCmd(ToolState.ChooseGroup)
    }
  }, [toolState])

  const mouseDownHandler = useCallback(
    (_event: React.MouseEvent<HTMLElement>) => {
      if (container) {
        const rect = container.getBoundingClientRect()
        setTop(rect.top)
        setLeft(rect.left)
        setDragging(true)
      }
    },
    [container],
  )
  const mouseUpHandler = useCallback(
    (_event: MouseEvent) => {
      setDragging(false)
    },
    [container],
  )
  const mouseMoveHandler = useCallback((event: MouseEvent) => {
    setTop((prev) => prev + event.movementY)
    setLeft((prev) => prev + event.movementX)
  }, [])

  useEffect(() => {
    if (dragging) {
      document.body.addEventListener('mouseup', mouseUpHandler)
      document.body.addEventListener('mousemove', mouseMoveHandler)
      return () => {
        document.body.removeEventListener('mouseup', mouseUpHandler)
        document.body.removeEventListener('mousemove', mouseMoveHandler)
      }
    }
  }, [dragging, mouseUpHandler, mouseMoveHandler])

  const style: React.CSSProperties = useMemo(() => {
    const result: React.CSSProperties = {
      display: showPanel ? 'block' : 'none',
    }
    if (container && toolSideColumn) {
      const containerRect = container.getBoundingClientRect()
      const toolSideColumnRect = toolSideColumn.getBoundingClientRect()
      if (top > -1000 && left > -1000) {
        result.top = Math.min(top, toolSideColumnRect.top - 200)
        result.left = Math.min(
          left,
          document.body.clientWidth - containerRect.width,
        )
        result.top = Math.max(result.top, 0)
        result.left = Math.max(result.left, 0)
      }
    }
    return result
  }, [showPanel, top, left, container, toolSideColumn, recalc]) // Recalc values on recalc or panel change

  const { t } = useTranslate()
  return (
    <>
      <Tooltip tooltip={t('TrafficLightManager')}>
        <Button variant='floating' onSelect={floatingButtonClickHandler}>
          <LogoIcon />
        </Button>
      </Tooltip>
      <BasePage
        ref={containerRef}
        style={{
          left: '0rem',
          ...style,
        }}
        header={
          <div onMouseDown={mouseDownHandler}>{t('TrafficLightManager')}</div>
        }
      >
        {([ToolState.Choosed, ToolState.Editing].includes(toolState) && (
          <EditorContainer>
            <TrafficLightMembersPanel />
            <div
              style={{
                width: '0.3em',
                backgroundColor: 'var(--panelColorDark)',
              }}
            ></div>
            <PhaseChooserPanel />
            <PhaseDetailsPanel />
          </EditorContainer>
        )) || (
          <LabelContainer>
            <div className='row' style={{ justifyContent: 'center' }}>
              <div style={{ margin: '20rem auto' }} />
              {new Map([
                [ToolState.ChooseGroup, t('CustomPhaseEditor.ChooseGroup')],
                [
                  ToolState.AddTrafficLights,
                  t('CustomPhaseEditor.AddTrafficLights'),
                ],
                [
                  ToolState.RemoveTrafficLights,
                  t('CustomPhaseEditor.RemoveTrafficLights'),
                ],
              ]).get(toolState) ?? ''}
            </div>
            {[
              ToolState.AddTrafficLights,
              ToolState.RemoveTrafficLights,
            ].includes(toolState) && (
              <div className='row'>
                <BackButton />
              </div>
            )}
          </LabelContainer>
        )}
      </BasePage>
    </>
  )
}
