import { createContext } from 'react'
import { createClipboard } from '../hooks/clipboard'
import { CityConfiguration, EdgeGroupMask, SubLaneGroupMask } from 'types'

const DEFAULT_CITY_CONFIGURATION = {
  leftHandTraffic: false,
}

const CityConfigurationContext = createContext<CityConfiguration>(
  DEFAULT_CITY_CONFIGURATION,
)
const LocalisationContext = createContext({})
const EdgeGroupMaskContextClipboard = createClipboard<EdgeGroupMask>()
const SubLaneGroupMaskContextClipboard = createClipboard<SubLaneGroupMask>()
const CurrentFocusPhaseIndexContext = createContext<
  [number, (value: number) => void]
>([-1, () => {}])

export {
  CityConfigurationContext,
  DEFAULT_CITY_CONFIGURATION,
  LocalisationContext,
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
  CurrentFocusPhaseIndexContext,
}
