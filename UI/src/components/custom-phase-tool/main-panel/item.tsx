import { useEffect, useState } from "react";
import styled from "styled-components";

import { call } from "cs2/api";

import Check from "@/components/common/icons/check";
import ChevronDown from "@/components/common/icons/chevron-down";
import ChevronUp from "@/components/common/icons/chevron-up";
import Delete from "@/components/common/icons/delete";
import Tune from "@/components/common/icons/tune";
import Visibility from "@/components/common/icons/visibility";
import VisibilityOff from "@/components/common/icons/visibility-off";
import Row from "@/components/main-panel/items/row";

import ItemDivider from "./item-divider";
import { useTranslate } from "@/localisations";

const Label = styled.div<{ dim?: boolean }>`
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

const IconContainer = styled.div<{ disabled?: boolean }>`
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

const ActiveDot = () => <div style={{ color: "#34bf42", marginLeft: "6rem" }}>•</div>;

export enum ItemState {
  None,
  Viewing,
  Editing,
}

export default function Item(
  props: {
    data: CustomPhaseItem,
    itemIndex: number,
    itemState: ItemState,
    currentIndex: number,
    itemCount: number,
    trafficLightGroup: TrafficLightGroup,
    updateItemState: (state: ItemState) => void,
    updateCurrentIndex: (index: number) => void,
  }
) {
  const { t } = useTranslate();
  const [isActiveLabel, setIsActiveLabel] = useState(false);
  const swapItem = (index1: number, index2: number) => {
    call("TrafficLightManager", "CallSwapCustomPhase", JSON.stringify({ index1, index2 }))

    if (index1 === props.currentIndex) {
      props.updateCurrentIndex(index2);
    } else if (index2 === props.currentIndex) {
      props.updateCurrentIndex(index1);
    }
  };
  useEffect(() => {
    if (props.itemState == ItemState.Viewing) {
      setIsActiveLabel(true);
    } else if (props.itemState == ItemState.Editing) {
      setIsActiveLabel(true);
    } else {
      setIsActiveLabel(false);
    }
  }, [props.itemState]);
  return (
    <>
      <Row style={{ padding: "0.25em" }}>
        <Label dim={!isActiveLabel}>
          {t("Phase") + " #" + (props.itemIndex + 1)}{props.trafficLightGroup.currentPhaseIndex === props.itemIndex && <ActiveDot />}
        </Label>
        <IconBarContainer>
          {props.itemState != ItemState.Editing && <>
            {props.itemState == ItemState.Viewing && <IconContainer>
              <VisibilityOff style={IconStyle} onClick={() => props.updateItemState(ItemState.None)} />
            </IconContainer>}
            {props.itemState == ItemState.None && <IconContainer>
              <Visibility style={IconStyle} onClick={() => props.updateItemState(ItemState.Viewing)} />
            </IconContainer>}
            <IconContainer>
              <Tune style={IconStyle} onClick={() => props.updateItemState(ItemState.Editing)} />
            </IconContainer>
          </>}
          {props.itemState == ItemState.Editing && <>
            <IconContainer>
              <Delete style={IconStyle} onClick={() => {
                call("TrafficLightManager", "CallRemoveCustomPhase", JSON.stringify({ index: props.itemIndex }))
                if (props.currentIndex === props.itemIndex) {
                  props.updateCurrentIndex(props.currentIndex - 1);
                  props.updateItemState(ItemState.None);
                }
              }} />
            </IconContainer>
            <IconContainer>
              <Check style={IconStyle} onClick={() => props.updateItemState(ItemState.None)} />
            </IconContainer>
            <IconContainer>
              {(props.itemIndex - 1) >= 0 && <ChevronUp style={{ ...IconStyle, ...IconStyleDisabled }} onClick={() => swapItem(props.itemIndex, props.itemIndex - 1)} />}
            </IconContainer>
            <IconContainer>
              {(props.itemIndex + 1) < props.itemCount && <ChevronDown style={{ ...IconStyle, ...IconStyleDisabled }} onClick={() => swapItem(props.itemIndex, props.itemIndex + 1)} />}
            </IconContainer>
          </>}
        </IconBarContainer>
      </Row>
      {<ItemDivider index={props.itemIndex} linked={props.data.linkedWithNextPhase} />}
    </>
  );
}