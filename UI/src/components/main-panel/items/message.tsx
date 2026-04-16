import { useTranslate } from '@/localisations';
import styled from 'styled-components';

const Container = styled.div`
  margin: 20rem auto;
  flex: 1;
  text-align: center;
`;

export default function Message(props: MainPanelItemMessage) {
  const { t } = useTranslate();
  return (
    <Container>
      {t(props.message)}
    </Container>
  );
}