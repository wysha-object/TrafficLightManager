import Button from "@/components/common/button";
import Delete from "@/components/common/icons/delete";
import Input from "@/components/common/input";
import Row from "@/components/main-panel/items/row";
import { useTranslate } from "@/localisations";
import { useValue, bindValue, trigger } from "cs2/api";
import { getModule } from "cs2/modding";
import { Dropdown, DropdownItem, DropdownToggle } from "cs2/ui";
import { useState } from "react";
import styled from "styled-components";

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

export default function Template(props: { item: CustomPhaseItem, itemIndex: number }) {
  let [selectTemplate, setSelectTemplate] = useState("");

  let [inputName, setInputName] = useState("");

  const settings = JSON.parse(useValue(bindValue("TrafficLightManager", "GetSettings", "{}"))) as Settings;
  settings.customPhaseTemplates.findIndex(
    (template) => template.m_Name === selectTemplate
  ) < 0 && setSelectTemplate(settings.defaultCustomPhaseTemplate.m_Name);

  const { t } = useTranslate();
  return (
    <>
      {
        props.item.bindWithTemplate &&
        <>
          <Row>
            <span style={{ flex: "1" }}>
              {t("CustomPhaseEditor.Template.BoundWith")}
            </span>
            <span style={{ color: "#77FF00" }}>
              {props.item.bindTemplate}
            </span>
          </Row>
          {
            settings.customPhaseTemplates.findIndex(
              (template) => template.m_Name === props.item.bindTemplate
            ) < 0 &&
            <>
              <Row>
                {t("CustomPhaseEditor.Template.InvalidBind")}
              </Row>
              <Row>
                <Button label="CustomPhaseEditor.Template.SaveAndBind" onClick={() => {
                  trigger("TrafficLightManager", "UpdateTemplate", JSON.stringify({
                    m_Name: props.item.bindTemplate,
                    m_IsPrioritiseTrack: props.item.prioritiseTrack,
                    m_IsPrioritisePublicCar: props.item.prioritisePublicCar,
                    m_IsPrioritisePedestrian: props.item.prioritisePedestrian,
                    m_MinimumDuration: props.item.minimumDuration,
                    m_MaximumDuration: props.item.maximumDuration,
                    m_TargetDurationMultiplier: props.item.targetDurationMultiplier,
                    m_LaneOccupiedMultiplier: props.item.laneOccupiedMultiplier,
                    m_IntervalExponent: props.item.intervalExponent,
                  }));
                }} />
              </Row>
            </>
          }
          {
            props.item.bindWithTemplate &&
            <Row>
              <Button label="CustomPhaseEditor.Template.Unbind" onClick={() => {
                trigger("TrafficLightManager", "UpdatePhaseTemplateBind", JSON.stringify({ index: props.itemIndex, bindWithTemplate: false }))
              }} />
            </Row>
          }
        </>
      }
      {
        !props.item.bindWithTemplate &&
        <>
          <Dropdown
            theme={getModule("game-ui/menu/themes/dropdown.module.scss", "classes")}
            content={
              settings.customPhaseTemplates.map((template) => (
                <DropdownItem 
                  key={template.m_Name}
                  value={template.m_Name}
                  onChange={(value) => {
                    setSelectTemplate(value);
                  }}
                >
                  <span style={{ flex: 1 }}>{template.m_Name}</span>
                  {template.m_Name !== settings.defaultCustomPhaseTemplate.m_Name &&
                    <IconContainer>
                      <Delete style={IconStyle} onClick={() => trigger("TrafficLightManager", "RemoveTemplate", template.m_Name)}></Delete>
                    </IconContainer>
                  }
                </DropdownItem>
              ))
            }
          >
            <Row>
              <DropdownToggle style={{ width: "100%" }}>
                {selectTemplate}
              </DropdownToggle>
            </Row>
          </Dropdown>
          <Row>
            <Button
              label="CustomPhaseEditor.Template.Apply"
              onClick={() => trigger("TrafficLightManager", "ApplyTemplate", JSON.stringify({ index: props.itemIndex, templateName: selectTemplate }))}
            />
          </Row>
          <Row>
            <Button
              label="CustomPhaseEditor.Template.ApplyAndBind"
              onClick={() => {
                trigger("TrafficLightManager", "ApplyTemplate", JSON.stringify({ index: props.itemIndex, templateName: selectTemplate }))
                trigger("TrafficLightManager", "UpdatePhaseTemplateBind", JSON.stringify({ index: props.itemIndex, bindWithTemplate: true, templateName: selectTemplate }))
              }}
            />
          </Row>
        </>
      }
      {
        !props.item.bindWithTemplate &&
        <>
          <Row>
            <Input style={{ width: "100%" }} onChange={(e) => setInputName(e.target.value)} value={inputName}>
            </Input>
          </Row>
          <Row>
            <div style={{ flex: "1", margin: "0 0.1em 0 0" }}>
              <Button disabled={inputName === settings.defaultCustomPhaseTemplate.m_Name || inputName === ""} label="CustomPhaseEditor.Template.Save" onClick={() => {
                trigger("TrafficLightManager", "UpdateTemplate", JSON.stringify({
                  m_Name: inputName,
                  m_IsPrioritiseTrack: props.item.prioritiseTrack,
                  m_IsPrioritisePublicCar: props.item.prioritisePublicCar,
                  m_IsPrioritisePedestrian: props.item.prioritisePedestrian,
                  m_MinimumDuration: props.item.minimumDuration,
                  m_MaximumDuration: props.item.maximumDuration,
                  m_TargetDurationMultiplier: props.item.targetDurationMultiplier,
                  m_LaneOccupiedMultiplier: props.item.laneOccupiedMultiplier,
                  m_IntervalExponent: props.item.intervalExponent,
                }))
              }} />
            </div>
            <div style={{ flex: "1", margin: "0 0 0 0.1em" }}>
              <Button disabled={inputName === settings.defaultCustomPhaseTemplate.m_Name || inputName === ""} label="CustomPhaseEditor.Template.SaveAndBind" onClick={() => {
                trigger("TrafficLightManager", "UpdateTemplate", JSON.stringify({
                  m_Name: inputName,
                  m_IsPrioritiseTrack: props.item.prioritiseTrack,
                  m_IsPrioritisePublicCar: props.item.prioritisePublicCar,
                  m_IsPrioritisePedestrian: props.item.prioritisePedestrian,
                  m_MinimumDuration: props.item.minimumDuration,
                  m_MaximumDuration: props.item.maximumDuration,
                  m_TargetDurationMultiplier: props.item.targetDurationMultiplier,
                  m_LaneOccupiedMultiplier: props.item.laneOccupiedMultiplier,
                  m_IntervalExponent: props.item.intervalExponent,
                }));
                trigger("TrafficLightManager", "UpdatePhaseTemplateBind", JSON.stringify({ index: props.itemIndex, bindWithTemplate: true, templateName: inputName }));
              }} />
            </div>
          </Row>
        </>

      }
    </>
  );
}