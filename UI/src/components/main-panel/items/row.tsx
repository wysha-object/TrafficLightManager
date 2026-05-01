import styled, { CSSProperties } from 'styled-components';

const Container = styled.div<{hoverEffect?: boolean}>`
  padding: 0.25em 0.5em;
  width: 100%;
  display: flex;
  align-items: center;
  &:hover {
    filter: ${props => props.hoverEffect ? "brightness(1.2) contrast(1.2)" : "none"};
  }
`;

export default function Row(props: { onClick?: () => void, children: React.ReactNode, hoverEffect?: boolean, style?: CSSProperties }) {
  return (
    <Container onClick={props.onClick} style={props.style} hoverEffect={props.hoverEffect}>
      {props.children}
    </Container>
  );
}