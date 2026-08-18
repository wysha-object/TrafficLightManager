import 'assets/styles/index.scss'
import { ModRegistrar } from 'cs2/modding'
import {
  CityConfigurationContext,
  LocalisationContext,
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
  CurrentFocusPhaseIndexContext,
} from 'context'
import { useGetCityConfigurationCmd, useGetDisplayPhaseIndexCmd } from 'hooks/cmds'
import ClipboardPanel from 'pages/clipboard-panel'
import CustomPhaseTool from 'pages/custom-phase-tool'
import MainPanel from 'pages/main-panel'
import { setDisplayPhaseIndexCmd, useGetLocalisationCmd } from 'hooks/cmds'

const register: ModRegistrar = (moduleRegistry) => {
  moduleRegistry.append('GameTopLeft', () => <App />)
}

function App() {
  const displayPhaseIndex = useGetDisplayPhaseIndexCmd()
  let localisation = useGetLocalisationCmd()
  const cityConfiguration = useGetCityConfigurationCmd()

  return (
    <div id="traffic-light-manager-root">
      <CityConfigurationContext.Provider value={cityConfiguration}>
        <LocalisationContext.Provider value={localisation}>
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
        </LocalisationContext.Provider>
      </CityConfigurationContext.Provider>
    </div>
  )
}

export default register
