import { CSSProperties, useCallback, useEffect, useRef, useState } from "react";
import styled from "styled-components";

const Container = styled.div`
  display: flex;
  flex-direction: row;
`;

const ContentContainer = styled.div`
  overflow-y: scroll;
`;

const ScrollbarTrack = styled.div`
  width: 0.2em;
  background-color: rgba(var(--scrollbarColor), 0.15);
  border-radius: 2rem;
`;

const ScrollbarThumb = styled.div<{active: boolean}>`
  position: relative;
  width: 0.2em;
  background-color: rgba(var(--scrollbarColor), 0.6);
  border-radius: 2rem;
  filter: ${props => props.active ? "brightness(1.2) contrast(1.2)" : "none"};
  &:hover {
    filter: brightness(1.2) contrast(1.2);
  }
`;

export default function Scrollable(props: {style?: CSSProperties, contentStyle?: CSSProperties, trackStyle?: CSSProperties, children?: React.ReactNode}) {
  const containerRef = useRef<HTMLDivElement>(null);
  const childrenRef = useRef<HTMLDivElement>(null);
  const trackRef = useRef<HTMLDivElement>(null);
  const [dragging, setDragging] = useState(false);
  const [showTrack, setShowTrack] = useState(false);
  const [thumbHeight, setThumbHeight] = useState(0);
  const [thumbTop, setThumbTop] = useState(0);
  const updateThumbPosition = useCallback(() => {
    if (containerRef.current) {
      const newThumbHeight = (containerRef.current.clientHeight / containerRef.current.scrollHeight) * 100;
      let newThumbTop = (containerRef.current.scrollTop / containerRef.current.scrollHeight) * 100;
      if (newThumbTop < 0) {
        newThumbTop = 0;
      } else if (newThumbTop + newThumbHeight > 100) {
        newThumbTop = 100 - newThumbHeight;
      }
      setThumbTop(newThumbTop);
      setThumbHeight(newThumbHeight);
      setShowTrack(containerRef.current.clientHeight + 10 < containerRef.current.scrollHeight);
    }
  }, []);
  const trackClickHandler = useCallback((event: React.MouseEvent<HTMLElement>) => {
    if (event.target !== event.currentTarget) {
      return;
    }
    if (containerRef.current && trackRef.current) {
      const rect = trackRef.current.getBoundingClientRect();
      const position = (((event.clientY - rect.top) / trackRef.current.clientHeight) - (thumbHeight / 100 * 0.5)) / (1 - (thumbHeight / 100));
      containerRef.current.scrollTop = position * (containerRef.current.scrollHeight - containerRef.current.clientHeight);
      updateThumbPosition();
    }
  }, [containerRef, trackRef, updateThumbPosition, thumbHeight]);
  const mouseUpHandler = useCallback((_event: MouseEvent) => {
    setDragging(false);
  }, []);
  const mouseMoveHandler = useCallback((event: MouseEvent) => {
    if (containerRef.current) {
      containerRef.current.scrollTop += event.movementY * (containerRef.current.scrollHeight / containerRef.current.clientHeight);
      updateThumbPosition();
    }
  }, [containerRef, updateThumbPosition]);
  useEffect(() => {
    if (containerRef.current && childrenRef.current) {
      const resizeObserver = new ResizeObserver(() => {
        updateThumbPosition();
      });
      resizeObserver.observe(containerRef.current);
      resizeObserver.observe(childrenRef.current);
      return () => resizeObserver.disconnect();
    }
  }, [containerRef, childrenRef, updateThumbPosition]);
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
  return (
    <Container style={props.style}>
      <ContentContainer style={props.contentStyle} ref={containerRef} onScroll={updateThumbPosition}>
        <div ref={childrenRef}>{props.children}</div>
      </ContentContainer>
      {showTrack && <ScrollbarTrack onClick={trackClickHandler} ref={trackRef} style={props.trackStyle}>
        <ScrollbarThumb
          active={dragging}
          onMouseDown={() => setDragging(true)}
          style={{top: thumbTop + "%", height: thumbHeight + "%"}}
        />
      </ScrollbarTrack>}
    </Container>
  );
}