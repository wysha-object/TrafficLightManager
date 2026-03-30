import styled from 'styled-components';

import WalkPath from './icons/walk-path';
import HumanMalePath from './icons/human-male-path';

const Button = styled.div`
  border-radius: 50%;
  width: 30rem;
  height: 30rem;
  &:hover {
    filter: brightness(1.2) contrast(1.2);
  }
`;

export default function TrafficSignButton(props: {
  allow: boolean,
  variant: "traffic-light" | "sign" | "pedestrian",
  sign: "←" | "↑" | "→" | "↶" | "↷",
  state: CustomPhaseSignalState,
  style?: React.CSSProperties,
  onClick: React.MouseEventHandler<HTMLDivElement>
}) {
  let border = "#01619e";
  let backgroundColor = "#000000";
  let signColor = "black";
  let fontSize = 85;
  let textX = 50;
  let textY = 80;

  if (props.variant == "sign") {
    border = props.allow ? "#01619e" : "#DA2121";
    backgroundColor = props.allow ? "#01619e" : "#E0E0E0";
    signColor = props.allow ? "white" : "black";
  }
  if (props.variant == "traffic-light" || props.variant == "pedestrian") {
    border = "#111111";
    backgroundColor = "#111111";
    signColor = props.state != "stop" ? "#476a4c" : "#a83c19";
    signColor = props.state == "yield" ? "#4348b2" : signColor;
  }

  if (props.sign == "↶") {
    textX = 45;
    textY = 75;
    fontSize = 80;
  }
  if (props.sign == "↷") {
    textX = 55;
    textY = 75;
    fontSize = 80;
  }
  // if (props.state == "yield") {
  //   textX = 50;
  //   textY = 50;
  //   fontSize = 55;
  //   signColor = "black";
  //   if (props.sign == "↑") {
  //     textY = 55;
  //   }
  //   if (props.sign == "↶") {
  //     fontSize = 40;
  //     textY = 42;
  //   }
  //   if (props.sign == "↷") {
  //     fontSize = 40;
  //     textY = 42;
  //   }
  // }

  return (
    <Button onClick={props.onClick} style={props.style}>
      <svg width="100%" height="100%" viewBox="0 0 100 100">
        {props.state != "none" && <circle cx="50" cy="50" r="45" stroke={border} strokeWidth="10" fill={backgroundColor} />}
        {/* {props.state == "yield" && <polygon points="50 10, 90 95, 10 95" stroke="rgb(240, 0, 0)" strokeWidth="8" fill="rgb(240, 240, 240)" transform="rotate(180 50 50)" />} */}
        {props.variant != "pedestrian" && <text
          x={textX}
          y={textY}
          dominantBaseline="middle"
          textAnchor="middle"
          fontSize={fontSize}
          fontWeight="700"
          fontFamily="Overpass"
          fill={signColor}
        >
          {props.sign}
        </text>}
        {props.variant == "pedestrian" && props.state == "go" && <WalkPath fill={signColor} transform="translate(10, 10) scale(3.5 3.5)" />}
        {props.variant == "pedestrian" && props.state != "go" && <HumanMalePath fill={signColor} transform="translate(9, 8) scale(3.5 3.5)" />}
        {!props.allow && props.variant == "sign" && <rect x="5" y="45" width="90" height="10" transform="rotate(45 50 50)" fill={border} />}
      </svg>
    </Button>
  );
}