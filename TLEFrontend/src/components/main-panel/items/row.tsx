import styled, { CSSProperties } from 'styled-components';
import { engineCall } from '@/engine';

const Container = styled.div<{hoverEffect?: boolean}>`
  padding: 0.25em 0.5em;
  width: 100%;
  display: flex;
  align-items: center;
  &:hover {
    filter: ${props => props.hoverEffect ? "brightness(1.2) contrast(1.2)" : "none"};
  }
`;

export default function Row(props: {data?: MainPanelItem, children: React.ReactNode, hoverEffect?: boolean, style?: CSSProperties}) {
  const clickHandler = () => {
    if (props.data && "engineEventName" in props.data && props.data.engineEventName) {
      engineCall(props.data.engineEventName, JSON.stringify(props.data));
    }
  };

  return (
    <Container onClick={clickHandler} style={props.style} hoverEffect={props.hoverEffect}>
      {props.children}
    </Container>
  );
}