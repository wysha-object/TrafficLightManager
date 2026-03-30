import { useContext } from "react";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

import TooltipContainer from "@/components/common/tooltip-container";

export default function Options() {
  const locale = useContext(LocaleContext);
  return (
    <TooltipContainer>
      {getString(locale, "test")}
    </TooltipContainer>
  );
}