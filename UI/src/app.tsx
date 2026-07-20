import { bindValue, useValue } from "cs2/api";

import { CityConfigurationContext, defaultCityConfiguration, EdgeGroupMaskContextClipboard, LocalisationContext, SubLaneGroupMaskContextClipboard } from "./context";

import MainPanel from "./components/main-panel";
import CustomPhaseTool from "./components/custom-phase-tool";
import ClipboardPanel from "./components/custom-phase-tool/clipboard-panel";

export default function App() {
  let localisation = JSON.parse(useValue(bindValue("TrafficLightManager", "GetLocalisation")));

  const cityConfigurationJson = useValue(bindValue("TrafficLightManager", "GetCityConfiguration", JSON.stringify(defaultCityConfiguration)));
  const cityConfiguration = JSON.parse(cityConfigurationJson);

  return (
    <CityConfigurationContext.Provider value={cityConfiguration}>
      <LocalisationContext.Provider value={localisation}>
        <EdgeGroupMaskContextClipboard.Provider>
          <SubLaneGroupMaskContextClipboard.Provider>
            <MainPanel />
            <CustomPhaseTool />
            <ClipboardPanel />
          </SubLaneGroupMaskContextClipboard.Provider>
        </EdgeGroupMaskContextClipboard.Provider>
      </LocalisationContext.Provider>
    </CityConfigurationContext.Provider>
  );
}