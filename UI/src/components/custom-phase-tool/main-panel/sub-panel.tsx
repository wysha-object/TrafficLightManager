import styled from "styled-components";

import { call } from "cs2/api";

import { useTranslate } from "@/localisations";

import Button from "@/components/common/button";
import Checkbox from "@/components/common/checkbox";
import Tooltip from "@/components/common/tooltip";
import TooltipIcon from "@/components/common/tooltip-icon";
import Divider from "@/components/main-panel/items/divider";
import MainPanelRange from "@/components/main-panel/items/range";
import Row from "@/components/main-panel/items/row";
import Title from "@/components/main-panel/items/title";
import TitleDim from "@/components/main-panel/items/title-dim";
import TooltipContainer from "@/components/common/tooltip-container";

const DimLabel = styled.div`
  color: var(--textColorDim);
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  display: inline;
`;

const ItemTitle = (props: { title: string, secondaryText?: string, tooltip?: React.ReactNode, dim?: boolean }) => {
  return (
    <Row>
      {props.dim && <TitleDim title={props.title} secondaryText={props.secondaryText} />}
      {!props.dim && <Title title={props.title} secondaryText={props.secondaryText} />}
      {props.tooltip && <>
        <Tooltip position="right-start" tooltip={props.tooltip}>
          <TooltipIcon style={{ marginLeft: "0.25em" }} />
        </Tooltip>
      </>}
    </Row>
  );
};

const EndPhaseButton = (props: { index: number, disabled?: boolean }) => {
  const clickHandler = () => {
    if (!props.disabled) {
      call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "EndPhasePrematurely", index: props.index }));
    }
  };
  const { t } = useTranslate();
  return (
    <Row hoverEffect={!props.disabled}>
      <Button
        label={props.disabled ? t("CustomPhaseEditor.PhaseEndRequested") : t("CustomPhaseEditor.EndPhasePrematurely")}
        disabled={props.disabled}
        onClick={clickHandler}
      />
    </Row>
  );
};

