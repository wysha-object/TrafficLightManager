import TrafficLightSvg from 'assets/images/traffic-light.svg'

export default function LogoIcon(props: React.SVGProps<SVGSVGElement>) {
  return (
    <TrafficLightSvg
      {...props}
      style={{ width: '36rem', height: '36rem', ...props.style }}
    />
  )
}
