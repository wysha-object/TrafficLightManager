import { useContext } from 'react';
import styled from 'styled-components';

import { LocaleContext } from '@/context';
import { getString } from '@/localisations';
import { engineCall } from '@/engine';

const Notice = styled.div`
  border-radius: 3rem;
  padding: 8rem;
  display: flex;
  width: 100%;
  background-color: rgba(75, 200, 240, 0.5);
`;

const Warning = styled.div`
  border-radius: 3rem;
  padding: 8rem;
  display: flex;
  width: 100%;
  background-color: rgba(200, 0, 0, 0.5);
`;

const Image = styled.img`
  width: 20rem;
  height: 20rem;
  margin-right: 10rem;
`;

const Label = styled.div`
  color: var(--textColor);
  flex: 1;
`;

export default function Notification(props: { data: MainPanelItemNotification }) {
  const locale = useContext(LocaleContext);
  const clickHandler = () => {
    if (props.data.engineEventName && props.data.engineEventName.length > 0) {
      engineCall(props.data.engineEventName, JSON.stringify(props.data));
    }
  };
  return (
    <>
      {props.data.notificationType == "warning" &&
      <Warning onClick={clickHandler}>
        <Image src="Media/Game/Icons/AdvisorNotifications.svg" />
        <Label>{getString(locale, props.data.label)}</Label>
      </Warning>}
      {props.data.notificationType == "notice" &&
      <Notice onClick={clickHandler}>
        <Image src="Media/Game/Icons/AdvisorNotifications.svg" />
        <Label>{getString(locale, props.data.label)}</Label>
      </Notice>}
    </>
  );
}