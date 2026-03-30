import { useContext } from "react";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

import TooltipContainer from "@/components/common/tooltip-container";

export default function LinkPhase(props: {link: boolean}) {
  const locale = useContext(LocaleContext);
  return (
    <TooltipContainer>
      {props.link && getString(locale, "LinkPhase")}
      {!props.link && getString(locale, "UnlinkPhase")}
    </TooltipContainer>
  );
}