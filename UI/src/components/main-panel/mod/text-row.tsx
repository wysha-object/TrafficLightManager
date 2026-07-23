import TipArea from 'components/base/tip-area'
import TipIcon from 'components/icon/tip-icon'

const containerProps = {
  className: 'row-with-hover-effect',
}

export default function TextRow(props: {
  title: string
  secondaryText?: string
  tooltip?: React.ReactNode
  dim?: boolean
}) {
  const item = (
    <div style={{ flex: '1', display: 'flex', alignItems: 'center' }}>
      <div style={{ flex: '1', display: 'flex' }}>
        <div style={{ flex: '1' }}>{props.title}</div>
        {props.secondaryText}
      </div>
    </div>
  )
  return props.tooltip ? (
    <TipArea {...containerProps} position='right' tooltip={props.tooltip}>
      {item}
      <div className='vertical-gap' />
      <TipIcon />
    </TipArea>
  ) : (
    <div {...containerProps}>{item}</div>
  )
}
