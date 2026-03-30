import { createContext } from "react";

import { defaultLocale } from "./localisations";

const defaultCityConfiguration = {
  leftHandTraffic: false
};

const CityConfigurationContext = createContext<CityConfiguration>(defaultCityConfiguration);
const LocaleContext = createContext(defaultLocale);

export {
  CityConfigurationContext,
  defaultCityConfiguration,
  LocaleContext
};