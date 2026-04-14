import { CSSProperties, useContext } from "react";
import styled from "styled-components";

import { bindValue, trigger, useValue } from "cs2/api";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

import Button from "@/components/common/button";
import Radio from "@/components/common/radio";
import Scrollable from "@/components/common/scrollable";
import Divider from "@/components/main-panel/items/divider";

const Label = styled.div<{ dim?: boolean }>`
  color: ${props => props.dim ? "var(--textColorDim)" : "var(--textColor)"};
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  display: inline;
`;

const Row = styled.div<{ hoverEffect?: boolean }>`
  padding: 0.25em 0.5em;
  width: 100%;
  display: flex;
  align-items: center;
  &:hover {
    filter: ${props => props.hoverEffect ? "brightness(1.2) contrast(1.2)" : "none"};
  }
`;

const ItemContainerStyle: CSSProperties = {
  display: "flex",
  flexDirection: "column",
  flex: 1,
};

const BackButton = (props: { onClick: () => void }) => {
  return (
    <Row hoverEffect={true} onClick={props.onClick}>
      <Button label="CustomPhaseEditor.Back" />
    </Row>
  );
};

function Item(props: { data: CustomPhaseItem, itemIndex: number, manualPhaseIndex: number }) {
  const locale = useContext(LocaleContext);
  const clickHandler = () => {
    trigger("TrafficLightManager", "SetManualPhaseIndex", props.itemIndex);
  };
  return (
    <Row onClick={clickHandler}>
      <Radio isChecked={props.manualPhaseIndex == props.itemIndex} />
      <Label dim={true}>
        {getString(locale, "Phase") + " #" + (props.itemIndex + 1)}
      </Label>
    </Row>
  );
}

export default function ManualControlPanel(props: { items: CustomPhaseItem[], onBack: () => void }) {
  const manualPhaseIndex: number = useValue(bindValue("TrafficLightManager", "GetManualPhaseIndex"));
  console.log("ManualControlPanel render", { manualPhaseIndex });

  const locale = useContext(LocaleContext);
  return (
    <>
      <Scrollable style={{ flex: 1 }} contentStyle={ItemContainerStyle}>
        <Row>
          <Label dim={false}>{getString(locale, "CustomPhaseEditor.ManualControl")}</Label>
        </Row>
        {props.items.map((item, index) => <Item data={item} itemIndex={index} manualPhaseIndex={manualPhaseIndex} />)}
      </Scrollable>
      <Divider />
      <BackButton onClick={props.onBack} />
    </>
  );
}