export default function SubPanel(props: { data: CustomPhaseItem | null, itemIndex: number, statisticsOnly?: boolean, trafficLightGroup: TrafficLightGroup }) {
  const data = props.data;

  if (!data) {
    return <></>;
  }

  const { t } = useTranslate();
  return (
    <>
      {!props.statisticsOnly &&
        <>
          <ItemTitle title="CustomPhaseEditor.Options.Title" tooltip={<TooltipContainer>{t("CustomPhaseEditor.Options.Tooltip")}</TooltipContainer>} />
          <Row hoverEffect={true}
            onClick={() => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "PrioritiseTrack", index: props.itemIndex }));
            }}
          >
            <Checkbox isChecked={data.prioritiseTrack} />
            <DimLabel>{t("CustomPhaseEditor.Options.PrioritiseTrack")}</DimLabel>
            <div style={{ flex: 1 }}></div>
            <Tooltip position="right-start" tooltip={<TooltipContainer>{t("CustomPhaseEditor.Options.PrioritiseTrack.Tooltip")}</TooltipContainer>}>
              <TooltipIcon style={{ marginLeft: "0.25em" }} />
            </Tooltip>
          </Row>
          <Row hoverEffect={true}
            onClick={() => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "PrioritisePublicCar", index: props.itemIndex }));
            }}
          >
            <Checkbox isChecked={data.prioritisePublicCar} />
            <DimLabel>{t("CustomPhaseEditor.Options.PrioritisePublicCar")}</DimLabel>
            <div style={{ flex: 1 }}></div>
            <Tooltip position="right-start" tooltip={<TooltipContainer>{t("CustomPhaseEditor.Options.PrioritisePublicCar.Tooltip")}</TooltipContainer>}>
              <TooltipIcon style={{ marginLeft: "0.25em" }} />
            </Tooltip>
          </Row>
          <Row hoverEffect={true}
            onClick={() => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "PrioritisePedestrian", index: props.itemIndex }));
            }}
          >
            <Checkbox isChecked={data.prioritisePedestrian} />
            <DimLabel>{t("CustomPhaseEditor.Options.PrioritisePedestrian")}</DimLabel>
            <div style={{ flex: 1 }}></div>
            <Tooltip position="right-start" tooltip={<TooltipContainer>{t("CustomPhaseEditor.Options.PrioritisePedestrian.Tooltip")}</TooltipContainer>}>
              <TooltipIcon style={{ marginLeft: "0.25em" }} />
            </Tooltip>
          </Row>
          <Divider />
          <ItemTitle title="CustomPhaseEditor.Adjustments.Title" />
          <MainPanelRange
            onChange={(value) => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "MinimumDuration", value: value, index: props.itemIndex }));
            }}
            label={t("CustomPhaseEditor.Adjustments.MinimumDuration")}
            value={data.minimumDuration}
            valuePrefix={""}
            valueSuffix={"s"}
            min={0}
            max={30}
            step={1}
            defaultValue={2}
            enableTextField={true}
            textFieldRegExp={"^\\d{0,4}$"}
          />
          <MainPanelRange
            onChange={(value) => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "MaximumDuration", value: value, index: props.itemIndex }));
            }}
            label={t("CustomPhaseEditor.Adjustments.MaximumDuration")}
            value={data.maximumDuration}
            valuePrefix={""}
            valueSuffix={"s"}
            min={5}
            max={300}
            step={5}
            defaultValue={30}
            enableTextField={true}
            textFieldRegExp={"^\\d{0,4}$"}
          />
          <MainPanelRange
            onChange={(value) => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "TargetDurationMultiplier", value: value, index: props.itemIndex }));
            }}
            label={t("CustomPhaseEditor.Adjustments.TargetDurationMultiplier")}
            value={data.targetDurationMultiplier}
            valuePrefix={""}
            valueSuffix={t("CustomPedestrianDurationMultiplierSuffix")}
            min={0.1}
            max={10}
            step={0.1}
            defaultValue={1}
            enableTextField={true}
            textFieldRegExp={"^\\d{0,4}(\\.\\d{0,2})?$"}
          />
          <MainPanelRange
            onChange={(value) => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "LaneOccupiedMultiplier", value: value, index: props.itemIndex }));
            }}
            label={t("CustomPhaseEditor.Adjustments.LaneOccupiedMultiplier")}
            value={data.laneOccupiedMultiplier}
            valuePrefix={""}
            valueSuffix={t("CustomPedestrianDurationMultiplierSuffix")}
            min={0.1}
            max={10}
            step={0.1}
            defaultValue={1}
            enableTextField={true}
            textFieldRegExp={"^\\d{0,4}(\\.\\d{0,2})?$"}
            tooltip={<TooltipContainer>{t("CustomPhaseEditor.Adjustments.LaneOccupiedMultiplier.Tooltip")}</TooltipContainer>}
          />
          <MainPanelRange
            onChange={(value) => {
              call("TrafficLightManager", "CallUpdateCustomPhaseData", JSON.stringify({ key: "IntervalExponent", value: value, index: props.itemIndex }));
            }}
            label={t("CustomPhaseEditor.Adjustments.IntervalExponent")}
            value={data.intervalExponent}
            valuePrefix={""}
            valueSuffix={""}
            min={0.1}
            max={10}
            step={0.1}
            defaultValue={2}
            enableTextField={true}
            textFieldRegExp={"^\\d{0,4}(\\.\\d{0,2})?$"}
            tooltip={<TooltipContainer>{t("CustomPhaseEditor.Adjustments.IntervalExponent.Tooltip")}</TooltipContainer>}
          />
          <Divider />
        </>
      }
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.Title")}
        tooltip={<TooltipContainer>{t("CustomPhaseEditor.Statistics.Tooltip")}</TooltipContainer>}
      />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.Timer")}
        secondaryText={`
          ${props.trafficLightGroup.currentPhaseIndex == props.itemIndex ? props.trafficLightGroup.timer : 0}
          / 
          ${(props.trafficLightGroup.currentPhaseIndex == props.itemIndex ?
            Math.min(Math.max(props.trafficLightGroup.targetDuration, data.minimumDuration), data.maximumDuration) : data.minimumDuration
          ).toFixed(2)}s
        `}
        dim={true}
      />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.Priority")}
        secondaryText={`${data.priority}`}
        dim={true} />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.LastRun")}
        secondaryText={`${data.turnsSinceLastRun}`}
        dim={true} />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.CarFlow")}
        secondaryText={`${data.carFlow.toFixed(8)}`}
        dim={true} />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.CarLaneOccupied")}
        secondaryText={`${data.carLaneOccupied}`}
        dim={true}
        tooltip={<TooltipContainer>{t("CustomPhaseEditor.Statistics.CarLaneOccupied.Tooltip")}</TooltipContainer>}
      />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.PublicCarLaneOccupied")}
        secondaryText={`${data.publicCarLaneOccupied}`}
        dim={true}
        tooltip={<TooltipContainer>{t("CustomPhaseEditor.Statistics.PublicCarLaneOccupied.Tooltip")}</TooltipContainer>}
      />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.TrackLaneOccupied")}
        secondaryText={`${data.trackLaneOccupied}`}
        dim={true}
        tooltip={<TooltipContainer>{t("CustomPhaseEditor.Statistics.TrackLaneOccupied.Tooltip")}</TooltipContainer>}
      />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.CrossWalkOccupied")}
        secondaryText={`${data.pedestrianLaneOccupied}`}
        dim={true}
        tooltip={<TooltipContainer>{t("CustomPhaseEditor.Statistics.CrossWalkOccupied.Tooltip")}</TooltipContainer>}
      />
      <ItemTitle
        title={t("CustomPhaseEditor.Statistics.WeightedWaiting")}
        secondaryText={`${data.weightedWaiting.toFixed(2)}`}
        dim={true}
      />
      {props.trafficLightGroup.manualPhaseIndex < 0 && props.trafficLightGroup.currentPhaseIndex == props.itemIndex && <EndPhaseButton index={props.itemIndex} disabled={data.endPhasePrematurely} />}
    </>
  );
}