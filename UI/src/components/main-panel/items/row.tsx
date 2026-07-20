import { CSSProperties } from 'react';
import styled from 'styled-components';

const Container = styled.div<{ hoverEffect?: boolean, hoverStyle?: string }>`
  padding: 0.25em 0.5em;
  width: 100%;
  display: flex;
  align-items: center;
  ${props => props.hoverEffect ? `
    &:hover {
      filter: brightness(1.2);
      background-color:  var(--panelColorNormal);
      ${props.hoverStyle}
    }
  ` : ''}
`;

export default function Row(props: { onClick?: () => void, children: React.ReactNode, hoverEffect?: boolean, hoverStyle?: string, style?: CSSProperties }) {
  return (
    <Container onClick={props.onClick} style={props.style} hoverEffect={props.hoverEffect} hoverStyle={props.hoverStyle}>
      {props.children}
    </Container>
  );
}