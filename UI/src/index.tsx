import 'assets/styles/index.scss'
import { ModRegistrar } from 'cs2/modding'
import {
  CityConfigurationContext,
  LocalisationContext,
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
  CurrentFocusPhaseIndexContext,
} from 'context'
import { useGetCityConfigurationCmd } from 'hooks/cmds'
import ClipboardPanel from 'pages/clipboard-panel'
import CustomPhaseTool from 'pages/custom-phase-tool'
import MainPanel from 'pages/main-panel'
import { useEffect, useState } from 'react'
import { setDisplayPhaseIndexCmd, useGetLocalisationCmd } from 'hooks/cmds'

const register: ModRegistrar = (moduleRegistry) => {
  moduleRegistry.append('GameTopLeft', () => <App />)
}

function App() {
  const [currentPhaseIndex, setCurrentPhaseIndex] = useState(-1)

  useEffect(() => {
    setDisplayPhaseIndexCmd(currentPhaseIndex)
  }, [currentPhaseIndex])

  let localisation = useGetLocalisationCmd()
  const cityConfiguration = useGetCityConfigurationCmd()

  return (
    <CityConfigurationContext.Provider value={cityConfiguration}>
      <LocalisationContext.Provider value={localisation}>
        <EdgeGroupMaskContextClipboard.Provider>
          <SubLaneGroupMaskContextClipboard.Provider>
            <CurrentFocusPhaseIndexContext.Provider
              value={[currentPhaseIndex, setCurrentPhaseIndex]}
            >
              <MainPanel />
              <CustomPhaseTool />
              <ClipboardPanel />
            </CurrentFocusPhaseIndexContext.Provider>
          </SubLaneGroupMaskContextClipboard.Provider>
        </EdgeGroupMaskContextClipboard.Provider>
      </LocalisationContext.Provider>
    </CityConfigurationContext.Provider>
  )
}

export default register
