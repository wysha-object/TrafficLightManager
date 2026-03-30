import styled from "styled-components";

import { call } from "cs2/api";

import LinkVariant from "@/components/common/icons/link-variant";
import LinkVariantOff from "@/components/common/icons/link-variant-off";
import Tooltip from "@/components/common/tooltip";
import LinkPhase from "@/components/custom-phase-tool/tooltips/link-phase";

const Container = styled.div`
  display: flex;
  align-items: center;
`;

const Divider = styled.div<{invisible?: boolean}>`
  height: 4px;
  background-color: ${props => props.invisible ? "transparent" : "rgba(255, 255, 255, 0.1)"};
  border-radius: 2px;
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  margin: 0.25em;
`;

const IconContainer = styled.div<{disabled?: boolean}>`
  display: flex;
  border-radius: 0.2em;
  &:hover {
    filter: ${props => props.disabled ? "none" : "brightness(1.2) contrast(1.2)"};
    background: ${props => props.disabled ? "transparent" : "rgba(0, 0, 0, 0.1)"};
  }
`;

const IconStyle = {
  color: "var(--textColorDim)",
  width: "1.0em",
  height: "1.0em",
  fontSize: "1.0em"
};

export default function ItemDivider(props: {index: number, linked: boolean}) {
  const clickHandler = () => {
    call("C2VM.TLE", "CallUpdateCustomPhaseData", JSON.stringify({key: "LinkedWithNextPhase", index: props.index}));
  };
  return (
    <Container>
      <Divider invisible={props.linked} style={{borderTopRightRadius: 0, borderBottomRightRadius: 0}} />
      <Tooltip position="right" tooltip={<LinkPhase link={!props.linked} />}>
        <IconContainer onClick={clickHandler}>
          {props.linked && <LinkVariantOff style={IconStyle} />}
          {!props.linked && <LinkVariant style={IconStyle} />}
        </IconContainer>
      </Tooltip>
      <Divider invisible={props.linked} style={{borderTopLeftRadius: 0, borderBottomLeftRadius: 0}} />
    </Container>
  );
}