import TooltipContainer from "@/components/common/tooltip-container";
import { useTranslate } from "@/localisations";

export default function TrafficSign(props: { state: CustomPhaseSignalState }) {
  const { t } = useTranslate();
  let text = "";
  if (props.state == "go") {
    text = t("TrafficSignGo");
  } else if (props.state == "yield") {
    text = t("TrafficSignYield");
  } else if (props.state == "stop") {
    text = t("TrafficSignStop");
  }
  return (
    <TooltipContainer>
      {text}
    </TooltipContainer>
  );
}