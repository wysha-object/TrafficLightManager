const fs = require('fs')

const STABLE_PUBLISH_CONFIGURATION_PATH = './PublishConfigurations/Stable.xml'
const BETA_PUBLISH_CONFIGURATION_PATH = './PublishConfigurations/Beta.xml'

const args = process.argv.slice(2)
if (args.length !== 1) {
  console.error('Please provide a valid argument: "stable" or "beta"')
  process.exit(1)
}

if (args[0] !== 'stable' && args[0] !== 'beta') {
  console.error('Invalid argument. Please provide "stable" or "beta"')
  process.exit(1)
}

const publishConfiguration =
  args[0] === 'stable'
    ? fs.readFileSync(STABLE_PUBLISH_CONFIGURATION_PATH, 'utf-8')
    : fs.readFileSync(BETA_PUBLISH_CONFIGURATION_PATH, 'utf-8')

fs.writeFileSync('./Code/Properties/PublishConfiguration.xml', publishConfiguration, 'utf-8')
