import { ToolState } from "@/constants";
import { ClipboardContext } from "@/hooks/clipboard";
import { useTranslate } from "@/localisations"
import { bindValue, useValue } from "cs2/api";
import { useCallback, useContext, useEffect, useMemo, useRef, useState } from "react"
import styled from "styled-components"
import Row from "../main-panel/items/row";
import { EdgeGroupMaskContextClipboard, SubLaneGroupMaskContextClipboard } from "@/context";
import Delete from "../common/icons/delete";
import EdgeViewer from "../common/edge-viewer";
import SublaneViewer from "../common/sublane-viewer";
import { Dropdown, DropdownItem, DropdownToggle, PanelFoldout } from "cs2/ui";
import { getModule } from "cs2/modding";
import Tooltip from "../common/tooltip";

const Container = styled.div`
    max-width: 18em;
    width: 18em;
    max-height: 56em;
    background-color: var(--panelColorNormal);
    backdrop-filter: var(--panelBlur);
    color: var(--textColor);
    position: fixed;
    overflow-y: scroll;
    top: calc(10rem + var(--floatingToggleSize));
    right: 4em;
    z-index: 1001;
    border-radius: 4rem;
`;

const Header = styled.div`
    height: 2em;
    background-color: var(--panelColorDark);
    display: flex;
    justify-content: center;
    align-items: center;
`;

const IconContainer = styled.div<{ disabled?: boolean }>`
  display: flex;
  margin-left: 0.35em;
  border-radius: 0.2em;
  &:hover {
    filter: ${props => props.disabled ? "none" : "brightness(1.2) contrast(1.2)"};
    background: ${props => props.disabled ? "transparent" : "rgba(0, 0, 0, 0.1)"};
  }
`;

export default function ClipboardPanel() {
    const { t } = useTranslate()

    const [displayPhaseConfig, setDisplayPhaseConfig] = useState<'auto' | number>('auto')
    const currentPhaseCount = (JSON.parse(useValue(bindValue("TrafficLightManager", "GetCustomPhaseItems"))) as CustomPhaseItem[]).length;
    const displayPhaseCount = useMemo(
        () => displayPhaseConfig === 'auto' ? currentPhaseCount : displayPhaseConfig,
        [displayPhaseConfig, currentPhaseCount]
    );

    const [showPanel, setShowPanel] = useState(false);

    const [top, setTop] = useState(-1000)
    const [right, setRight] = useState(-1000)
    const [dragging, setDragging] = useState(false)
    const containerRef = useRef<HTMLDivElement>(null)

    const toolState = JSON.parse(useValue(bindValue("TrafficLightManager", "GetToolState"))) as ToolState;

    useEffect(() => {
        setShowPanel(toolState === ToolState.Editing);
    }, [toolState]);

    const mouseDownHandler = useCallback((_event: React.MouseEvent<HTMLElement>) => {
        if (containerRef.current) {
            const rect = containerRef.current.getBoundingClientRect()
            setTop(rect.top)
            setRight(document.body.clientWidth - rect.right)
            setDragging(true)
        }
    }, [setTop, setRight, setDragging, containerRef.current]);
    const mouseUpHandler = useCallback((_event: MouseEvent) => {
        setDragging(false)
    }, [containerRef.current])
    const mouseMoveHandler = useCallback((event: MouseEvent) => {
        setTop((prev) => {
            console.log(prev, event.movementY)
            return prev + event.movementY
        })
        setRight((prev) => prev - event.movementX)
    }, [setTop, setRight])
    useEffect(() => {
        if (dragging) {
            document.body.addEventListener("mouseup", mouseUpHandler)
            document.body.addEventListener("mousemove", mouseMoveHandler)
            return () => {
                document.body.removeEventListener("mouseup", mouseUpHandler)
                document.body.removeEventListener("mousemove", mouseMoveHandler)
            }
        }
    }, [dragging, mouseUpHandler, mouseMoveHandler])

    const style: React.CSSProperties = useMemo(() => {
        const result: React.CSSProperties = {
            display: showPanel ? "block" : "none"
        };
        const toolSideColumn = document.querySelector(".tool-side-column_l9i");
        if (containerRef.current && toolSideColumn) {
            const containerRect = containerRef.current.getBoundingClientRect();
            const toolSideColumnRect = toolSideColumn.getBoundingClientRect();
            if (top > -1000 && right > -1000) {
                result.top = Math.min(top, toolSideColumnRect.top - 200);
                result.right = Math.min(right, document.body.clientWidth - containerRect.width);
                result.top = Math.max(result.top, 0);
                result.right = Math.max(result.right, 0);
            }
        }
        return result;
    }, [showPanel, top, right, containerRef.current]);

    return (
        <Container ref={containerRef} style={style}>
            <Header>
                <span onMouseDown={mouseDownHandler}>
                    {t("ClipboardPanel.Title")}
                </span>
                <Dropdown
                    theme={getModule("game-ui/menu/themes/dropdown.module.scss", "classes")}
                    content={
                        (['auto', ...Array.from({ length: 16 }, (_, index) => index + 1)] as ('auto' | number)[]).map((item) => (
                            <DropdownItem
                                key={item}
                                value={item}
                                onChange={(value) => {
                                    setDisplayPhaseConfig(value);
                                }}
                            >
                                {item}
                            </DropdownItem>
                        ))
                    }
                >
                    <Tooltip position={"bottom"} tooltip={t("ClipboardPanel.DisplayPhaseCountTooltip")}>
                        <DropdownToggle style={{ width: '6em', height: '1.5em' }}>
                            {displayPhaseConfig}
                        </DropdownToggle>
                    </Tooltip>
                </Dropdown>
            </Header>
            <ClipboardViewer displayPhaseCount={displayPhaseCount} />
        </Container>
    )
}

