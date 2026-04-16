import { createContext } from "react";

const defaultCityConfiguration = {
  leftHandTraffic: false
};

const CityConfigurationContext = createContext<CityConfiguration>(defaultCityConfiguration);
const LocalisationContext = createContext({});

export {
  CityConfigurationContext,
  defaultCityConfiguration,
  LocalisationContext
};