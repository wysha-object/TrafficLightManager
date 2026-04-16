import TooltipContainer from "@/components/common/tooltip-container";
import { useTranslate } from "@/localisations";

export default function LinkPhase(props: { link: boolean }) {
  const { t } = useTranslate();
  return (
    <TooltipContainer>
      {props.link && t("LinkPhase")}
      {!props.link && t("UnlinkPhase")}
    </TooltipContainer>
  );
}