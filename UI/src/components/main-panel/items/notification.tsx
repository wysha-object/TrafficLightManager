import styled from 'styled-components';

import { useTranslate } from '@/localisations';

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

export enum NotificationType {
  Warning = 0,
  Notice = 1
}

export default function Notification(props: { onClick: () => void, notificationType: NotificationType, label: string }) {
  const { t } = useTranslate();
  return (
    <>
      {props.notificationType == NotificationType.Warning &&
        <Warning onClick={props.onClick}>
          <Image src="Media/Game/Icons/AdvisorNotifications.svg" />
          <Label>{t(props.label)}</Label>
        </Warning>}
      {props.notificationType == NotificationType.Notice &&
        <Notice onClick={props.onClick}>
          <Image src="Media/Game/Icons/AdvisorNotifications.svg" />
          <Label>{t(props.label)}</Label>
        </Notice>}
    </>
  );
}