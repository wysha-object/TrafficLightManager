import { bindValue, useValue } from "cs2/api";

import { CityConfigurationContext, defaultCityConfiguration, LocalisationContext } from "./context";

import MainPanel from "./components/main-panel";
import CustomPhaseTool from "./components/custom-phase-tool";

export default function App() {
  let localisation = JSON.parse(useValue(bindValue("TrafficLightManager", "GetLocalisation")));

  const cityConfigurationJson = useValue(bindValue("C2VM.TLE", "GetCityConfiguration", JSON.stringify(defaultCityConfiguration)));
  const cityConfiguration = JSON.parse(cityConfigurationJson);

  return (
    <CityConfigurationContext.Provider value={cityConfiguration}>
      <LocalisationContext.Provider value={localisation}>
        <MainPanel />
        <CustomPhaseTool />
      </LocalisationContext.Provider>
    </CityConfigurationContext.Provider>
  );
}