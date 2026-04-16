const fs = require('fs');
const path = require('path');

const i18nDir = "./Code/Resources/Localization";
const baseLocalization = "en-US";
const fallbackLocalizationMap = {
    "zh": "zh-HANS"
};

console.log('Start processing localization files');

const files = fs.readdirSync(i18nDir);
const localizationMap = new Map(files
    .filter(filePath => filePath.endsWith('.json'))
    .map(filePath => {
        const baseName = path.basename(filePath, '.json');
        const jsonRs = JSON.parse(fs.readFileSync(path.join(i18nDir, filePath), 'utf-8'));
        return [
            baseName,
            Object.keys(jsonRs)
                .sort()
                .reduce((acc, key) => {
                    acc[key] = jsonRs[key];
                    return acc;
                }, {})
        ]
    })
);

const baseTranslation = localizationMap.get(baseLocalization);
localizationMap.forEach((translation, localization) => {
    if (localization === baseLocalization) {
        return;
    }

    for (const key of Object.keys(baseTranslation).concat(Object.keys(translation))) {
        if (!Object.hasOwn(baseTranslation, key)) {
            delete translation[key];
            console.log(`REMOVE: Removed extra key "${key}" from localization "${localization}".`);
        } else if (!Object.hasOwn(translation, key)) {
            let target = baseLocalization;
            for (const prefix of Object.keys(fallbackLocalizationMap)) {
                if (localization.startsWith(prefix)) {
                    target = fallbackLocalizationMap[prefix];
                    break;
                }
            }

            translation[key] = localizationMap.get(target)[key];
            console.log(`ADD: Added missing key "${key}" to localization "${localization}" from "${target}".`);
        }
    }
});

localizationMap.forEach((translation, localization) => {
    fs.writeFileSync(path.join(i18nDir, `${localization}.json`), JSON.stringify(translation, null, 4), 'utf-8');
});

console.log('Finish processing localization files');