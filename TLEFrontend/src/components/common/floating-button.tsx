import { MouseEventHandler } from 'react';
import styled from 'styled-components';

import Tooltip from './tooltip';

const FloatingButtonContainer = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  width: var(--floatingToggleSize);
  height: var(--floatingToggleSize);
  padding-top: var(--gap2);
  padding-right: var(--gap2);
  padding-bottom: var(--gap2);
  padding-left: var(--gap2);
  background-color: var(--accentColorNormal);
  border-top-left-radius: var(--floatingToggleBorderRadius);
  border-top-right-radius: var(--floatingToggleBorderRadius);
  border-bottom-left-radius: var(--floatingToggleBorderRadius);
  border-bottom-right-radius: var(--floatingToggleBorderRadius);
  margin-right: 6rem;
  margin-bottom: 6rem;
  &:hover {
    background-color: var(--accentColorNormal-hover);
  }
`;

const FloatingButtonImage = styled.img`
  width: 100%;
  height: 100%;
`;

export default function FloatingButton(props: {show: boolean, src: string, tooltip: string, onClick?: MouseEventHandler<HTMLDivElement>}) {
  return (
    <Tooltip position="bottom-start" tooltip={props.tooltip} tooltipStyle={{marginTop: 0}}>
      <FloatingButtonContainer onClick={props.onClick} style={{display: props.show ? "flex" : "none"}}>
        <FloatingButtonImage src={props.src} />
      </FloatingButtonContainer>
    </Tooltip>
  );
}