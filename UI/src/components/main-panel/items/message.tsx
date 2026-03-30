import { useContext } from 'react';
import styled from 'styled-components';

import { LocaleContext } from '@/context';
import { getString } from '@/localisations';

const Container = styled.div`
  margin: 20rem auto;
  flex: 1;
  text-align: center;
`;

export default function Message(props: MainPanelItemMessage) {
  const locale = useContext(LocaleContext);
  return (
    <Container>
      {getString(locale, props.message)}
    </Container>
  );
}