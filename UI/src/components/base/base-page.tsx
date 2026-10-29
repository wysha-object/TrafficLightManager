import React, { DetailedHTMLProps, forwardRef, HTMLAttributes } from 'react'

export interface BasePageProps extends DetailedHTMLProps<
  HTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> {
  header?: React.ReactNode
  footer?: React.ReactNode
  children?: React.ReactNode
}

const BasePage = forwardRef<HTMLDivElement, BasePageProps>(
  function BasePage(props, ref) {
    return (
      <div
        ref={ref}
        {...props}
        style={{
          maxHeight: '50em',
          top: 'calc(10rem + var(--floatingToggleSize))',
          borderRadius: '1em',
          position: 'fixed',
          zIndex: '1000',
          overflow: 'hidden',
          color: 'var(--textColor)',
          display: 'flex',
          flexDirection: 'column',
          backgroundImage:
            'linear-gradient( var(--panelGradientStart) , var(--panelGradientEnd) )',
          ...props.style,
        }}
      >
        {props.header && (
          <div
            style={{
              fontSize: '1.1em',
              padding: '6rem 10rem',
              height: '2em',
              backgroundColor: 'var(--panelColorDark)',
              color: 'var(--accentColorNormal)',
            }}
          >
            {props.header}
          </div>
        )}
        {props.children && (
          <div style={{ flex: '1 1 0' }}>
            {props.children}
          </div>
        )}
        {props.footer && (
          <div
            style={{
              height: '2em',
              backgroundColor: 'var(--panelColorDark)',
            }}
          >
            {props.footer}
          </div>
        )}
      </div>
    )
  },
)
export default BasePage
