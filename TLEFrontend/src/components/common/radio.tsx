import styled from 'styled-components';

const Circle = styled.div`
  border-width: 2px;
  border-style: solid;
  border-bottom-color: var(--accentColorNormal);
  border-left-color: var(--accentColorNormal);
  border-right-color: var(--accentColorNormal);
  border-top-color: var(--accentColorNormal);
  border-radius: 50%;
  margin: 0 0.5em 0 0;
  width: 1.0em;
  height: 1.0em;
  padding: 3px;
`;

const Bullet = styled.div<{isChecked: boolean}>`
  width: 100%;
  height: 100%;
  background-color: var(--textColor);
  opacity: ${props => props.isChecked ? 1 : 0};
  border-radius: 50%;
`;

export default function Radio(props: {isChecked: boolean}) {
  return (
    <Circle>
      <Bullet isChecked={props.isChecked} />
    </Circle>
  );
}