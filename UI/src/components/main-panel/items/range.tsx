import { ChangeEvent, KeyboardEvent, useEffect, useMemo, useState } from 'react';
import styled from 'styled-components';

import Input from '@/components/common/input';
import Range from '@/components/common/range';

import Check from '@/components/common/icons/check';
import Edit from '@/components/common/icons/edit';
import ResetSettings from '@/components/common/icons/reset-settings';

import TitleDim from './title-dim';
import { useTranslate } from '@/localisations';
import Tooltip from '@/components/common/tooltip';
import TooltipIcon from '@/components/common/tooltip-icon';

const Container = styled.div`
  padding: 4rem 8rem;
`;

const Gap = styled.div`
  height: 6rem;
`;

const IconContainer = styled.div<{ disabled?: boolean }>`
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

export default function MainPanelRange(
  props: {
    onChange: (value: number) => void,
    label: string,
    value: number,
    valuePrefix: string,
    valueSuffix: string,
    defaultValue: number,
    enableTextField?: boolean,
    textFieldRegExp?: string,
    min: number,
    max: number,
    step: number,
    tooltip?: React.ReactNode
  }
) {
  const { t } = useTranslate();
  const [value, setValue] = useState(0);
  const [textFieldActive, setTextFieldActive] = useState(false);
  const [textFieldValue, setTextFieldValue] = useState("");
  const textFieldRegExp = useMemo(() => {
    return props.textFieldRegExp ? new RegExp(props.textFieldRegExp) : null;
  }, [props.textFieldRegExp]);
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
        props.onChange(newValue);
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
    props.onChange(props.defaultValue);
  };
  useEffect(() => {
    setValue(props.value);
  }, [props.value]);
  return (
    <Container>
      <TitleContainer>
        <TitleDim title={props.label} secondaryText={!textFieldActive ? t(props.valuePrefix) + `${Math.round(value * 100) / 100}` + t(props.valueSuffix) : ""} />
        {textFieldActive && <Input type="number" style={{ minWidth: "3em", width: "3em" }} onChange={textFieldChangeHandler} onKeyDown={textFieldKeyDownHandler} value={textFieldValue} autoFocus />}
        {props.enableTextField && <>
          {textFieldActive && <IconContainer><Check style={IconStyle} onClick={submitTextField} /></IconContainer>}
          {!textFieldActive && <IconContainer><Edit style={IconStyle} onClick={enableTextField} /></IconContainer>}
        </>}
        <IconContainer><ResetSettings style={IconStyle} onClick={resetHandler} /></IconContainer>
        {props.tooltip && <>
          <Tooltip position="right-start" tooltip={props.tooltip}>
            <TooltipIcon style={{ marginLeft: "0.25em" }} />
          </Tooltip>
        </>}
      </TitleContainer>
      <Gap />
      <Range min={props.min} max={props.max} step={props.step} value={props.value} onChange={props.onChange} onUpdate={updateHandler} />
    </Container>
  );
}