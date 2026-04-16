import { LocalisationContext } from "@/context";
import { useContext } from "react";

const useTranslate = () => {
  let locale: any = useContext(LocalisationContext);
  let t = (key: string) => locale[key] || key;
  return {
    t
  }
}

export {
  useTranslate,
};