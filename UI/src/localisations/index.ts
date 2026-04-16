import { LocalisationContext } from "@/context";
import { useContext } from "react";

const useTranslate = () => {
  let localisation: any = useContext(LocalisationContext);
  let t = (key: string) => localisation[key] || key;
  return {
    t
  }
}

export {
  useTranslate,
};