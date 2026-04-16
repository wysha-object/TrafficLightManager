import { bindValue, useValue } from "cs2/api";

const useTranslate = () => {
  let locale = JSON.parse(useValue(bindValue("TrafficLightManager", "GetLocalisation")));
  let t = (key: string) => locale[key] || key;
  return {
    t
  }
}

export {
  useTranslate,
};