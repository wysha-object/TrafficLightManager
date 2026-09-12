import fs from 'node:fs'
import { execSync } from 'node:child_process'

const I18N_SOURCE = './Code/lang/en-US.json'

console.log('Starting crowdin-upload script')

const source = JSON.parse(fs.readFileSync(I18N_SOURCE, 'utf-8'))
fs.writeFileSync(
  I18N_SOURCE,
  JSON.stringify(
    Object.keys(source)
      .sort()
      .reduce((previousValue, currentValue) => {
        previousValue[currentValue] = source[currentValue]
        return previousValue
      }, {}),
    null,
    4,
  ),
  'utf-8',
)

console.log('Finished formatting source')

execSync(
  `crowdin upload sources --branch master`,
  {
    shell: true,
    stdio: 'inherit',
  },
)

console.log('Finished crowdin upload')