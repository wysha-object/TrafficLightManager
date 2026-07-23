import InfoSvg from 'assets/images/info.svg'

export default function TipIcon(props: React.SVGProps<SVGSVGElement>) {
  return <InfoSvg style={{ fill: 'var(--accentColorNormal)' }} {...props} />
}
