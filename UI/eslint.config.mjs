import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";
import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";

export default [
  {files: ["**/*.{js,mjs,cjs,ts,jsx,tsx}"]},
  {languageOptions: { globals: globals.browser }},
  {settings: { react: { version: "detect" } }},
  pluginJs.configs.recommended,
  ...tseslint.configs.recommended,
  react.configs.flat.recommended,
  reactHooks.configs['recommended-latest'],
  {rules: {
    "react/react-in-jsx-scope": "off",
    "@typescript-eslint/no-unused-vars": [
      "warn",
      {
        argsIgnorePattern: "^_",
        varsIgnorePattern: "^_",
        caughtErrorsIgnorePattern: "^_",
      },
    ],
  }},
];