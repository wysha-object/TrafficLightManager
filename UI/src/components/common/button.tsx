import { MouseEventHandler, useContext } from 'react';
import styled from 'styled-components';

import { LocaleContext } from '@/context';
import { getString } from '@/localisations';

const ButtonComponent = styled.div<{disabled?: boolean}>`
  padding: 3rem;
  border-radius: 3rem;
  color: var(--accentColorLighter);
  background-color: var(--toolbarFieldColor);
  flex: 1;
  text-align: center;
  ${props => props.disabled ? "filter: brightness(1.0) contrast(0.6);" : ""}
  &:hover {
    ${props => props.disabled ? "" : "filter: brightness(1.2) contrast(1.2);"}
  }
`;

export default function Button(props: {label: string, disabled?: boolean, onClick?: MouseEventHandler<HTMLDivElement>}) {
  const locale = useContext(LocaleContext);
  return (
    <ButtonComponent {...props}>{getString(locale, props.label)}</ButtonComponent>
  );
}