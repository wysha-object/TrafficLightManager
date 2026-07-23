import {
  CSSProperties,
  DetailedHTMLProps,
  HTMLAttributes,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react'
import { createPortal } from 'react-dom'

const bottomStyle: CSSProperties = {
  marginTop: '0.25em',
  transform: 'translate(-50%, 0)',
}

const topEndStyle: CSSProperties = {
  marginBottom: '0.25em',
}

const rightStyle: CSSProperties = {
  marginLeft: '0.25em',
  transform: 'translate(0, -50%)',
}

const rightStartStyle: CSSProperties = {
  marginLeft: '0.25em',
}

const bottomStartStyle: CSSProperties = {
  marginTop: '0.25em',
}

export default function TipArea(
  props: DetailedHTMLProps<HTMLAttributes<HTMLDivElement>, HTMLDivElement> & {
    position: 'top-end' | 'right' | 'right-start' | 'bottom' | 'bottom-start'
    tooltip: React.ReactNode
  },
) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [show, setShow] = useState(false)
  const [tooltipStyle, setTooltipStyle] = useState<CSSProperties>({})

  const showTooltip = useCallback(() => {
    if (containerRef.current && !show) {
      const element = containerRef.current
      const rect = element.getBoundingClientRect()
      if (props.position == 'top-end') {
        setTooltipStyle({
          right: window.innerWidth - rect.right,
          bottom: window.innerHeight - rect.top,
          ...topEndStyle,
        })
      } else if (props.position == 'right') {
        setTooltipStyle({
          top: rect.top + rect.height / 2,
          left: rect.right,
          ...rightStyle,
        })
      } else if (props.position == 'right-start') {
        setTooltipStyle({
          top: rect.top,
          left: rect.right,
          ...rightStartStyle,
        })
      } else if (props.position == 'bottom') {
        setTooltipStyle({
          top: rect.bottom,
          left: rect.left + rect.width / 2,
          ...bottomStyle,
        })
      } else if (props.position == 'bottom-start') {
        setTooltipStyle({
          top: rect.bottom,
          left: rect.left,
          ...bottomStartStyle,
        })
      }
      setShow(true)
    }
  }, [containerRef, show, props.position])

  const hideTooltip = useCallback(() => {
    setShow(false)
  }, [])

  // Workaround for mouseleave not firing reliably
  useEffect(() => {
    if (containerRef.current && show) {
      const mouseMoveHandler = (e: MouseEvent) => {
        if (containerRef.current) {
          const rect = containerRef.current.getBoundingClientRect()
          const isOutsideHeader =
            e.clientX < rect.left ||
            e.clientX > rect.right ||
            e.clientY < rect.top ||
            e.clientY > rect.bottom
          if (isOutsideHeader) {
            hideTooltip()
          }
        }
      }
      document.body.addEventListener('mousemove', mouseMoveHandler)
      return () =>
        document.body.removeEventListener('mousemove', mouseMoveHandler)
    }
  }, [containerRef, show, hideTooltip])

  return (
    <>
      <div ref={containerRef} onMouseEnter={showTooltip} {...props}>
        {props.children}
      </div>
      {show &&
        createPortal(
          <div
            style={{
              maxWidth: '30vw',
              padding: '0.5em 0.5em',
              margin: 0,
              position: 'fixed',
              textAlign: 'center',
              borderRadius: '4rem',
              fontSize: 'var(--fontSizeS)',
              color: 'var(--textColorDim)',
              backgroundColor: 'var(--tooltipColor)',
              filter: 'var(--tooltipFilter)',
              zIndex: 100,
              ...tooltipStyle,
            }}
          >
            {props.tooltip}
          </div>,
          document.body,
        )}
    </>
  )
}
