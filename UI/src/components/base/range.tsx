import { useCallback, useEffect, useRef, useState } from 'react'
import styled from 'styled-components'

const RangeComponent = styled.div`
  padding: 0.25em 0;
  width: 100%;
`

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
`

const Filler = styled.div`
  background-color: var(--accentColorNormal);
  box-shadow: var(--accentColorNormal) -0.5em 0;
  border-radius: 0.25em 0 0 0.25em;
  height: 0.5em;
`

const Thumb = styled.div<{ active: boolean }>`
  background-color: var(--textColor);
  border-radius: 50%;
  width: 1em;
  height: 1em;
  margin-left: -0.5em;
  transform: ${(props) => (props.active ? 'scale3d(1.1, 1.1, 1)' : 'none')};
  &:hover {
    transform: scale3d(1.1, 1.1, 1);
  }
`

export default function Range(props: {
  min: number
  max: number
  step: number
  value: number
  onChange?: (value: number) => void
  onUpdate?: (value: number) => void
}) {
  const [dragging, setDragging] = useState(false)
  const [value, setValue] = useState(0)
  const sliderRef = useRef<HTMLDivElement>(null)

  const getNewValue = useCallback(
    (clientX: number) => {
      let sliderLeft = 0
      let sliderWidth = 0
      if (sliderRef.current) {
        const rect = sliderRef.current.getBoundingClientRect()
        sliderLeft = rect.left
        sliderWidth = rect.right - rect.left
      }
      let newValue =
        Math.round(
          (((clientX - sliderLeft) / sliderWidth) * (props.max - props.min)) /
            props.step,
        ) *
          props.step +
        props.min
      if (newValue < props.min) {
        newValue = props.min
      }
      if (newValue > props.max) {
        newValue = props.max
      }
      return newValue
    },
    [props.min, props.max, props.step],
  )

  const mouseDownHandler = (_event: React.MouseEvent<HTMLElement>) => {
    setDragging(true)
  }
  const mouseUpHandler = useCallback(
    (event: MouseEvent) => {
      const newValue = getNewValue(event.clientX)
      setValue(newValue)
      setDragging(false)
      if (props.onChange) {
        props.onChange(newValue)
      }
    },
    [props, getNewValue],
  )
  const mouseMoveHandler = useCallback(
    (event: MouseEvent) => {
      const newValue = getNewValue(event.clientX)
      setValue(newValue)
      if (props.onUpdate) {
        props.onUpdate(newValue)
      }
    },
    [props, getNewValue],
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
  }, [dragging, mouseMoveHandler, mouseUpHandler])

  useEffect(() => {
    if (!dragging) {
      if (props.value < props.min || isNaN(props.value)) {
        setValue(props.min)
      } else if (props.value > props.max) {
        setValue(props.max)
      } else {
        setValue(props.value)
      }
    }
  }, [props.value, props.min, props.max, dragging])

  const sliderValue = ((value - props.min) / (props.max - props.min)) * 100

  return (
    <RangeComponent onMouseDown={mouseDownHandler}>
      <Track ref={sliderRef}>
        <Filler style={{ width: sliderValue + '%' }} />
        <Thumb active={dragging} />
      </Track>
    </RangeComponent>
  )
}
