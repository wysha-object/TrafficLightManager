import { MouseEventHandler } from 'react';
import styled from 'styled-components';

const Container = styled.div`
  background-color: var(--panelColorDark);
  backdrop-filter: var(--panelBlur);
  color: var(--accentColorNormal);
  font-size: 14rem;
  padding: 6rem 10rem;
  min-height: 36rem;
  display: flex;
  flex-direction: row;
  align-items: center;
`;

const HeaderImage = styled.img`
  width: 24rem;
  height: 24rem;
`;

const HeaderTitle = styled.div`
  text-transform: uppercase;
  flex: 1;
  text-align: center;
  overflow-x: hidden;
  overflow-y: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
`;

export default function Header(props: {title: string, image: string, onMouseDown?: MouseEventHandler<HTMLDivElement>}) {
  return (
    <Container onMouseDown={props.onMouseDown}>
        <HeaderImage src={props.image} />
        <HeaderTitle>{props.title}</HeaderTitle>
    </Container>
  );
}