import Checkbox from 'components/base/checkbox'
import TipArea from 'components/base/tip-area'
import TipIcon from 'components/icon/tip-icon'
import { ReactNode } from 'react'

export default function CheckboxRow(props: {
  isChecked: boolean
  label: string
  tooltip: ReactNode
  onClick: () => void
}) {
  return (
    <TipArea
      className='row-with-hover-effect'
      onClick={props.onClick}
      position='right-start'
      tooltip={props.tooltip}
    >
      <div style={{ flex: '1', display: 'flex', alignItems: 'center' }}>
        <Checkbox isChecked={props.isChecked} />
        <div>{props.label}</div>
      </div>
      <div className='vertical-gap' />
      <TipIcon />
    </TipArea>
  )
}