function ClipboardViewer(props: { displayPhaseCount: number }) {
    const { t } = useTranslate()

    const [currentView, setCurrentView] = useState<"edge" | "subLane">("edge")
    const edgeClipboard = useContext(EdgeGroupMaskContextClipboard.context)
    const subLaneClipboard = useContext(SubLaneGroupMaskContextClipboard.context)
    const clipboard = useMemo(() => {
        return currentView === "edge" ? edgeClipboard : subLaneClipboard
    }, [currentView, edgeClipboard, subLaneClipboard])

    return (
        <div>
            <Row>
                <Row onClick={() => setCurrentView("edge")} style={{
                    width: "50%",
                    backgroundColor: currentView === "edge" ? "var(--panelColorDark)" : "transparent"
                }} hoverEffect={true}>
                    {t("ClipboardPanel.EdgeHistory")}
                </Row>
                <Row onClick={() => setCurrentView("subLane")} style={{
                    width: "50%",
                    backgroundColor: currentView === "subLane" ? "var(--panelColorDark)" : "transparent"
                }} hoverEffect={true}>
                    {t("ClipboardPanel.SubLaneHistory")}
                </Row>
            </Row>
            <ClipboardList clipboard={clipboard} displayPhaseCount={props.displayPhaseCount} />
        </div>
    )
}

function ClipboardList(props: { clipboard: ClipboardContext<EdgeGroupMask> | ClipboardContext<SubLaneGroupMask>, displayPhaseCount: number }) {
    const { clipboard } = props;
    return (
        <>
            {
                clipboard.history.map((item, index) => (
                    <ClipboardItem
                        key={index}
                        item={item}
                        index={index}
                        deleteHandler={() => clipboard.deleteAt(index)}
                        selectHandler={() => clipboard.select(index)}
                        displayPhaseCount={props.displayPhaseCount}
                    />
                ))
            }
        </>
    )
}

function ClipboardItem(props: { item: EdgeGroupMask | SubLaneGroupMask, index: number, deleteHandler: () => void, selectHandler: () => void, displayPhaseCount: number }) {
    const { item, index, deleteHandler, selectHandler } = props;
    console.log(item)
    return (
        <Row onClick={selectHandler} style={{ flexDirection: "column" }}>
            <PanelFoldout
                header={
                    <Row style={{ justifyContent: "space-between" }}>
                        {`#${index}`}
                        <IconContainer>
                            <Delete onClick={deleteHandler} style={{ color: "var(--textColorDim)", height: "1.1em", width: "1.1em", fontSize: "1.1em" }} />
                        </IconContainer>
                    </Row>
                }
            >
                <Row>
                    {
                        "m_Edge" in item ?
                            <EdgeItem groupMask={item} displayPhaseCount={props.displayPhaseCount} /> :
                            <SubLaneItem groupMask={item} displayPhaseCount={props.displayPhaseCount} />
                    }
                </Row>
            </PanelFoldout>
        </Row>
    )
}

function EdgeItem(props: { groupMask: EdgeGroupMask, displayPhaseCount: number }) {
    const { groupMask, displayPhaseCount } = props;
    return (
        <Row style={{ flexDirection: "column", alignItems: "flex-start" }}>
            {
                Array.from({ length: displayPhaseCount }, (_, i) => i).map(
                    (phaseIndex) =>
                        <EdgeViewer
                            data={groupMask}
                            displayOptions={
                                {
                                    carLane: {
                                        left: true,
                                        straight: true,
                                        right: true,
                                        uTurn: true
                                    },
                                    publicCarLane: {
                                        left: true,
                                        straight: true,
                                        right: true,
                                        uTurn: true
                                    },
                                    trackLane: {
                                        left: true,
                                        straight: true,
                                        right: true
                                    },
                                    pedestrianLaneStopLine: true,
                                    pedestrianLaneNonStopLine: true
                                }
                            }
                            index={phaseIndex}
                        />
                )
            }
        </Row>
    )
}

function SubLaneItem(props: { groupMask: SubLaneGroupMask, displayPhaseCount: number }) {
    const { groupMask, displayPhaseCount } = props;
    return (
        <Row style={{ flexDirection: "column", alignItems: "flex-start" }}>
            {
                Array.from({ length: displayPhaseCount }, (_, i) => i).map(
                    (phaseIndex) =>
                        <SublaneViewer
                            subLane={groupMask}
                            displayOptions={{
                                carLane: {
                                    left: true,
                                    straight: true,
                                    right: true,
                                    uTurn: true
                                },
                                trackLane: {
                                    left: true,
                                    straight: true,
                                    right: true
                                },
                                pedestrianLaneStopLine: true
                            }}
                            index={phaseIndex}
                        />
                )
            }
        </Row>
    )
}