import DeleteSvg from 'assets/images/delete.svg'
import { ClipboardContext, ClipboardHistoryItem } from 'hooks/clipboard'
import { useTranslate } from 'hooks/translate'
import {
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import {
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
} from 'context'
import EdgeViewer from '../components/shared/edge-viewer'
import SublaneViewer from '../components/shared/sublane-viewer'
import {
  Button,
  Dropdown,
  DropdownItem,
  DropdownToggle,
  PanelFoldout,
  Scrollable,
} from 'cs2/ui'
import { getModule } from 'cs2/modding'
import TipArea from '../components/base/tip-area'
import { EdgeGroupMask, SubLaneGroupMask, ToolState } from 'types'
import classNames from 'classnames'
import BasePage from 'components/base/base-page'
import styled from 'styled-components'
import { useGetCustomPhaseItemsCmd, useGetToolStateCmd } from 'hooks/cmds'

export default function ClipboardPanel() {
  const { t } = useTranslate()

  const [displayPhaseConfig, setDisplayPhaseConfig] = useState<'auto' | number>(
    'auto',
  )
  const customPhaseItems = useGetCustomPhaseItemsCmd()
  const currentPhaseCount = useMemo(
    () => customPhaseItems.length,
    [customPhaseItems],
  )
  const displayPhaseCount = useMemo(
    () =>
      displayPhaseConfig === 'auto' ? currentPhaseCount : displayPhaseConfig,
    [displayPhaseConfig, currentPhaseCount],
  )

  const [showPanel, setShowPanel] = useState(false)

  const [top, setTop] = useState(-1000)
  const [right, setRight] = useState(-1000)
  const [dragging, setDragging] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  const toolState = useGetToolStateCmd()

  useEffect(() => {
    setShowPanel(toolState !== ToolState.Disabled)
  }, [toolState])

  const mouseDownHandler = useCallback(
    (_event: React.MouseEvent<HTMLElement>) => {
      if (containerRef.current) {
        const rect = containerRef.current.getBoundingClientRect()
        setTop(rect.top)
        setRight(document.body.clientWidth - rect.right)
        setDragging(true)
      }
    },
    [setTop, setRight, setDragging, containerRef.current],
  )
  const mouseUpHandler = useCallback(
    (_event: MouseEvent) => {
      setDragging(false)
    },
    [containerRef.current],
  )
  const mouseMoveHandler = useCallback(
    (event: MouseEvent) => {
      setTop((prev) => {
        console.log(prev, event.movementY)
        return prev + event.movementY
      })
      setRight((prev) => prev - event.movementX)
    },
    [setTop, setRight],
  )
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
    const toolSideColumn = document.querySelector('.tool-side-column_l9i')
    if (containerRef.current && toolSideColumn) {
      const containerRect = containerRef.current.getBoundingClientRect()
      const toolSideColumnRect = toolSideColumn.getBoundingClientRect()
      if (top > -1000 && right > -1000) {
        result.top = Math.min(top, toolSideColumnRect.top - 200)
        result.right = Math.min(
          right,
          document.body.clientWidth - containerRect.width,
        )
        result.top = Math.max(result.top, 0)
        result.right = Math.max(result.right, 0)
      }
    }
    return result
  }, [showPanel, top, right, containerRef.current])

  return (
    <BasePage
      ref={containerRef}
      style={{
        maxWidth: '16em',
        width: '16em',
        right: '4em',
        ...style,
      }}
      header={
        <div
          style={{
            display: 'flex',
            flexDirection: 'row',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <div
            onMouseDown={mouseDownHandler}
            style={{ color: 'var(--accentColorNormal)' }}
          >
            {t('ClipboardPanel.Title')}
          </div>
          <Dropdown
            theme={getModule(
              'game-ui/menu/themes/dropdown.module.scss',
              'classes',
            )}
            content={(
              [
                'auto',
                ...Array.from({ length: 16 }, (_, index) => index + 1),
              ] as ('auto' | number)[]
            ).map((item) => (
              <DropdownItem
                key={item}
                value={item}
                onChange={(value) => {
                  setDisplayPhaseConfig(value)
                }}
              >
                {item}
              </DropdownItem>
            ))}
          >
            <TipArea
              position={'top-end'}
              tooltip={t('ClipboardPanel.DisplayPhaseCountTooltip')}
            >
              <DropdownToggle style={{ width: '6em', height: '1.5em' }}>
                {displayPhaseConfig}
              </DropdownToggle>
            </TipArea>
          </Dropdown>
        </div>
      }
    >
      <ClipboardViewer displayPhaseCount={displayPhaseCount} />
    </BasePage>
  )
}

function ClipboardViewer(props: { displayPhaseCount: number }) {
  const { t } = useTranslate()

  const [currentView, setCurrentView] = useState<'edge' | 'sub-lane'>('edge')
  const edgeClipboard = useContext(EdgeGroupMaskContextClipboard.context)
  const subLaneClipboard = useContext(SubLaneGroupMaskContextClipboard.context)
  const clipboard = useMemo(() => {
    return currentView === 'edge' ? edgeClipboard : subLaneClipboard
  }, [currentView, edgeClipboard, subLaneClipboard])

  return (
    <div
      style={{
        backgroundColor: 'var(--panelColorNormal)',
      }}
    >
      <div className='row' style={{ paddingBottom: '0' }}>
        <Button
          variant='flat'
          className={classNames({
            'top-option-button': true,
            selected: currentView === 'edge',
          })}
          onClick={() => setCurrentView('edge')}
          style={{
            flex: 1,
          }}
        >
          {t('ClipboardPanel.EdgeHistory')}
        </Button>
        <Button
          variant='flat'
          className={classNames({
            'top-option-button': true,
            selected: currentView === 'sub-lane',
          })}
          onClick={() => setCurrentView('sub-lane')}
          style={{
            flex: 1,
          }}
        >
          {t('ClipboardPanel.SubLaneHistory')}
        </Button>
      </div>
      <div className='horizontal-divider' />
      <Scrollable>
        <ClipboardList
          clipboard={clipboard}
          displayPhaseCount={props.displayPhaseCount}
        />
      </Scrollable>
    </div>
  )
}

const ClipboardListContainer = styled.div`
  backdrop-filter: var(--panelBlur);
`

function ClipboardList(props: {
  clipboard:
    ClipboardContext<EdgeGroupMask> | ClipboardContext<SubLaneGroupMask>
  displayPhaseCount: number
}) {
  return (
    <ClipboardListContainer>
      {props.clipboard.history.map((item, index) => (
        <ClipboardItem
          key={index}
          item={item}
          index={index}
          deleteHandler={() => props.clipboard.deleteAt(index)}
          selectHandler={() => props.clipboard.selectAt(index)}
          displayPhaseCount={props.displayPhaseCount}
        />
      ))}
    </ClipboardListContainer>
  )
}

function ClipboardItem(props: {
  item:
    ClipboardHistoryItem<EdgeGroupMask> | ClipboardHistoryItem<SubLaneGroupMask>
  index: number
  deleteHandler: () => void
  selectHandler: () => void
  displayPhaseCount: number
}) {
  return (
    <div onClick={props.selectHandler} style={{ flexDirection: 'column' }}>
      <PanelFoldout
        header={
          <div className='row' style={{ justifyContent: 'space-between' }}>
            {`#${props.index}`}
            <Button variant='round' onClick={props.deleteHandler}>
              <DeleteSvg />
            </Button>
          </div>
        }
      >
        <div className='horizontal-divider-with-gap' />
        <div style={{ flexDirection: 'column' }}>
          {Array.from({ length: props.displayPhaseCount }, (_, i) => i).map(
            (phaseIndex) => (
              <div
                className='row'
                style={{ flexDirection: 'column', alignItems: 'flex-start' }}
              >
                {'m_Edge' in props.item.value ? (
                  <EdgeViewer
                    data={props.item.value as EdgeGroupMask}
                    displayOptions={{
                      carLane: {
                        left: true,
                        straight: true,
                        right: true,
                        uTurn: true,
                      },
                      publicCarLane: {
                        left: true,
                        straight: true,
                        right: true,
                        uTurn: true,
                      },
                      trackLane: {
                        left: true,
                        straight: true,
                        right: true,
                      },
                      pedestrianLaneStopLine: true,
                      pedestrianLaneNonStopLine: true,
                    }}
                    index={phaseIndex}
                  />
                ) : (
                  <SublaneViewer
                    subLane={props.item.value as SubLaneGroupMask}
                    displayOptions={{
                      carLane: {
                        left: true,
                        straight: true,
                        right: true,
                        uTurn: true,
                      },
                      trackLane: {
                        left: true,
                        straight: true,
                        right: true,
                      },
                      pedestrianLaneStopLine: true,
                    }}
                    index={phaseIndex}
                  />
                )}
                <div className='horizontal-divider-with-gap' />
              </div>
            ),
          )}
        </div>
      </PanelFoldout>
    </div>
  )
}
