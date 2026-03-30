import { useContext } from 'react';
import styled from 'styled-components';

import { LocaleContext } from '@/context';
import { getString } from '@/localisations';

const Container = styled.div`
  display: flex;
  justify-content: space-between;
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
  align-items: center;
`;

const TitleText = styled.div`
  color: var(--textColorDim);
  flex-grow: 1;
  flex-shrink: 1;
  flex-basis: auto;
`;

const SecondaryText = styled.div`
  color: var(--textColorDim);
  margin-left: 6rem;
`;

export default function TitleDim(props: MainPanelItemTitle) {
  const locale = useContext(LocaleContext);
  return (
    <Container>
      <TitleText>{getString(locale, props.title)}</TitleText>
      {props.secondaryText && <SecondaryText>{getString(locale, props.secondaryText)}</SecondaryText>}
    </Container>
  );
}