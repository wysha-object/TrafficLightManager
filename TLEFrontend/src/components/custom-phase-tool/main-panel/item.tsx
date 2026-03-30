import { useContext, useEffect, useState } from "react";
import styled from "styled-components";

import { call } from "cs2/api";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

import Check from "@/components/common/icons/check";
import ChevronDown from "@/components/common/icons/chevron-down";
import ChevronUp from "@/components/common/icons/chevron-up";
import Delete from "@/components/common/icons/delete";
import Tune from "@/components/common/icons/tune";
import Visibility from "@/components/common/icons/visibility";
import VisibilityOff from "@/components/common/icons/visibility-off";
import Row from "@/components/main-panel/items/row";

import ItemDivider from "./item-divider";

const Label = styled.div<{dim?: boolean}>`
  color: ${props => props.dim ? "var(--textColorDim)" : "var(--textColor)"};
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  display: inline;
  filter: ${props => props.dim ? "brightness(0.8)" : "none"};
`;

const IconBarContainer = styled.div`
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  flex-direction: row;
  display: flex;
  justify-content: flex-end;
`;

const IconContainer = styled.div<{disabled?: boolean}>`
  display: flex;
  margin-left: 0.35em;
  border-radius: 0.2em;
  &:hover {
    filter: ${props => props.disabled ? "none" : "brightness(1.2) contrast(1.2)"};
    background: ${props => props.disabled ? "transparent" : "rgba(0, 0, 0, 0.1)"};
  }
`;

const IconStyle = {
  color: "var(--textColorDim)",
  width: "1.1em",
  height: "1.1em",
  fontSize: "1.1em"
};

const IconStyleDisabled = {
  opacity: 0.3
};

const ActiveDot = () => <div style={{color: "#34bf42", marginLeft: "6rem"}}>•</div>;

export default function Item(props: {data: MainPanelItemCustomPhase}) {
  const locale = useContext(LocaleContext);
  const [isActiveLabel, setIsActiveLabel] = useState(false);
  const [showEditor, setShowEditor] = useState(false);
  const swap = (index1: number, index2: number) => {
    if (index1 < 0 || index2 < 0 || index1 >= props.data.length || index2 >= props.data.length) {
      return;
    }
    call("C2VM.TLE", "CallSwapCustomPhase", JSON.stringify({index1, index2}));
  };
  useEffect(() => {
    if (props.data.activeViewingIndex >= 0) {
      setIsActiveLabel(props.data.activeViewingIndex == props.data.index);
    } else if (props.data.activeIndex >= 0) {
      setIsActiveLabel(props.data.activeIndex == props.data.index);
    } else {
      setIsActiveLabel(props.data.index + 1 == props.data.currentSignalGroup);
    }
    setShowEditor(props.data.activeIndex == props.data.index);
  }, [props.data.activeViewingIndex, props.data.activeIndex, props.data.index, props.data.currentSignalGroup]);
  return (
    <>
      <Row style={{padding: "0.25em"}}>
        <Label dim={!isActiveLabel}>
          {getString(locale, "Phase") + " #" + (props.data.index + 1)}{props.data.activeIndex < 0 && props.data.index + 1 == props.data.currentSignalGroup && <ActiveDot />}
        </Label>
        <IconBarContainer>
          {!showEditor && <>
            {props.data.activeViewingIndex == props.data.index && <IconContainer>
              <VisibilityOff style={IconStyle} onClick={() => call("C2VM.TLE", "CallSetActiveCustomPhaseIndex", JSON.stringify({key: "ActiveViewingCustomPhaseIndex", value: -1}))} />
            </IconContainer>}
            {props.data.activeViewingIndex != props.data.index && <IconContainer>
              <Visibility style={IconStyle} onClick={() => call("C2VM.TLE", "CallSetActiveCustomPhaseIndex", JSON.stringify({key: "ActiveViewingCustomPhaseIndex", value: props.data.index}))} />
            </IconContainer>}
            <IconContainer>
              <Tune style={IconStyle} onClick={() => call("C2VM.TLE", "CallSetActiveCustomPhaseIndex", JSON.stringify({key: "ActiveEditingCustomPhaseIndex", value: props.data.index}))} />
            </IconContainer>
          </>}
          {showEditor && <>
            <IconContainer>
              <Delete style={IconStyle} onClick={() => call("C2VM.TLE", "CallRemoveCustomPhase", JSON.stringify({index: props.data.index}))} />
            </IconContainer>
            <IconContainer>
              <Check style={IconStyle} onClick={() => call("C2VM.TLE", "CallSetActiveCustomPhaseIndex", JSON.stringify({key: "ActiveEditingCustomPhaseIndex", value: -1}))} />
            </IconContainer>
            <IconContainer disabled={props.data.activeIndex <= 0}>
              <ChevronUp style={{...IconStyle, ...(props.data.activeIndex <= 0 && IconStyleDisabled)}} onClick={() => swap(props.data.activeIndex, props.data.activeIndex - 1)} />
            </IconContainer>
            <IconContainer disabled={props.data.activeIndex >= (props.data.length - 1)}>
              <ChevronDown style={{...IconStyle, ...(props.data.activeIndex >= (props.data.length - 1) && IconStyleDisabled)}} onClick={() => swap(props.data.activeIndex, props.data.activeIndex + 1)} />
            </IconContainer>
          </>}
        </IconBarContainer>
      </Row>
      {props.data.index + 1 < props.data.length && <ItemDivider index={props.data.index} linked={props.data.linkedWithNextPhase} />}
    </>
  );
}