import { useTranslate } from '@/localisations';
import styled from 'styled-components';

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
  const { t } = useTranslate();
  return (
    <Container>
      <TitleText>{t(props.title)}</TitleText>
      {props.secondaryText && <SecondaryText>{t(props.secondaryText)}</SecondaryText>}
    </Container>
  );
}