import { ModRegistrar } from "cs2/modding";

import App from './app';

const register: ModRegistrar = (moduleRegistry) => {
  moduleRegistry.append("GameTopLeft", () => (
    <App />
  ));
};

export default register;