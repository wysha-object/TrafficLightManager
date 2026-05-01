import { CSSProperties, useEffect, useState } from "react";
import styled from "styled-components";

import { bindValue, call, trigger, useValue } from "cs2/api";

import Button from "@/components/common/button";
import Scrollable from "@/components/common/scrollable";
import Divider from "@/components/main-panel/items/divider";
import Row from "@/components/main-panel/items/row";

import Item, { ItemState } from "./item";
import ManualControlPanel from "./manual-control-panel";
import SubPanel from "./sub-panel";
import { useTranslate } from "@/localisations";
import { ToolState } from "@/constants";

const Container = styled.div`
  width: 40em;
  display: flex;
  flex-direction: row;
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
`;

const TrafficLightsMembersContainer = styled.div`
  width: 10em;
  background-color: var(--panelColorNormal);
  backdrop-filter: var(--panelBlur);
  color: var(--textColor);
  flex: 1;
  position: relative;
  padding: 0.25em;
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
  const { t } = useTranslate();
  return (
    <Row onClick={() => {
      call("TrafficLightManager", "CallAddCustomPhase")
    }}><Button label={t("CustomPhaseEditor.Add")} /></Row>
  );
};

const ManualControlButton = (props: { onClick: () => void }) => {
  return (
    <Row hoverEffect={true}>
      <Button label="CustomPhaseEditor.ManualControl" onClick={props.onClick} />
    </Row>
  );
};

const AddMemberButton = () => {
  return (
    <Row>
      <Button label="CustomPhaseEditor.AddMember" onClick={() => trigger("TrafficLightManager", "SetToolState", ToolState.AddTrafficLights)} />
    </Row>
  )
}

const RemoveMemberButton = () => {
  return (
    <Row>
      <Button label="CustomPhaseEditor.RemoveMember" onClick={() => trigger("TrafficLightManager", "SetToolState", ToolState.RemoveTrafficLights)} />
    </Row>
  )
}

export default function MainPanel() {
  let [currentIndex, setCurrentIndex] = useState(-1);
  let [currentItemState, setCurrentItemState] = useState(ItemState.None);
  let [manualControl, setManualControl] = useState(false);

  const trafficLightGroup = JSON.parse(useValue(bindValue("TrafficLightManager", "GetTrafficLightGroup")));

  const trafficLightsMembers = JSON.parse(useValue(bindValue("TrafficLightManager", "GetTrafficLightsMembers"))) as any[];
  const customPhaseItems = JSON.parse(useValue(bindValue("TrafficLightManager", "GetCustomPhaseItems"))) as CustomPhaseItem[];

  useEffect(() => {
    if (currentItemState === ItemState.Editing) {
      trigger("TrafficLightManager", "SetToolState", ToolState.Editing);
    } else {
      trigger("TrafficLightManager", "SetToolState", ToolState.Choosed);
    }
  }, [currentItemState]);

  useEffect(() => {
    trigger("TrafficLightManager", "SetDisplayPhaseIndex", currentIndex);
    if (currentItemState === ItemState.Editing) {
      trigger("TrafficLightManager", "SetEditingPhaseIndex", currentIndex);
    } else {
      trigger("TrafficLightManager", "SetEditingPhaseIndex", -1);
    }
  }, [currentIndex, currentItemState]);

  let subPanelIndex = currentIndex >= 0 ? currentIndex : trafficLightGroup.currentPhaseIndex;
  let subPanelItem = subPanelIndex >= 0 ? customPhaseItems[subPanelIndex] : null;

  const { t } = useTranslate();
  return (
    <Container>
      <TrafficLightsMembersContainer>
        <Row>{t("CustomPhaseEditor.TrafficLightsMembers")}</Row>
        <Scrollable contentStyle={ItemContainerStyle}>
          {
            trafficLightsMembers.map(
              (item, _) => <Row>#{item.entityIndex}</Row>
            )
          }
        </Scrollable>
        <Divider />
        <AddMemberButton />
        <RemoveMemberButton />
      </TrafficLightsMembersContainer>
      <div style={{ width: "0.3em", backgroundColor: "var(--panelColorDark)" }}></div>
      <LeftPanelContainer>
        {!manualControl && <>
          <Scrollable style={{ flex: 1 }} contentStyle={ItemContainerStyle}>
            {customPhaseItems.map(
              (item, index) => <Item
                data={item}
                itemIndex={index}
                itemState={index === currentIndex ? currentItemState : ItemState.None}
                currentIndex={currentIndex}
                itemCount={customPhaseItems.length}
                trafficLightGroup={trafficLightGroup}
                updateItemState={(state) => {
                  if (state === ItemState.None) {
                    setCurrentIndex(-1);
                  } else {
                    setCurrentIndex(index);
                  }
                  setCurrentItemState(state);
                }}
                updateCurrentIndex={setCurrentIndex}
              />
            )}
          </Scrollable>
          {customPhaseItems.length > 0 && <Divider />}
          {customPhaseItems.length > 0 && <ManualControlButton onClick={() => setManualControl(true)} />}
          {customPhaseItems.length < 16 && <AddButton />}
        </>}
        {manualControl && <ManualControlPanel items={customPhaseItems} onBack={() => {
          trigger("TrafficLightManager", "SetManualPhaseIndex", -1);
          setManualControl(false);
        }} />}
      </LeftPanelContainer>
      <RightPanelContainer>
        <Scrollable style={{ flex: 1 }} contentStyle={{ flex: 1 }} trackStyle={{ marginLeft: "0.25em" }}>
          <SubPanel data={subPanelItem} itemIndex={subPanelIndex} statisticsOnly={subPanelIndex !== currentIndex || currentItemState !== ItemState.Editing} trafficLightGroup={trafficLightGroup} />
        </Scrollable>
      </RightPanelContainer>
    </Container>
  );
}