import { useContext } from "react";
import styled from "styled-components";

import { call } from "cs2/api";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

import Button from "@/components/common/button";
import Checkbox from "@/components/common/checkbox";
import Tooltip from "@/components/common/tooltip";
import TooltipIcon from "@/components/common/tooltip-icon";
import OptionsTooltip from '@/components/custom-phase-tool/tooltips/options';
import Divider from "@/components/main-panel/items/divider";
import MainPanelRange from "@/components/main-panel/items/range";
import Row from "@/components/main-panel/items/row";
import Title from "@/components/main-panel/items/title";
import TitleDim from "@/components/main-panel/items/title-dim";

const DimLabel = styled.div`
  color: var(--textColorDim);
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  display: inline;
`;

const ItemTitle = (props: {title: string, secondaryText?: string, tooltip?: React.ReactNode, dim?: boolean}) => {
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
          <TooltipIcon style={{marginLeft: "0.25em"}} />
        </Tooltip>
      </>}
    </Row>
  );
};

const EndPhaseButton = (props: {index: number, disabled?: boolean}) => {
  const clickHandler = () => {
    if (!props.disabled) {
      call("C2VM.TLE", "CallUpdateCustomPhaseData", JSON.stringify({key: "EndPhasePrematurely", index: props.index}));
    }
  };
  return (
    <Row hoverEffect={!props.disabled}>
      <Button
        label={props.disabled ? "PhaseEndRequested" : "EndPhasePrematurely"}
        disabled={props.disabled}
        onClick={clickHandler}
      />
    </Row>
  );
};

export default function SubPanel(props: {data: MainPanelItemCustomPhase | null, statisticsOnly?: boolean}) {
  const locale = useContext(LocaleContext);
  const data = props.data;

  if (!data) {
    return <></>;
  }

  return (
    <>
      {!props.statisticsOnly && <>
        <ItemTitle title="Options" tooltip={<OptionsTooltip />} />
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
          <DimLabel>{getString(locale, "PrioritiseTrack")}</DimLabel>
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
          <DimLabel>{getString(locale, "PrioritisePublicCar")}</DimLabel>
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
          <DimLabel>{getString(locale, "PrioritisePedestrian")}</DimLabel>
        </Row>
        <Divider />
        <ItemTitle title="Adjustments" />
        <MainPanelRange data={{
            itemType: "range",
            key: "MinimumDuration",
            label: "MinimumDuration",
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
        />
        <MainPanelRange data={{
            itemType: "range",
            key: "MaximumDuration",
            label: "MaximumDuration",
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
        />
        <MainPanelRange data={{
            itemType: "range",
            key: "TargetDurationMultiplier",
            label: "TargetDurationMultiplier",
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
        />
        <MainPanelRange data={{
            itemType: "range",
            key: "LaneOccupiedMultiplier",
            label: "LaneOccupiedMultiplier",
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
        />
        <MainPanelRange data={{
            itemType: "range",
            key: "IntervalExponent",
            label: "IntervalExponent",
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
        />
        <Divider />
      </>}
      <ItemTitle title="Statistics" />
      <ItemTitle title="Timer" secondaryText={`${data.timer} / ${Round(Math.min(Math.max(data.targetDuration, data.minimumDuration), data.maximumDuration))}`} dim={true} />
      <ItemTitle title="Priority" secondaryText={`${data.priority}`} dim={true} />
      <ItemTitle title="LastRun" secondaryText={`${data.turnsSinceLastRun}`} dim={true} />
      <ItemTitle title="CarFlow" secondaryText={`${Round(data.carFlow)}`} dim={true} />
      <ItemTitle title="LanesOccupied" secondaryText={`${data.carLaneOccupied}, ${data.publicCarLaneOccupied}, ${data.trackLaneOccupied}, ${data.pedestrianLaneOccupied}`} dim={true} />
      <ItemTitle title="WeightedWaiting" secondaryText={`${Round(data.weightedWaiting)}`} dim={true} />
      {data.activeIndex < 0 && data.manualSignalGroup <= 0 && data.currentSignalGroup == data.index + 1 && <EndPhaseButton index={data.index} disabled={data.endPhasePrematurely} />}
    </>
  );
}

function Round(num: number): number {
  return Math.round(num * 100) / 100;
}