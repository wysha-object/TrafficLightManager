import styled from "styled-components";

import Info from "@/components/common/icons/info";
import { CSSProperties } from "react";

const IconContainer = styled.div<{disabled?: boolean}>`
  display: flex;
  border-radius: 0.2em;
  &:hover {
    filter: ${props => props.disabled ? "none" : "brightness(1.2) contrast(1.2)"};
  }
`;

const IconStyle = {
  color: "var(--accentColorNormal)",
  width: "1.0em",
  height: "1.0em",
  fontSize: "1.0em"
};

export default function TooltipIcon(props: {style?: CSSProperties, iconStyle?: CSSProperties}) {
  return (
    <IconContainer style={props.style}>
      <Info style={{...IconStyle, ...props.iconStyle}} />
    </IconContainer>
  );
}