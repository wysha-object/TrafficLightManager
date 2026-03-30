import { ChangeEvent, KeyboardEvent, useContext, useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';

import { LocaleContext } from '@/context';
import { engineCall } from '@/engine';
import { getString } from '@/localisations';

import Input from '@/components/common/input';
import Range from '@/components/common/range';

import Check from '@/components/common/icons/check';
import Edit from '@/components/common/icons/edit';
import ResetSettings from '@/components/common/icons/reset-settings';

import TitleDim from './title-dim';

const Container = styled.div`
  padding: 4rem 8rem;
`;

const Gap = styled.div`
  height: 6rem;
`;

const IconContainer = styled.div<{disabled?: boolean}>`
  margin-left: 0.25em;
  border-radius: 0.2em;
  &:hover {
    filter: ${props => props.disabled ? "none" : "brightness(1.2) contrast(1.2)"};
    background: ${props => props.disabled ? "transparent" : "rgba(0, 0, 0, 0.1)"};
  }
`;

const TitleContainer = styled.div`
  display: flex;
  flex-wrap: wrap;
  align-items: center;
`;

const IconStyle = {
  color: "var(--textColorDim)",
  width: "1em",
  height: "1em",
  fontSize: "1em"
};

export default function MainPanelRange(props: {data: MainPanelItemRange}) {
  const locale = useContext(LocaleContext);
  const [value, setValue] = useState(0);
  const [textFieldActive, setTextFieldActive] = useState(false);
  const [textFieldValue, setTextFieldValue] = useState("");
  const textFieldRegExp = useMemo(() => {
    return props.data.textFieldRegExp ? new RegExp(props.data.textFieldRegExp) : null;
  }, [props.data.textFieldRegExp]);
  const changeHandler = (value: number) => {
    if ("engineEventName" in props.data) {
      engineCall(props.data.engineEventName, JSON.stringify({key: props.data.key, value}));
    }
  };
  const updateHandler = (value: number) => {
    setValue(value);
  };
  const enableTextField = () => {
    setTextFieldValue("");
    setTextFieldActive(true);
  };
  const submitTextField = () => {
    setTextFieldActive(false);
    if (textFieldValue.length > 0) {
      const newValue = parseFloat(textFieldValue);
      if (!isNaN(newValue)) {
        changeHandler(newValue);
      }
    }
  };
  const textFieldChangeHandler = (event: ChangeEvent<HTMLInputElement>) => {
    if (textFieldRegExp !== null) {
      if (event.target.value.match(textFieldRegExp)) {
        setTextFieldValue(event.target.value);
      }
    } else {
      setTextFieldValue(event.target.value);
    }
  };
  const textFieldKeyDownHandler = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key == "Enter") {
      submitTextField();
    }
  };
  const resetHandler = () => {
    setTextFieldActive(false);
    changeHandler(props.data.defaultValue);
  };
  useEffect(() => {
    setValue(props.data.value);
  }, [props.data.value]);
  return (
    <Container>
      <TitleContainer>
        <TitleDim itemType="title" title={props.data.label} secondaryText={!textFieldActive ? getString(locale, props.data.valuePrefix) + `${Math.round(value * 100) / 100}` + getString(locale, props.data.valueSuffix) : ""} />
        {textFieldActive && <Input type="number" style={{minWidth: "3em", width: "3em"}} onChange={textFieldChangeHandler} onKeyDown={textFieldKeyDownHandler} value={textFieldValue} autoFocus />}
        {props.data.enableTextField && <>
          {textFieldActive && <IconContainer><Check style={IconStyle} onClick={submitTextField} /></IconContainer>}
          {!textFieldActive && <IconContainer><Edit style={IconStyle} onClick={enableTextField} /></IconContainer>}
        </>}
        <IconContainer><ResetSettings style={IconStyle} onClick={resetHandler} /></IconContainer>
      </TitleContainer>
      <Gap />
      <Range data={props.data} onChange={changeHandler} onUpdate={updateHandler} />
    </Container>
  );
}