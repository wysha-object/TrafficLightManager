const fs = require('fs');
const path = require('path');

const LOCALISATIONS_DIR = path.join(__dirname, '../src/localisations');

/**
 * 将转义序列解析为实际字符
 * @param {string} str
 * @returns {string}
 */
function unescapeString(str) {
  return str
    .replace(/\\\\/g, '\0BACKSLASH\0')
    .replace(/\\"/g, '"')
    .replace(/\\n/g, '\n')
    .replace(/\\t/g, '\t')
    .replace(/\\r/g, '\r')
    .replace(/\0BACKSLASH\0/g, '\\');
}

/**
 * 将特殊字符转义为转义序列
 * @param {string} str
 * @returns {string}
 */
function escapeString(str) {
  return str
    .replace(/\\/g, '\\\\')
    .replace(/"/g, '\\"')
    .replace(/\n/g, '\\n')
    .replace(/\t/g, '\\t')
    .replace(/\r/g, '\\r');
}

/**
 * 解析 TypeScript 导出对象，保持 key 顺序
 * @param {string} content 
 * @returns {{ keys: string[], values: Record<string, string> }}
 */
function parseExport(content) {
  const match = content.match(/export default \{([\s\S]*?)\};?\s*$/);
  if (!match) return { keys: [], values: {} };

  const body = match[1];
  const keys = [];
  const values = {};

  // 匹配每个 key-value 对
  const regex = /(\w+):\s*["'`]((?:[^"'`\\]|\\.)*)["\'`]/g;
  let m;
  while ((m = regex.exec(body)) !== null) {
    const key = m[1];
    // 将转义序列解析为实际字符
    const value = unescapeString(m[2]);
    keys.push(key);
    values[key] = value;
  }

  return { keys, values };
}

/**
 * 生成 TypeScript 导出文件内容
 * @param {string[]} keys 按顺序的 key 列表
 * @param {Record<string, string>} values key-value 映射
 * @returns {string}
 */
function generateExport(keys, values) {
  const lines = ['export default {'];
  keys.forEach((key, index) => {
    const value = values[key] || '';
    // 正确转义所有特殊字符
    const escapedValue = escapeString(value);
    const comma = index < keys.length - 1 ? ',' : '';
    lines.push(`  ${key}: "${escapedValue}"${comma}`);
  });
  lines.push('};');
  return lines.join('\n') + '\n';
}

/**
 * 判断是否是中文语言
 * @param {string} filename 
 * @returns {boolean}
 */
function isChineseLocale(filename) {
  return filename.startsWith('zh-');
}

function main() {
  // 读取基准文件
  const enUSPath = path.join(LOCALISATIONS_DIR, 'en-US.ts');
  const enUSContent = fs.readFileSync(enUSPath, 'utf-8');
  const enUS = parseExport(enUSContent);

  // 读取 zh-HANS 作为中文备选
  const zhHANSPath = path.join(LOCALISATIONS_DIR, 'zh-HANS.ts');
  const zhHANSContent = fs.readFileSync(zhHANSPath, 'utf-8');
  const zhHANS = parseExport(zhHANSContent);

  const baseKeys = enUS.keys;

  console.log(`Base keys from en-US.ts: ${baseKeys.length}`);
  console.log('---');

  // 获取所有本地化文件（排除 index.ts 和 en-US.ts）
  const files = fs.readdirSync(LOCALISATIONS_DIR)
    .filter(f => f.endsWith('.ts') && f !== 'index.ts' && f !== 'en-US.ts');

  let totalFixed = 0;

  files.forEach(file => {
    const filePath = path.join(LOCALISATIONS_DIR, file);
    const content = fs.readFileSync(filePath, 'utf-8');
    const parsed = parseExport(content);

    const isChinese = isChineseLocale(file);
    const missingKeys = baseKeys.filter(k => !(k in parsed.values));

    // 添加缺失的 key
    missingKeys.forEach(key => {
      if (isChinese && key in zhHANS.values) {
        // 中文语言优先使用 zh-HANS
        parsed.values[key] = zhHANS.values[key];
        console.log(`  [+] ${key}: Using zh-HANS fallback`);
      } else {
        // 其他语言使用 en-US
        parsed.values[key] = enUS.values[key];
        console.log(`  [+] ${key}: Using en-US fallback`);
      }
    });

    // 按照 en-US 的 key 顺序重新生成文件（始终写入以确保格式和转义一致）
    const newContent = generateExport(baseKeys, parsed.values);
    fs.writeFileSync(filePath, newContent, 'utf-8');

    if (missingKeys.length > 0) {
      console.log(`[FIXED] ${file}: Fixed ${missingKeys.length} missing keys`);
      totalFixed += missingKeys.length;
    } else {
      console.log(`[OK] ${file}: Reformatted`);
    }
  });

  console.log('---');
  if (totalFixed > 0) {
    console.log(`Done! Fixed ${totalFixed} missing keys in total.`);
  } else {
    console.log('All localisations are in sync!');
  }
}

main();
