const fs = require('fs')
const path = require('path')

const i18nDir = './Code/lang'

const files = fs.readdirSync(i18nDir)
let localizationMap = new Map(
  files
    .filter((filePath) => filePath.endsWith('.json'))
    .map((filePath) => {
      const baseName = path.basename(filePath, '.json')
      const jsonRs = JSON.parse(
        fs.readFileSync(path.join(i18nDir, filePath), 'utf-8'),
      )
      return [
        baseName,
        Object.keys(jsonRs)
          .sort()
          .reduce((acc, key) => {
            acc[key] = jsonRs[key]
            return acc
          }, {}),
      ]
    }),
)

localizationMap.forEach((translation, localization) => {
  fs.writeFileSync(
    path.join(i18nDir, `${localization}.json`),
    JSON.stringify(
      Object.keys(translation)
        .sort()
        .reduce((acc, key) => {
          acc[key] = translation[key]
          return acc
        }, {}),
      null,
      4,
    ),
    'utf-8',
  )
})

console.log('Finish processing localization files')
