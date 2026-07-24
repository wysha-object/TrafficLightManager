import { DEFAULT_CITY_CONFIGURATION } from 'context'
import { bindValue, call, useValue } from 'cs2/api'
import { Entity } from 'cs2/utils'
import {
  CityConfiguration,
  CustomPhaseItem,
  ToolState,
  ScreenPointMap,
  EdgeInfo,
  TrafficLightGroup,
  Settings,
  CustomPhaseTemplate,
  EdgeGroupMask,
  SubLaneGroupMask,
  WorldPosition,
} from 'types'

export function useGetToolStateCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetToolState', '-1')),
  ) as ToolState
}
export function useGetDisplayPhaseIndexCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetDisplayPhaseIndex', '-1')),
  ) as number
}

export function useGetLocalisationCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetLocalisation', '{}')),
  ) as Record<string, string>
}

export function useGetCityConfigurationCmd() {
  return JSON.parse(
    useValue(
      bindValue(
        'TrafficLightManager',
        'GetCityConfiguration',
        JSON.stringify(DEFAULT_CITY_CONFIGURATION),
      ),
    ),
  ) as CityConfiguration
}
export function useGetScreenPointCmd() {
  return useValue(
    bindValue('TrafficLightManager', 'GetScreenPoint', {}),
  ) as ScreenPointMap
}
export function useGetEdgeInfoCmd() {
  return useValue(
    bindValue('TrafficLightManager', 'GetEdgeInfo', []),
  ) as EdgeInfo[]
}
export function useGetCustomPhaseItemsCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetCustomPhaseItems', '[]')),
  ) as CustomPhaseItem[]
}
export function useGetTrafficLightGroupCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetTrafficLightGroup', '{}')),
  ) as TrafficLightGroup
}
export function useGetTrafficLightsMembersCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetTrafficLightsMembers', '[]')),
  ) as any[]
}
export function useGetSettingsCmd() {
  return JSON.parse(
    useValue(bindValue('TrafficLightManager', 'GetSettings', '{}')),
  ) as Settings
}
export function useGetTrafficLightGroupNameCmd() {
  return useValue(
    bindValue('TrafficLightManager', 'GetTrafficLightManagerGroupName', ''),
  ) as string
}
export function useGetSystemDefaultTemplateCmd() {
  return JSON.parse(
    useValue(
      bindValue('TrafficLightManager', 'GetSystemDefaultTemplate', '{}'),
    ),
  ) as CustomPhaseTemplate
}

export async function setToolStateCmd(inputValue: ToolState): Promise<void> {
  return await call('TrafficLightManager', 'SetToolState', inputValue)
}
export async function setDisplayPhaseIndexCmd(inputValue: number): Promise<void> {
  return await call('TrafficLightManager', 'SetDisplayPhaseIndex', inputValue)
}
export async function setManualPhaseIndexCmd(inputValue: number): Promise<void> {
  return await call('TrafficLightManager', 'SetManualPhaseIndex', inputValue)
}
export async function setCustomPhaseDataCmd(
  inputValue: { index: number } & (
    | { key: 'MinimumDuration'; value: number }
    | { key: 'MaximumDuration'; value: number }
    | { key: 'TargetDurationMultiplier'; value: number }
    | { key: 'LaneOccupiedMultiplier'; value: number }
    | { key: 'IntervalExponent'; value: number }
    | { key: 'PrioritiseTrack'; value: boolean }
    | { key: 'PrioritisePublicCar'; value: boolean }
    | { key: 'PrioritisePedestrian'; value: boolean }
    | { key: 'LinkedWithNextPhase'; value: boolean }
    | { key: 'EndPhasePrematurely' }
    | { key: 'Name'; value: string }
    | {
        key: 'BindWithTemplate'
        value:
          | {
              bindWithTemplate: true
              templateName: string
            }
          | {
              bindWithTemplate: false
            }
      }
    | { key: 'ApplyTemplate'; value: string }
  ),
): Promise<void> {
  let value: any = inputValue
  if (inputValue.key === 'BindWithTemplate') {
    value = {
      ...inputValue,
      value: JSON.stringify(inputValue.value),
    }
  }
  return await call(
    'TrafficLightManager',
    'SetCustomPhaseData',
    JSON.stringify(value),
  )
}
export async function setTemplateCmd(inputValue: CustomPhaseTemplate): Promise<void> {
  return await call(
    'TrafficLightManager',
    'SetTemplate',
    JSON.stringify(inputValue),
  )
}
export async function removeTemplateCmd(inputValue: string): Promise<void> {
  return await call('TrafficLightManager', 'RemoveTemplate', inputValue)
}
export async function setTrafficLightGroupNameCmd(inputValue: string): Promise<void> {
  return await call(
    'TrafficLightManager',
    'SetTrafficLightGroupName',
    inputValue,
  )
}
export async function addCustomPhaseCmd(): Promise<void> {
  return await call('TrafficLightManager', 'AddCustomPhase', '')
}
export async function removeCustomPhaseCmd(inputValue: number): Promise<void> {
  return await call('TrafficLightManager', 'RemoveCustomPhase', inputValue)
}
export async function swapCustomPhaseCmd(index1: number, index2: number): Promise<void> {
  return await call(
    'TrafficLightManager',
    'SwapCustomPhase',
    JSON.stringify({ index1, index2 }),
  )
}
export async function updateEdgeGroupMaskCmd(
  groupMaskArray: EdgeGroupMask[],
  entity: Entity,
): Promise<void> {
  return await call(
    'TrafficLightManager',
    'UpdateEdgeGroupMask',
    JSON.stringify({ groupMaskArray, entity }),
  )
}
export async function updateSubLaneGroupMaskCmd(
  groupMaskArray: SubLaneGroupMask[],
  entity: Entity,
): Promise<void> {
  return await call(
    'TrafficLightManager',
    'UpdateSubLaneGroupMask',
    JSON.stringify({ groupMaskArray, entity }),
  )
}
export async function addWorldPosition(inputValue: WorldPosition[]): Promise<void> {
  return await call(
    'TrafficLightManager',
    'AddWorldPosition',
    JSON.stringify(inputValue),
  )
}
export async function removeWorldPosition(inputValue: WorldPosition[]): Promise<void> {
  return await call(
    'TrafficLightManager',
    'RemoveWorldPosition',
    JSON.stringify(inputValue),
  )
}
export async function lookAt(
  x: number,
  y: number,
  z: number,
  distance: number,
): Promise<void> {
  return await call(
    'TrafficLightManager',
    'LookAt',
    JSON.stringify({ x, y, z, distance }),
  )
}
export async function copyPhaseCmd(index: number): Promise<number> {
  return (await call('TrafficLightManager', 'CopyPhase', index)) as number
}
export async function getStorage(key: string): Promise<object | null> {
  const jsonString = await call('TrafficLightManager', 'GetStorage', key) as string
  if (!jsonString) {
    return null
  }
  return JSON.parse(jsonString) as object
}
export async function updateStorage(key: string, value: object): Promise<void> {
  return await call(
    'TrafficLightManager',
    'UpdateStorage',
    JSON.stringify({ key, value:  JSON.stringify(value) }),
  )
}
