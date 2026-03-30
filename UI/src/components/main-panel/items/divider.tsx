import styled from 'styled-components';

const Container = styled.div`
  height: 2px;
  width: auto;
  border: 2px solid rgba(255, 255, 255, 0.1);
  margin: 6rem -6rem;
`;

export default function Divider() {
  return (
    <Container />
  );
}