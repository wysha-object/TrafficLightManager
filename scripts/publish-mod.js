import { XMLParser, XMLBuilder } from 'fast-xml-parser'
import { execSync } from 'node:child_process'
import fs from 'node:fs'

const STABLE_PUBLISH_CONFIGURATION_PATH = './PublishConfigurations/Stable.xml'
const BETA_PUBLISH_CONFIGURATION_PATH = './PublishConfigurations/Beta.xml'

const args = process.argv.slice(2)
if (args.length !== 1) {
  console.error('Please provide a valid argument: "stable" or "beta"')
  process.exit(1)
}

args[0] = args[0].toLocaleUpperCase()

if (args[0] !== 'STABLE' && args[0] !== 'BETA') {
  console.error('Invalid argument. Please provide "stable" or "beta"')
  process.exit(1)
}

let publishConfiguration =
  args[0] === 'STABLE'
    ? fs.readFileSync(STABLE_PUBLISH_CONFIGURATION_PATH, 'utf-8')
    : fs.readFileSync(BETA_PUBLISH_CONFIGURATION_PATH, 'utf-8')

if (args[0] === 'BETA') {
  const parser = new XMLParser({
    ignoreAttributes: false,
  })
  const builder = new XMLBuilder({
    ignoreAttributes: false,
    format: true,
  })
  const parsedPublishConfiguration = parser.parse(publishConfiguration)

  const now = new Date()
  const date =
    now.getUTCFullYear().toString() +
    String(now.getUTCMonth() + 1).padStart(2, '0') +
    String(now.getUTCDate()).padStart(2, '0') +
    'T' +
    String(now.getUTCHours()).padStart(2, '0') +
    String(now.getUTCMinutes()).padStart(2, '0')

  const commitHash = execSync('git rev-parse HEAD')
    .toString()
    .trim()
    .slice(0, 7)

  parsedPublishConfiguration['Publish']['ModVersion']['@_Value'] =
    parsedPublishConfiguration['Publish']['ModVersion']['@_Value'] +
    '.' +
    date +
    '+' +
    commitHash

  publishConfiguration = builder.build(parsedPublishConfiguration)
}

fs.writeFileSync(
  './Code/Properties/PublishConfiguration.xml',
  publishConfiguration,
  'utf-8',
)

execSync(
  `dotnet publish Code/Code.csproj -p:PublishProfile=PublishNewVersion -p:ReleaseChannel=${args[0]}`,
  {
    shell: true,
    stdio: 'inherit'
  },
)
