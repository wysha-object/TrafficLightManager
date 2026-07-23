declare module '*.scss'
declare module '*.css'
declare module '*.svg' {
  import React from 'react'
  const ReactComponent: React.FC<React.SVGProps<SVGSVGElement>>
  export default ReactComponent
} // we recommed using SVGs for all the icons and UI elements
declare module '*.png'
declare module '*.jpg'
declare module '*.gif'
