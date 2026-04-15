import styled from "styled-components";

import { bindValue, call, useValue } from "cs2/api";

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
  const item: MainPanelItemTitle = {
    itemType: "title",
    ...props
  };
  return (
    <Row data={item}>
      {props.dim && <TitleDim {...item} />}
      {!props.dim && <Title {...item} />}
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
      call("C2VM.TLE", "CallUpdateCustomPhaseData", JSON.stringify({ key: "EndPhasePrematurely", index: props.index }));
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

export default function SubPanel(props: { data: CustomPhaseItem | null, itemIndex: number, statisticsOnly?: boolean }) {
  const data = props.data;

  const timer = useValue(bindValue("TrafficLightManager", "GetTimer"));
  const currentSignalGroup: number = useValue(bindValue("TrafficLightManager", "GetCurrentPhaseIndex"));
  const manualSignalGroup: number = useValue(bindValue("TrafficLightManager", "GetManualPhaseIndex"));

  if (!data) {
    return <></>;
  }

  const { t } = useTranslate();
  return (
    <>
      {!props.statisticsOnly && <>
        <ItemTitle title="CustomPhaseEditor.Options.Title" tooltip={<TooltipContainer>{t("CustomPhaseEditor.Options.Tooltip")}</TooltipContainer>} />
        <Row hoverEffect={true} data={{
          itemType: "checkbox",
          type: "",
          isChecked: data.prioritiseTrack,
          key: "PrioritiseTrack",
          value: "0",
          label: "",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
        >
          <Checkbox isChecked={data.prioritiseTrack} />
          <DimLabel>{t("CustomPhaseEditor.Options.PrioritiseTrack")}</DimLabel>
        </Row>
        <Row hoverEffect={true} data={{
          itemType: "checkbox",
          type: "",
          isChecked: data.prioritisePublicCar,
          key: "PrioritisePublicCar",
          value: "0",
          label: "",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
        >
          <Checkbox isChecked={data.prioritisePublicCar} />
          <DimLabel>{t("CustomPhaseEditor.Options.PrioritisePublicCar")}</DimLabel>
        </Row>
        <Row hoverEffect={true} data={{
          itemType: "checkbox",
          type: "",
          isChecked: data.prioritisePedestrian,
          key: "PrioritisePedestrian",
          value: "0",
          label: "",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
        >
          <Checkbox isChecked={data.prioritisePedestrian} />
          <DimLabel>{t("CustomPhaseEditor.Options.PrioritisePedestrian")}</DimLabel>
        </Row>
        <Divider />
        <ItemTitle title="CustomPhaseEditor.Adjustments.Title" />
        <MainPanelRange data={{
          itemType: "range",
          key: "MinimumDuration",
          label: "CustomPhaseEditor.Adjustments.MinimumDuration",
          value: data.minimumDuration,
          valuePrefix: "",
          valueSuffix: "s",
          min: 0,
          max: 30,
          step: 1,
          defaultValue: 2,
          enableTextField: true,
          textFieldRegExp: "^\\d{0,4}$",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
          index={props.itemIndex}
        />
        <MainPanelRange data={{
          itemType: "range",
          key: "MaximumDuration",
          label: "CustomPhaseEditor.Adjustments.MaximumDuration",
          value: data.maximumDuration,
          valuePrefix: "",
          valueSuffix: "s",
          min: 5,
          max: 300,
          step: 5,
          defaultValue: 300,
          enableTextField: true,
          textFieldRegExp: "^\\d{0,4}$",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
          index={props.itemIndex}
        />
        <MainPanelRange data={{
          itemType: "range",
          key: "TargetDurationMultiplier",
          label: "CustomPhaseEditor.Adjustments.TargetDurationMultiplier",
          value: data.targetDurationMultiplier,
          valuePrefix: "",
          valueSuffix: "CustomPedestrianDurationMultiplierSuffix",
          min: 0.1,
          max: 10,
          step: 0.1,
          defaultValue: 1,
          enableTextField: true,
          textFieldRegExp: "^\\d{0,4}(\\.\\d{0,2})?$",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
          index={props.itemIndex}
        />
        <MainPanelRange data={{
          itemType: "range",
          key: "LaneOccupiedMultiplier",
          label: "CustomPhaseEditor.Adjustments.LaneOccupiedMultiplier",
          value: data.laneOccupiedMultiplier,
          valuePrefix: "",
          valueSuffix: "CustomPedestrianDurationMultiplierSuffix",
          min: 0.1,
          max: 10,
          step: 0.1,
          defaultValue: 1,
          enableTextField: true,
          textFieldRegExp: "^\\d{0,4}(\\.\\d{0,2})?$",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
          index={props.itemIndex}
        />
        <MainPanelRange data={{
          itemType: "range",
          key: "IntervalExponent",
          label: "CustomPhaseEditor.Adjustments.IntervalExponent",
          value: data.intervalExponent,
          valuePrefix: "",
          valueSuffix: "",
          min: 0.1,
          max: 10,
          step: 0.1,
          defaultValue: 2,
          enableTextField: true,
          textFieldRegExp: "^\\d{0,4}(\\.\\d{0,2})?$",
          engineEventName: "C2VM.TLE.CallUpdateCustomPhaseData"
        }}
          index={props.itemIndex}
        />
        <Divider />
      </>}
      <ItemTitle title="CustomPhaseEditor.Statistics.Title" />
      <ItemTitle title="CustomPhaseEditor.Statistics.Timer" secondaryText={`${currentSignalGroup == props.itemIndex ? timer : 0}/ ${Round(Math.min(Math.max(data.targetDuration, data.minimumDuration), data.maximumDuration))}`} dim={true} />
      <ItemTitle title="CustomPhaseEditor.Statistics.Priority" secondaryText={`${data.priority}`} dim={true} />
      <ItemTitle title="CustomPhaseEditor.Statistics.LastRun" secondaryText={`${data.turnsSinceLastRun}`} dim={true} />
      <ItemTitle title="CustomPhaseEditor.Statistics.LastRun" secondaryText={`${data.turnsSinceLastRun}`} dim={true} />
      <ItemTitle title="CustomPhaseEditor.Statistics.LanesOccupied" secondaryText={`${data.carLaneOccupied}, ${data.publicCarLaneOccupied}, ${data.trackLaneOccupied}, ${data.pedestrianLaneOccupied}`} dim={true} />
      <ItemTitle title="CustomPhaseEditor.Statistics.WeightedWaiting" secondaryText={`${Round(data.weightedWaiting)}`} dim={true} />
      {manualSignalGroup < 0 && currentSignalGroup == props.itemIndex && <EndPhaseButton index={props.itemIndex} disabled={data.endPhasePrematurely} />}
    </>
  );
}

function Round(num: number): number {
  return Math.round(num * 100) / 100;
}