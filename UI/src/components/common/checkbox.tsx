import styled from 'styled-components';

const Box = styled.div`
  border-width: 2px;
  border-style: solid;
  border-bottom-color: var(--textColorDimmer);
  border-left-color: var(--textColorDimmer);
  border-right-color: var(--textColorDimmer);
  border-top-color: var(--textColorDimmer);
  border-radius: 3rem;
  margin: 0 0.5em 0 0;
  width: 1.0em;
  height: 1.0em;
  padding: 1px;
`;

const Checkmark = styled.div<{isChecked: boolean}>`
  width: 100%;
  height: 100%;
  mask-size: 100% auto;
  background-color: var(--textColor);
  opacity: ${props => props.isChecked ? 1 : 0};
`;

export default function Checkbox(props: {isChecked: boolean}) {
  return (
    <Box>
      <Checkmark isChecked={props.isChecked} style={{maskImage: "url(Media/Glyphs/Checkmark.svg)"}} />
    </Box>
  );
}