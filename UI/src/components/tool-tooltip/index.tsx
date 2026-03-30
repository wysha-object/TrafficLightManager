import { useContext, useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import styled from "styled-components";

import { bindValue, useValue } from "cs2/api";

import { LocaleContext } from "@/context";
import { getString } from "@/localisations";

const Container = styled.div`
  position: fixed;
  top: 0;
  left: 0;
`;

const TooltipContainer = styled.div`
  font-size: var(--fontSizeXS);
  color: var(--textColorDim);
  background-color: var(--tooltipColor);
  filter: var(--tooltipFilter);
  border-radius: 4rem;
  padding: 0.25em 0.5em;
  margin: 0.25em 0 0 0;
  z-index: 0;
  display: flex;
  flex-direction: row;
  align-items: center;
`;

const Image = styled.img`
  width: 1.25em;
  height: 1.25em;
  margin-right: 0.25em;
`;

export default function ToolTooltip() {
  const locale = useContext(LocaleContext);

  const [top, setTop] = useState(0);
  const [left, setLeft] = useState(0);

  const tooltipMessage: ToolTooltipMessage[] = useValue(bindValue("C2VM.TLE", "GetToolTooltipMessage", []));

  useEffect(() => {
    const mouseMoveHandler = (e: MouseEvent) => {
      setTop(e.clientY + 20);
      setLeft(e.clientX + 20);
    };
    document.body.addEventListener("mousemove", mouseMoveHandler);
    return () => document.body.removeEventListener("mousemove", mouseMoveHandler);
  }, []);

  const tooltip = useMemo(() => tooltipMessage.map(item => (
    <TooltipContainer key={item.image + item.message}>
      <Image src={item.image} />
      {getString(locale, item.message)}
    </TooltipContainer>
  )), [locale, tooltipMessage]);

  return (
    <>
      {tooltipMessage.length > 0 && top > 0 && createPortal(
        <Container style={{transform: "translate(" + left + "px, " + top + "px)"}}>
          {tooltip}
        </Container>,
        document.body
      )}
    </>
  );
}