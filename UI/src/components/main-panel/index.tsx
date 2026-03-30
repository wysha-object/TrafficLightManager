import { useCallback, useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';

import engine from 'cohtml/cohtml';
import { bindValue, useValue } from 'cs2/api';

import { MainPanelState } from '@/constants';

import Header from './header';
import Content from './content';

import FloatingButton from '@/components/common/floating-button';
import CustomPhaseMainPanel from '@/components/custom-phase-tool/main-panel';

const defaultPanel = {
  title: "",
  image: "",
  position: {top: -999999, left: -999999},
  showPanel: false,
  showFloatingButton: false,
  state: 0,
  items: []
};

const useMainPanel = () => {
  const [panel, setPanel] = useState<MainPanel>(defaultPanel);

  const result = useValue(bindValue("C2VM.TLE", "GetMainPanel", "{}"));

  useEffect(() => {
    const newPanel = JSON.parse(result);
    setPanel({
      title: newPanel.title ?? defaultPanel.title,
      image: newPanel.image ?? defaultPanel.image,
      position: newPanel.position ?? defaultPanel.position,
      showPanel: newPanel.showPanel ?? defaultPanel.showPanel,
      showFloatingButton: newPanel.showFloatingButton ?? defaultPanel.showFloatingButton,
      state: newPanel.state ?? defaultPanel.state,
      items: newPanel.items ?? defaultPanel.items
    });
  }, [result]);

  return panel;
};

const Container = styled.div`
  position: absolute;
  top: calc(10rem + var(--floatingToggleSize));
  left: 0rem;
  border-radius: 4rem;
  overflow: hidden;
  margin: -10rem 0 0 -10rem;
  padding: 10rem 10rem 6rem 10rem;
`;

export default function MainPanel() {
  const [showFloatingButton, setShowFloatingButton] = useState(false);
  const [showPanel, setShowPanel] = useState(false);

  const [top, setTop] = useState(-999999);
  const [left, setLeft] = useState(-999999);
  const [dragging, setDragging] = useState(false);
  const [recalc, setRecalc] = useState({});

  const [container, setContainer] = useState<Element | null>(null);
  const [toolSideColumn, setToolSideColumn] = useState<Element | null>(null);

  const panel = useMainPanel();

  const containerRef = useCallback((el: Element | null) => setContainer(el), []);

  useEffect(() => {
    setShowPanel(panel.showPanel);
    setShowFloatingButton(panel.showFloatingButton);
    if (!dragging) {
      setTop(panel.position.top);
      setLeft(panel.position.left);
    }
  }, [panel.showPanel, panel.showFloatingButton, panel.position.top, panel.position.left, dragging]);

  // Save everything when the panel is closed
  useEffect(() => {
    return () => {
      engine.call("C2VM.TLE.CallMainPanelSave", "{}");
    };
  }, []);

  useEffect(() => {
    setToolSideColumn(document.querySelector(".tool-side-column_l9i"));
    if (container && showPanel) {
      const resizeObserver = new ResizeObserver(() => setRecalc({}));
      resizeObserver.observe(container);
      resizeObserver.observe(document.body);
      return () => resizeObserver.disconnect();
    }
  }, [container, showPanel]);

  const floatingButtonClickHandler = useCallback(() => {
    if (panel.showPanel) {
      engine.call("C2VM.TLE.CallSetMainPanelState", JSON.stringify({value: `${MainPanelState.Hidden}`}));
    } else {
      engine.call("C2VM.TLE.CallSetMainPanelState", JSON.stringify({value: `${MainPanelState.Empty}`}));
    }
  }, [panel.showPanel]);

  const mouseDownHandler = useCallback((_event: React.MouseEvent<HTMLElement>) => {
    if (container) {
      const rect = container.getBoundingClientRect();
      setTop(rect.top);
      setLeft(rect.left);
      setDragging(true);
    }
  }, [container]);
  const mouseUpHandler = useCallback((_event: MouseEvent) => {
    if (container) {
      const rect = container.getBoundingClientRect();
      engine.call("C2VM.TLE.CallMainPanelUpdatePosition", JSON.stringify({top: Math.floor(rect.top), left: Math.floor(rect.left)}));
    }
    setDragging(false);
  }, [container]);
  const mouseMoveHandler = useCallback((event: MouseEvent) => {
    setTop((prev) => prev + event.movementY);
    setLeft((prev) => prev + event.movementX);
  }, []);

  useEffect(() => {
    if (dragging) {
      document.body.addEventListener("mouseup", mouseUpHandler);
      document.body.addEventListener("mousemove", mouseMoveHandler);
      return () => {
        document.body.removeEventListener("mouseup", mouseUpHandler);
        document.body.removeEventListener("mousemove", mouseMoveHandler);
      };
    }
  }, [dragging, mouseUpHandler, mouseMoveHandler]);

  const style: React.CSSProperties = useMemo(() => {
    const result: React.CSSProperties = {
      display: showPanel ? "block" : "none"
    };
    if (container && toolSideColumn) {
      const containerRect = container.getBoundingClientRect();
      const toolSideColumnRect = toolSideColumn.getBoundingClientRect();
      result.maxHeight = Math.max(200, toolSideColumnRect.top - containerRect.top);
      if (top > -999999 && left > -999999) {
        result.top = Math.min(top, toolSideColumnRect.top - 200);
        result.left = Math.min(left, document.body.clientWidth - containerRect.width);
        result.top = Math.max(result.top, 0);
        result.left = Math.max(result.left, 0);
      }
    }
    return result;
  }, [showPanel, top, left, container, toolSideColumn, recalc, panel]); // Recalc values on recalc or panel change

  return (
    <>
      <FloatingButton
        show={showFloatingButton}
        src="Media/Game/Icons/TrafficLights.svg"
        tooltip={panel.title}
        onClick={floatingButtonClickHandler}
      />
      <Container
        ref={containerRef}
        style={style}
      >
        <Header title={panel.title} image={panel.image} onMouseDown={mouseDownHandler} />
        {panel.state != MainPanelState.CustomPhase && <Content items={panel.items} />}
        {panel.state == MainPanelState.CustomPhase && <CustomPhaseMainPanel items={panel.items} />}
      </Container>
    </>
  );
}