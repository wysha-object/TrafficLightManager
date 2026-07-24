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
const EdgeGroupMaskContextClipboard = createClipboard<EdgeGroupMask>('EdgeGroupMaskClipboard')
const SubLaneGroupMaskContextClipboard = createClipboard<SubLaneGroupMask>('SubLaneGroupMaskClipboard')
const CurrentFocusPhaseIndexContext = createContext<
  [number, (value: number) => Promise<void>]
>([-1, async () => {}])

export {
  CityConfigurationContext,
  DEFAULT_CITY_CONFIGURATION,
  LocalisationContext,
  EdgeGroupMaskContextClipboard,
  SubLaneGroupMaskContextClipboard,
  CurrentFocusPhaseIndexContext,
}
