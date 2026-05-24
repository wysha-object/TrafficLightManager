import styled from "styled-components";
import Check from "./icons/check";
import Edit from "./icons/edit";
import Input from "./input";
import { ChangeEvent, KeyboardEvent, useEffect, useState } from 'react';

const IconStyle = {
    color: "var(--textColorDim)",
    width: "1em",
    height: "1em",
    fontSize: "1em"
};

const IconContainer = styled.div<{ disabled?: boolean }>`
  margin: 0.25em;
  border-radius: 0.2em;
  &:hover {
    filter: ${props => props.disabled ? "none" : "brightness(1.2) contrast(1.2)"};
    background: ${props => props.disabled ? "transparent" : "rgba(0, 0, 0, 0.1)"};
  }
`;

export default function TextField(
    props: {
        onChange: (value: string) => void,
        value: string,
        displayWhenEmpty?: string
    }
) {
    const [textFieldActive, setTextFieldActive] = useState(false);
    const [textFieldValue, setTextFieldValue] = useState("");
    const enableTextField = () => {
        setTextFieldActive(true);
    };
    const submitTextField = () => {
        setTextFieldActive(false);
        props.onChange(textFieldValue);
    };
    const textFieldChangeHandler = (event: ChangeEvent<HTMLInputElement>) => {
        setTextFieldValue(event.target.value);
    };
    const textFieldKeyDownHandler = (event: KeyboardEvent<HTMLInputElement>) => {
        if (event.key == "Enter") {
            submitTextField();
        }
    };
    useEffect(() => {
        setTextFieldValue(props.value);
    }, [props.value]);
    return (
        <div style={{ display: "flex", margin: "0.1em", alignItems: "center", width: "100%" }}>
            <div style={{ flex: 1 }}>
                {
                    textFieldActive ?
                    <Input
                        style={{width: "100%"}}
                        type="text"
                        onChange={textFieldChangeHandler}
                        onKeyDown={textFieldKeyDownHandler}
                        value={textFieldValue}
                        autoFocus
                    /> :
                    <>
                        {textFieldValue || <div style={{opacity: 0.5}}>{props.displayWhenEmpty}</div>}
                    </>
                }

            </div>
            {
                textFieldActive ?
                <IconContainer><Check style={IconStyle} onClick={submitTextField} /></IconContainer> :
                <IconContainer><Edit style={IconStyle} onClick={enableTextField} /></IconContainer>
            }
        </div>
    )
}