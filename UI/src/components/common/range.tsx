import { useCallback, useEffect, useRef, useState } from 'react';
import styled from 'styled-components';

const RangeComponent = styled.div`
  padding: 0.25em 0;
  width: 100%;
`;

const Track = styled.div`
  background-color: rgba(255, 255, 255, 0.5);
  border-radius: 0.25em;
  width: 100%;
  height: 0.5em;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  flex-direction: row;
  padding: 0 0.5em;
`;

const Filler = styled.div`
  background-color: var(--accentColorNormal);
  box-shadow: var(--accentColorNormal) -0.5em 0;
  border-radius: 0.25em 0 0 0.25em;
  height: 0.5em;
`;

const Thumb = styled.div<{active: boolean}>`
  background-color: var(--textColor);
  border-radius: 50%;
  width: 1.0em;
  height: 1.0em;
  margin-left: -0.5em;
  transform: ${props => props.active ? "scale3d(1.1, 1.1, 1)" : "none"};
  &:hover {
    transform: scale3d(1.1, 1.1, 1);
  }
`;

export default function Range(props: {
  data: {min: number, max: number, step: number, value: number},
  onChange?: (value: number) => void,
  onUpdate?: (value: number) => void
}) {
  const data = props.data;

  const [dragging, setDragging] = useState(false);
  const [value, setValue] = useState(0);
  const sliderRef = useRef<HTMLDivElement>(null);

  const getNewValue = useCallback((clientX: number) => {
    let sliderLeft = 0;
    let sliderWidth = 0;
    if (sliderRef.current) {
      const rect = sliderRef.current.getBoundingClientRect();
      sliderLeft = rect.left;
      sliderWidth = rect.right - rect.left;
    }
    let newValue = (Math.round((((clientX - sliderLeft) / sliderWidth) * (data.max - data.min)) / data.step) * data.step) + data.min;
    if (newValue < data.min) {
      newValue = data.min;
    }
    if (newValue > data.max) {
      newValue = data.max;
    }
    return newValue;
  }, [data]);

  const mouseDownHandler = (_event: React.MouseEvent<HTMLElement>) => {
    setDragging(true);
  };
  const mouseUpHandler = useCallback((event: MouseEvent) => {
    const newValue = getNewValue(event.clientX);
    setValue(newValue);
    setDragging(false);
    if (props.onChange) {
      props.onChange(newValue);
    }
  }, [props, getNewValue]);
  const mouseMoveHandler = useCallback((event: MouseEvent) => {
    const newValue = getNewValue(event.clientX);
    setValue(newValue);
    if (props.onUpdate) {
      props.onUpdate(newValue);
    }
  }, [props, getNewValue]);

  useEffect(() => {
    if (dragging) {
      document.body.addEventListener("mouseup", mouseUpHandler);
      document.body.addEventListener("mousemove", mouseMoveHandler);
      return () => {
        document.body.removeEventListener("mouseup", mouseUpHandler);
        document.body.removeEventListener("mousemove", mouseMoveHandler);
      };
    }
  }, [dragging, mouseMoveHandler, mouseUpHandler]);

  useEffect(() => {
    if (!dragging) {
      if (data.value < data.min || isNaN(data.value)) {
        setValue(data.min);
      } else if (data.value > data.max) {
        setValue(data.max);
      } else {
        setValue(data.value);
      }
    }
  }, [data, dragging]);

  const sliderValue = (value - data.min) / (data.max - data.min) * 100;

  return (
    <RangeComponent onMouseDown={mouseDownHandler}>
      <Track ref={sliderRef}>
        <Filler style={{width: sliderValue + "%"}} />
        <Thumb active={dragging} />
      </Track>
    </RangeComponent>
  );
}