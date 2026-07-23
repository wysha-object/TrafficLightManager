import React, { DetailedHTMLProps, forwardRef, HTMLAttributes } from 'react'

export interface BasePageProps extends DetailedHTMLProps<
  HTMLAttributes<HTMLDivElement>,
  HTMLDivElement
> {
  header?: React.ReactNode
  children?: React.ReactNode
}

const BasePage = forwardRef<HTMLDivElement, BasePageProps>(
  function BasePage(props, ref) {
    return (
      <div
        ref={ref}
        {...props}
        style={{
          maxHeight: '56em',
          top: 'calc(10rem + var(--floatingToggleSize))',
          borderRadius: '4rem',
          position: 'fixed',
          zIndex: '1000',
          ...props.style,
        }}
      >
        <div
          style={{
            fontSize: '1.1em',
            padding: '6rem 10rem',
            height: '2em',
            backgroundColor: 'var(--panelColorDark)',
            backdropFilter: 'var(--panelBlur)',
            color: 'var(--accentColorNormal)',
          }}
        >
          {props.header}
        </div>
        {props.children}
      </div>
    )
  },
)
export default BasePage
