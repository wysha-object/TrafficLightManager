import CheckSvg from 'assets/images/check.svg'
import EditSvg from 'assets/images/edit.svg'
import ResetSettingsSvg from 'assets/images/reset-settings.svg'
import { ChangeEvent, KeyboardEvent, useEffect, useMemo, useState } from 'react'
import Input from 'components/base/input'
import Range from 'components/base/range'
import { useTranslate } from 'hooks/translate'
import TipArea from 'components/base/tip-area'
import { Button } from 'cs2/ui'
import TipIcon from 'components/icon/tip-icon'

export default function RangeRow(props: {
  onChange: (value: number) => void
  label: string
  value: number
  valuePrefix: string
  valueSuffix: string
  defaultValue: number
  enableTextField?: boolean
  textFieldRegExp?: string
  min: number
  max: number
  step: number
  tooltip?: React.ReactNode
}) {
  const { t } = useTranslate()
  const [value, setValue] = useState(0)
  const [textFieldActive, setTextFieldActive] = useState(false)
  const [textFieldValue, setTextFieldValue] = useState('')
  const textFieldRegExp = useMemo(() => {
    return props.textFieldRegExp ? new RegExp(props.textFieldRegExp) : null
  }, [props.textFieldRegExp])
  const updateHandler = (value: number) => {
    setValue(value)
  }
  const enableTextField = () => {
    setTextFieldValue('')
    setTextFieldActive(true)
  }
  const submitTextField = () => {
    setTextFieldActive(false)
    if (textFieldValue.length > 0) {
      const newValue = parseFloat(textFieldValue)
      if (!isNaN(newValue)) {
        props.onChange(newValue)
      }
    }
  }
  const textFieldChangeHandler = (event: ChangeEvent<HTMLInputElement>) => {
    if (textFieldRegExp !== null) {
      if (event.target.value.match(textFieldRegExp)) {
        setTextFieldValue(event.target.value)
      }
    } else {
      setTextFieldValue(event.target.value)
    }
  }
  const textFieldKeyDownHandler = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key == 'Enter') {
      submitTextField()
    }
  }
  const resetHandler = () => {
    setTextFieldActive(false)
    props.onChange(props.defaultValue)
  }
  useEffect(() => {
    setValue(props.value)
  }, [props.value])
  return (
    <div className='row-with-hover-effect' style={{ flexDirection: 'column' }}>
      <div
        style={{
          width: '100%',
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
        }}
      >
        <div style={{ flex: '1', display: 'flex' }}>
          <div style={{ flex: '1' }}>{props.label}</div>
          {!textFieldActive
            ? t(props.valuePrefix) +
              `${Math.round(value * 100) / 100}` +
              t(props.valueSuffix)
            : ''}
        </div>
        {textFieldActive && (
          <Input
            type='number'
            style={{ minWidth: '3em', width: '3em' }}
            onChange={textFieldChangeHandler}
            onKeyDown={textFieldKeyDownHandler}
            value={textFieldValue}
            autoFocus
          />
        )}
        <div className='vertical-gap' />
        {props.enableTextField && (
          <>
            {textFieldActive ? (
              <Button variant='round' onClick={submitTextField}>
                <CheckSvg />
              </Button>
            ) : (
              <Button variant='round' onClick={enableTextField}>
                <EditSvg />
              </Button>
            )}
          </>
        )}
        <div className='vertical-gap' />
        <Button variant='round' onClick={resetHandler}>
          <ResetSettingsSvg />
        </Button>
        {props.tooltip && (
          <>
            <div className='vertical-gap' />
            <TipArea position='right-start' tooltip={props.tooltip}>
              <TipIcon />
            </TipArea>
          </>
        )}
      </div>
      <div className='horizontal-gap' />
      <Range
        min={props.min}
        max={props.max}
        step={props.step}
        value={props.value}
        onChange={props.onChange}
        onUpdate={updateHandler}
      />
    </div>
  )
}
