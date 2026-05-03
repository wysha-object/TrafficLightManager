import { useCallback, useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';

import engine from 'cohtml/cohtml';
import { bindValue, trigger, useValue } from 'cs2/api';

import Header from './header';

import FloatingButton from '@/components/common/floating-button';
import CustomPhaseMainPanel from '@/components/custom-phase-tool/main-panel';
import { useTranslate } from '@/localisations';
import { ToolState } from '@/constants';
import Row from './items/row';
import Message from './items/message';
import Button from '../common/button';

const HeaderContainer = styled.div`
  position: absolute;
  top: calc(10rem + var(--floatingToggleSize));
  left: 0rem;
  border-radius: 4rem;
  overflow: hidden;
  margin: -10rem 0 0 -10rem;
  padding: 10rem 10rem 6rem 10rem;
`;

const Container = styled.div`
  width: 20em;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 0.25em;
  overflow-y: scroll;
`;

const BackButton = () => {
  return (
    <Row>
      <Button label="CustomPhaseEditor.Back" onClick={() => trigger("TrafficLightManager", "SetToolState", ToolState.Choosed)} />
    </Row>
  )
}

export default function MainPanel() {
  const [showFloatingButton, _] = useState(true);
  const [showPanel, setShowPanel] = useState(false);

  const [top, setTop] = useState(-999999);
  const [left, setLeft] = useState(-999999);
  const [dragging, setDragging] = useState(false);
  const [recalc, setRecalc] = useState({});

  const [container, setContainer] = useState<Element | null>(null);
  const [toolSideColumn, setToolSideColumn] = useState<Element | null>(null);

  const toolState = JSON.parse(useValue(bindValue("TrafficLightManager", "GetToolState"))) as ToolState;

  const containerRef = useCallback((el: Element | null) => setContainer(el), []);

  useEffect(() => {
    setShowPanel(toolState !== ToolState.Disabled);
  }, [toolState]);

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
    if (toolState !== ToolState.Disabled) {
      trigger("TrafficLightManager", "SetToolState", ToolState.Disabled);
    } else {
      trigger("TrafficLightManager", "SetToolState", ToolState.ChooseGroup);
    }
  }, [toolState]);

  const mouseDownHandler = useCallback((_event: React.MouseEvent<HTMLElement>) => {
    if (container) {
      const rect = container.getBoundingClientRect();
      setTop(rect.top);
      setLeft(rect.left);
      setDragging(true);
    }
  }, [container]);
  const mouseUpHandler = useCallback((_event: MouseEvent) => {
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
      if (top > -999999 && left > -999999) {
        result.top = Math.min(top, toolSideColumnRect.top - 200);
        result.left = Math.min(left, document.body.clientWidth - containerRect.width);
        result.top = Math.max(result.top, 0);
        result.left = Math.max(result.left, 0);
      }
    }
    return result;
  }, [showPanel, top, left, container, toolSideColumn, recalc]); // Recalc values on recalc or panel change

  const { t } = useTranslate();
  return (
    <>
      <FloatingButton
        show={showFloatingButton}
        src="Media/Game/Icons/TrafficLights.svg"
        tooltip={t("TrafficLightManager")}
        onClick={floatingButtonClickHandler}
      />
      <HeaderContainer
        ref={containerRef}
        style={style}
      >
        <Header title={t("TrafficLightManager")} image={"Media/Game/Icons/TrafficLights.svg"} onMouseDown={mouseDownHandler} />
        {[ToolState.Choosed, ToolState.Editing].includes(toolState) &&
          <CustomPhaseMainPanel />
          ||
          <Container>
            <Row>
              <Message message={
                new Map(
                  [
                    [ToolState.ChooseGroup, t("CustomPhaseEditor.ChooseGroup")],
                    [ToolState.AddTrafficLights, t("CustomPhaseEditor.AddTrafficLights")],
                    [ToolState.RemoveTrafficLights, t("CustomPhaseEditor.RemoveTrafficLights")]
                  ]).get(toolState) ?? ""} />
            </Row>
            {
              [ToolState.AddTrafficLights, ToolState.RemoveTrafficLights].includes(toolState) &&
              <Row>
                <BackButton />
              </Row>
            }
          </Container>
        }
      </HeaderContainer>
    </>
  );
}