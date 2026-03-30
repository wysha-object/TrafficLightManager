import { useContext } from "react";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

import TooltipContainer from "@/components/common/tooltip-container";

export default function TrafficSign(props: {state: CustomPhaseSignalState}) {
  const locale = useContext(LocaleContext);
  let text = "";
  if (props.state == "go") {
    text = getString(locale, "TrafficSignGo");
  } else if (props.state == "yield") {
    text = getString(locale, "TrafficSignYield");
  } else if (props.state == "stop") {
    text = getString(locale, "TrafficSignStop");
  }
  return (
    <TooltipContainer>
      {text}
    </TooltipContainer>
  );
}