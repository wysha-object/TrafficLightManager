import { useTranslate } from 'hooks/translate'
import { CustomPhaseSignalState } from 'types'

export default function TrafficSignTooltip(props: {
  state: CustomPhaseSignalState
}) {
  const { t } = useTranslate()
  let text = ''
  if (props.state == 'go') {
    text = t('TrafficSignGo')
  } else if (props.state == 'yield') {
    text = t('TrafficSignYield')
  } else if (props.state == 'stop') {
    text = t('TrafficSignStop')
  }
  return <>{text}</>
}
