import { useEffect, useState } from "react";

import engine from "cohtml/cohtml";
import { bindValue, useValue } from "cs2/api";

import { CityConfigurationContext, defaultCityConfiguration, LocaleContext } from "./context";
import { defaultLocale } from "./localisations";

import MainPanel from "./components/main-panel";
import CustomPhaseTool from "./components/custom-phase-tool";

export default function App() {
  const [locale, setLocale] = useState(defaultLocale);

  const localeValue = useValue(bindValue("C2VM.TLE", "GetLocale", "{}"));
  const newLocale = JSON.parse(localeValue);
  if (newLocale.locale && newLocale.locale != locale) {
    setLocale(newLocale.locale);
  }

  const cityConfigurationJson = useValue(bindValue("C2VM.TLE", "GetCityConfiguration", JSON.stringify(defaultCityConfiguration)));
  const cityConfiguration = JSON.parse(cityConfigurationJson);

  useEffect(() => {
    const keyDownHandler = (event: KeyboardEvent) => {
      if (event.ctrlKey && event.key == "S") {
        engine.call("C2VM.TLE.CallKeyPress", JSON.stringify({ctrlKey: event.ctrlKey, key: event.key}));
      }
    };
    document.addEventListener("keydown", keyDownHandler);
    return () => document.removeEventListener("keydown", keyDownHandler);
  }, []);

  return (
    <CityConfigurationContext.Provider value={cityConfiguration}>
      <LocaleContext.Provider value={locale}>
        <MainPanel />
        <CustomPhaseTool />
      </LocaleContext.Provider>
    </CityConfigurationContext.Provider>
  );
}