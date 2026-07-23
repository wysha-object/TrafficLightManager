import CheckSvg from 'assets/images/check.svg'
import EditSvg from 'assets/images/edit.svg'
import Input from './input'
import { ChangeEvent, KeyboardEvent, useEffect, useState } from 'react'
import { Button } from 'cs2/ui'

export default function InputField(props: {
  style?: React.CSSProperties
  onChange: (value: string) => void
  value: string
  displayWhenEmpty?: string
}) {
  const [textFieldActive, setTextFieldActive] = useState(false)
  const [textFieldValue, setTextFieldValue] = useState('')
  const enableTextField = () => {
    setTextFieldActive(true)
  }
  const submitTextField = () => {
    setTextFieldActive(false)
    props.onChange(textFieldValue)
  }
  const textFieldChangeHandler = (event: ChangeEvent<HTMLInputElement>) => {
    setTextFieldValue(event.target.value)
  }
  const textFieldKeyDownHandler = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key == 'Enter') {
      submitTextField()
    }
  }
  useEffect(() => {
    setTextFieldValue(props.value)
  }, [props.value])
  return (
    <div
      style={{
        display: 'flex',
        alignItems: 'center',
        width: '100%',
        ...props.style,
      }}
    >
      <div style={{ flex: 1 }}>
        {textFieldActive ? (
          <Input
            style={{ width: '100%' }}
            type='text'
            onChange={textFieldChangeHandler}
            onKeyDown={textFieldKeyDownHandler}
            value={textFieldValue}
            autoFocus
          />
        ) : (
          <>
            {textFieldValue || (
              <div style={{ opacity: 0.5 }}>{props.displayWhenEmpty}</div>
            )}
          </>
        )}
      </div>
      <div className='vertical-gap' />
      {textFieldActive ? (
        <Button variant='round' onClick={submitTextField}>
          <CheckSvg />
        </Button>
      ) : (
        <Button variant='round' onClick={enableTextField}>
          <EditSvg />
        </Button>
      )}
    </div>
  )
}
