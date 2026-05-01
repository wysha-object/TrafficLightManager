import { LocalisationContext } from "@/context";
import { useContext } from "react";

const useTranslate = () => {
  const localisation: any = useContext(LocalisationContext);
  const t = (key: string) => localisation[key] || key;
  return {
    t
  }
}

export {
  useTranslate,
};