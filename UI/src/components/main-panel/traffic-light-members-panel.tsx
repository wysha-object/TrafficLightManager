import TextInput from 'components/base/text-input'
import { Button, Scrollable } from 'cs2/ui'
import {
  useGetTrafficLightsMembersCmd,
  useGetTrafficLightGroupNameCmd,
  setToolStateCmd,
  lookAt,
  setTrafficLightGroupNameCmd,
} from 'hooks/cmds'
import { useTranslate } from 'hooks/translate'
import styled from 'styled-components'
import { ToolState } from 'types'

const PanelContainer = styled.div`
  width: 11em;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 0.25em;
`

export default function TrafficLightMembersPanel() {
  const { t } = useTranslate()

  const trafficLightGroupName = useGetTrafficLightGroupNameCmd()
  const trafficLightsMembers = useGetTrafficLightsMembersCmd()

  return (
    <PanelContainer>
      <TextInput
        style={{ margin: '2rem 2rem' }}
        onChange={(value) => setTrafficLightGroupNameCmd(value)}
        value={trafficLightGroupName}
        displayWhenEmpty={'Traffic Light Group'}
      />
      <div className='row'>{t('CustomPhaseEditor.TrafficLightsMembers')}</div>
      <Scrollable style={{ flex: 1 }}>
        {trafficLightsMembers.map((item, _) => (
          <div
            className='row-with-hover-effect'
            key={item.entityIndex}
            onClick={() =>
              item.position &&
              lookAt(item.position.x, item.position.y, item.position.z, 200)
            }
          >
            #{item.entityIndex}
          </div>
        ))}
      </Scrollable>
      <div className='horizontal-divider-with-gap' />
      <div className='row'>
        <Button
          style={{ width: '100%' }}
          variant='flat'
          onClick={() => setToolStateCmd(ToolState.AddTrafficLights)}
        >
          {t('CustomPhaseEditor.AddMember')}
        </Button>
      </div>
      <div className='row'>
        <Button
          style={{ width: '100%' }}
          variant='flat'
          onClick={() => setToolStateCmd(ToolState.RemoveTrafficLights)}
        >
          {t('CustomPhaseEditor.RemoveMember')}
        </Button>
      </div>
    </PanelContainer>
  )
}
