import 'assets/styles/index.scss'
import { ModRegistrar } from 'cs2/modding'
import {
  CityConfigurationContext,
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
  CurrentFocusPhaseIndexContext,
} from 'context'
import { useGetCityConfigurationCmd, useGetDisplayPhaseIndexCmd } from 'hooks/cmds'
import ClipboardPanel from 'pages/clipboard-panel'
import CustomPhaseTool from 'pages/custom-phase-tool'
import MainPanel from 'pages/main-panel'
import { setDisplayPhaseIndexCmd } from 'hooks/cmds'

const register: ModRegistrar = (moduleRegistry) => {
  moduleRegistry.append('GameTopLeft', () => <App />)
}

function App() {
  const displayPhaseIndex = useGetDisplayPhaseIndexCmd()
  const cityConfiguration = useGetCityConfigurationCmd()

  return (
    <div id="traffic-light-manager-root">
      <CityConfigurationContext.Provider value={cityConfiguration}>
        <EdgeGroupMaskContextClipboard.Provider>
          <SubLaneGroupMaskContextClipboard.Provider>
            <CurrentFocusPhaseIndexContext.Provider
              value={[displayPhaseIndex, setDisplayPhaseIndexCmd]}
            >
              <MainPanel />
              <CustomPhaseTool />
              <ClipboardPanel />
            </CurrentFocusPhaseIndexContext.Provider>
          </SubLaneGroupMaskContextClipboard.Provider>
        </EdgeGroupMaskContextClipboard.Provider>
      </CityConfigurationContext.Provider>
    </div>
  )
}

export default register
