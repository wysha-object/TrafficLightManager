import { CSSProperties } from "react";
import styled from "styled-components";

import { call } from "cs2/api";

import { MainPanelState } from "@/constants";

import Button from "@/components/common/button";
import Scrollable from "@/components/common/scrollable";
import Divider from "@/components/main-panel/items/divider";
import Row from "@/components/main-panel/items/row";

import Item from "./item";
import ManualControlPanel from "./manual-control-panel";
import SubPanel from "./sub-panel";

const Container = styled.div`
  width: 30em;
  display: flex;
  flex-direction: row;
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
`;

const LeftPanelContainer = styled.div`
  width: 13.5em;
  max-width: 13.5em;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 0.25em;
`;

const RightPanelContainer = styled.div`
  width: 16.5em;
  max-width: 16.5em;
  background-color: var(--sectionBackgroundColor);
  backdrop-filter: var(--panelBlur);
  flex: 1;
  position: relative;
  padding: 0.25em;
`;

const ItemContainerStyle: CSSProperties = {
  display: "flex",
  flexDirection: "column",
  flex: 1,
};

const AddButton = () => {
  const data: MainPanelItemButton = {
    itemType: "button",
    type: "button",
    key: "add",
    value: "add",
    label: "Add",
    engineEventName: "C2VM.TLE.CallAddCustomPhase"
  };
  return (
    <Row data={data}><Button {...data} /></Row>
  );
};

const BackButton = () => {
  const data: MainPanelItemButton = {
    itemType: "button",
    type: "button",
    key: "state",
    value: `${MainPanelState.Main}`,
    label: "Back",
    engineEventName: "C2VM.TLE.CallSetMainPanelState"
  };
  return (
    <Row data={data}><Button {...data} /></Row>
  );
};

const ManualControlButton = (props: {currentSignalGroup: number}) => {
  const clickHandler = () => {
    const manualSignalGroup = props.currentSignalGroup > 0 ? props.currentSignalGroup : 1;
    call("C2VM.TLE", "CallSetActiveCustomPhaseIndex", JSON.stringify({key: "ManualSignalGroup", value: manualSignalGroup}));
  };
  return (
    <Row hoverEffect={true}>
      <Button label="ManualControl" onClick={clickHandler} />
    </Row>
  );
};

export default function MainPanel(props: {items: MainPanelItem[]}) {
  let activeIndex = -1;
  let activeViewingIndex = -1;
  let activeItem: MainPanelItemCustomPhase | null = null;
  let currentItem: MainPanelItemCustomPhase | null = null;
  let currentSignalGroup = 0;
  let manualSignalGroup = 0;
  let length = 0;
  if (props.items.length > 0 && props.items[0].itemType == "customPhase") {
    activeIndex = props.items[0].activeIndex;
    activeViewingIndex = props.items[0].activeViewingIndex;
    currentSignalGroup = props.items[0].currentSignalGroup;
    manualSignalGroup = props.items[0].manualSignalGroup;
    length = props.items[0].length;
  }
  if (activeIndex >= 0) {
    const newActiveItem = props.items[activeIndex];
    if (newActiveItem.itemType == "customPhase") {
      activeItem = newActiveItem;
    }
  }
  if (manualSignalGroup > 0) {
    const newCurrentItem = props.items[manualSignalGroup - 1];
    if (newCurrentItem.itemType == "customPhase") {
      currentItem = newCurrentItem;
    }
  } else if (activeViewingIndex >= 0) {
    const newCurrentItem = props.items[activeViewingIndex];
    if (newCurrentItem.itemType == "customPhase") {
      currentItem = newCurrentItem;
    }
  } else if (currentSignalGroup > 0 && currentSignalGroup - 1 < props.items.length) {
    const newCurrentItem = props.items[currentSignalGroup - 1];
    if (newCurrentItem.itemType == "customPhase") {
      currentItem = newCurrentItem;
    }
  }
  return (
    <Container>
      <LeftPanelContainer>
        {manualSignalGroup <= 0 && <>
          <Scrollable style={{flex: 1}} contentStyle={ItemContainerStyle}>
            {props.items.map(item => item.itemType == "customPhase" && <Item data={item} />)}
          </Scrollable>
          {length > 0 && <Divider />}
          {length < 16 && <AddButton />}
          {length > 0 && <ManualControlButton currentSignalGroup={currentSignalGroup} />}
          <BackButton />
        </>}
        {manualSignalGroup > 0 && <ManualControlPanel items={props.items} />}
      </LeftPanelContainer>
      <RightPanelContainer>
        <Scrollable style={{flex: 1}} contentStyle={{flex: 1}} trackStyle={{marginLeft: "0.25em"}}>
          {activeItem && <SubPanel data={activeItem} />}
          {!activeItem && currentItem && <SubPanel data={currentItem} statisticsOnly={true} />}
        </Scrollable>
      </RightPanelContainer>
    </Container>
  );
}