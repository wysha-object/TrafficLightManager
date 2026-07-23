import DeleteSvg from 'assets/images/delete.svg'
import Input from 'components/base/input'
import { useTranslate } from 'hooks/translate'
import { getModule } from 'cs2/modding'
import { Button, Dropdown, DropdownItem, DropdownToggle } from 'cs2/ui'
import { useState } from 'react'
import { CustomPhaseItem } from 'types'
import {
  setCustomPhaseDataCmd,
  removeTemplateCmd,
  setTemplateCmd,
  useGetSettingsCmd,
  useGetSystemDefaultTemplateCmd,
} from 'hooks/cmds'

export default function TemplateSection(props: {
  item: CustomPhaseItem
  itemIndex: number
}) {
  let [selectTemplate, setSelectTemplate] = useState('')

  let [inputName, setInputName] = useState('')

  const settings = useGetSettingsCmd()
  const systemDefaultTemplate = useGetSystemDefaultTemplateCmd()
  settings.customPhaseTemplates.findIndex(
    (template) => template.m_Name === selectTemplate,
  ) < 0 && setSelectTemplate(settings.defaultCustomPhaseTemplate.m_Name)

  const { t } = useTranslate()
  return (
    <>
      {props.item.bindWithTemplate && (
        <>
          <div className='row'>
            <span style={{ flex: '1' }}>
              {t('CustomPhaseEditor.Template.BoundWith')}
            </span>
            <span style={{ color: '#77FF00' }}>{props.item.bindTemplate}</span>
          </div>
          {settings.customPhaseTemplates.findIndex(
            (template) => template.m_Name === props.item.bindTemplate,
          ) < 0 && (
            <>
              <div className='row'>
                {t('CustomPhaseEditor.Template.InvalidBind')}
              </div>
              <div className='row'>
                <Button
                  style={{ width: '100%' }}
                  variant='flat'
                  onClick={() => {
                    setTemplateCmd({
                      m_Name: props.item.bindTemplate,
                      m_IsPrioritiseTrack: props.item.prioritiseTrack,
                      m_IsPrioritisePublicCar: props.item.prioritisePublicCar,
                      m_IsPrioritisePedestrian: props.item.prioritisePedestrian,
                      m_MinimumDuration: props.item.minimumDuration,
                      m_MaximumDuration: props.item.maximumDuration,
                      m_TargetDurationMultiplier:
                        props.item.targetDurationMultiplier,
                      m_LaneOccupiedMultiplier:
                        props.item.laneOccupiedMultiplier,
                      m_IntervalExponent: props.item.intervalExponent,
                    })
                  }}
                >
                  {t('CustomPhaseEditor.Template.SaveAndBind')}
                </Button>
              </div>
            </>
          )}
          {props.item.bindWithTemplate && (
            <div className='row'>
              <Button
                style={{ width: '100%' }}
                variant='flat'
                onClick={() => {
                  setCustomPhaseDataCmd({
                    index: props.itemIndex,
                    key: 'BindWithTemplate',
                    value: { bindWithTemplate: false },
                  })
                }}
              >
                {t('CustomPhaseEditor.Template.Unbind')}
              </Button>
            </div>
          )}
        </>
      )}
      {!props.item.bindWithTemplate && (
        <>
          <Dropdown
            theme={getModule(
              'game-ui/menu/themes/dropdown.module.scss',
              'classes',
            )}
            content={settings.customPhaseTemplates.map((template) => (
              <DropdownItem
                key={template.m_Name}
                value={template.m_Name}
                onChange={(value) => {
                  setSelectTemplate(value)
                }}
              >
                <span style={{ flex: 1 }}>{template.m_Name}</span>
                {template.m_Name !== systemDefaultTemplate.m_Name && (
                  <Button
                    variant='round'
                    onClick={() => removeTemplateCmd(template.m_Name)}
                  >
                    <DeleteSvg />
                  </Button>
                )}
              </DropdownItem>
            ))}
          >
            <div className='row'>
              <DropdownToggle style={{ width: '100%' }}>
                {selectTemplate}
              </DropdownToggle>
            </div>
          </Dropdown>
          <div className='row'>
            <Button
              style={{ width: '100%' }}
              variant='flat'
              onClick={() =>
                setCustomPhaseDataCmd({
                  key: 'ApplyTemplate',
                  index: props.itemIndex,
                  value: selectTemplate,
                })
              }
            >
              {t('CustomPhaseEditor.Template.Apply')}
            </Button>
          </div>
          <div className='row'>
            <Button
              style={{ width: '100%' }}
              variant='flat'
              onClick={() => {
                setCustomPhaseDataCmd({
                  key: 'ApplyTemplate',
                  index: props.itemIndex,
                  value: selectTemplate,
                })
                setCustomPhaseDataCmd({
                  key: 'BindWithTemplate',
                  index: props.itemIndex,
                  value: {
                    bindWithTemplate: true,
                    templateName: selectTemplate,
                  },
                })
              }}
            >
              {t('CustomPhaseEditor.Template.ApplyAndBind')}
            </Button>
          </div>
          <div className='horizontal-gap' />
          <div className='row'>
            <Input
              style={{ width: '100%' }}
              onChange={(e) => setInputName(e.target.value)}
              value={inputName}
            ></Input>
          </div>
          <div className='row'>
            <div style={{ flex: '1 1 0' }}>
              <Button
                variant='flat'
                disabled={
                  inputName === systemDefaultTemplate.m_Name || inputName === ''
                }
                onClick={() => {
                  setTemplateCmd({
                    m_Name: inputName,
                    m_IsPrioritiseTrack: props.item.prioritiseTrack,
                    m_IsPrioritisePublicCar: props.item.prioritisePublicCar,
                    m_IsPrioritisePedestrian: props.item.prioritisePedestrian,
                    m_MinimumDuration: props.item.minimumDuration,
                    m_MaximumDuration: props.item.maximumDuration,
                    m_TargetDurationMultiplier:
                      props.item.targetDurationMultiplier,
                    m_LaneOccupiedMultiplier: props.item.laneOccupiedMultiplier,
                    m_IntervalExponent: props.item.intervalExponent,
                  })
                }}
              >
                {t('CustomPhaseEditor.Template.Save')}
              </Button>
            </div>
            <div className='vertical-gap' />
            <div style={{ flex: '1 1 0' }}>
              <Button
                variant='flat'
                disabled={
                  inputName === systemDefaultTemplate.m_Name || inputName === ''
                }
                onClick={() => {
                  setTemplateCmd({
                    m_Name: inputName,
                    m_IsPrioritiseTrack: props.item.prioritiseTrack,
                    m_IsPrioritisePublicCar: props.item.prioritisePublicCar,
                    m_IsPrioritisePedestrian: props.item.prioritisePedestrian,
                    m_MinimumDuration: props.item.minimumDuration,
                    m_MaximumDuration: props.item.maximumDuration,
                    m_TargetDurationMultiplier:
                      props.item.targetDurationMultiplier,
                    m_LaneOccupiedMultiplier: props.item.laneOccupiedMultiplier,
                    m_IntervalExponent: props.item.intervalExponent,
                  })
                  setCustomPhaseDataCmd({
                    key: 'BindWithTemplate',
                    index: props.itemIndex,
                    value: {
                      bindWithTemplate: true,
                      templateName: inputName,
                    },
                  })
                }}
              >
                {t('CustomPhaseEditor.Template.SaveAndBind')}
              </Button>
            </div>
          </div>
        </>
      )}
    </>
  )
}
