import fs from 'node:fs'

const MOD_CONFIGURATION_PATH = './UI/mod.json'
const CHANGELOG_PATH = './changelog.md'

console.log('Starting bump-version script')

let modConfiguration = JSON.parse(fs.readFileSync(MOD_CONFIGURATION_PATH, 'utf-8'))

const args = process.argv.slice(2)
if (args.length === 0) {
  console.error('Please provide a valid argument: x.y.z')
  process.exit(1)
}

modConfiguration['version'] = args[0]
fs.writeFileSync(MOD_CONFIGURATION_PATH, JSON.stringify(modConfiguration, null, 2), 'utf-8')
fs.writeFileSync(CHANGELOG_PATH, '', 'utf-8');
console.log(`Bumped version to ${args[0]}`)
console.log('Finished bumping version')