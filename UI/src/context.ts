import { createContext } from "react";
import { createClipboard } from "./hooks/clipboard";

const defaultCityConfiguration = {
  leftHandTraffic: false
};

const CityConfigurationContext = createContext<CityConfiguration>(defaultCityConfiguration);
const LocalisationContext = createContext({});
const EdgeGroupMaskContextClipboard = createClipboard<EdgeGroupMask>();
const SubLaneGroupMaskContextClipboard = createClipboard<SubLaneGroupMask>();

export {
  CityConfigurationContext,
  defaultCityConfiguration,
  LocalisationContext,
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
};