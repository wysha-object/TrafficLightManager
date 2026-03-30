import { ModRegistrar } from "cs2/modding";

import App from './app';

const register: ModRegistrar = (moduleRegistry) => {
  moduleRegistry.append("Menu", () => (
    <div>
      <h1>test123456</h1>
    </div>
  ))
  moduleRegistry.append("GameTopLeft", () => (
    <div id="c2vm-tle" style={{margin: 0}}>
      <App />
    </div>
  ));
};

export default register;