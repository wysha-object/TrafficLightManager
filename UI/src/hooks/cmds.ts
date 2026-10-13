import { bindValue, call, useValue } from 'cs2/api'
import { Entity } from 'cs2/utils'
import {
  CityConfiguration,
  CustomPhaseItem,
  ToolState,
  EdgeInfo,
  TrafficLightGroup,
  Settings,
  CustomPhaseTemplate,
  EdgeGroupMask,
  SubLaneGroupMask,
  UpdateCustomPhaseType,
} from 'types'

type Position = { x: number; y: number; z: number }
type Camera = { position: Position; rotation: Position }
type TrafficLightMember = { entityIndex: number; position: Position | null }

const GetToolState = bindValue<ToolState>(
  'TrafficLightManager',
  'GetToolState',
  ToolState.Disabled,
)
const GetDisplayPhaseIndex = bindValue<number>(
  'TrafficLightManager',
  'GetDisplayPhaseIndex',
  -1,
)
const GetCityConfiguration = bindValue<CityConfiguration>(
  'TrafficLightManager',
  'GetCityConfiguration',
  {
    leftHandTraffic: false,
  },
)
const GetCamera = bindValue<Camera>('TrafficLightManager', 'GetCamera', {
  position: { x: 0, y: 0, z: 0 },
  rotation: { x: 0, y: 0, z: 0 },
})
const GetEdgeInfo = bindValue<EdgeInfo[]>(
  'TrafficLightManager',
  'GetEdgeInfo',
  [],
)
const GetCustomPhaseItems = bindValue<CustomPhaseItem[]>(
  'TrafficLightManager',
  'GetCustomPhaseItems',
  [],
)
const GetTrafficLightGroup = bindValue<TrafficLightGroup>(
  'TrafficLightManager',
  'GetTrafficLightGroup',
  {
    timer: 0,
    currentPhaseIndex: 0,
    manualPhaseIndex: 0,
    targetDuration: 0,
    passedCarCount: 0,
  },
)
const GetTrafficLightsMembers = bindValue<TrafficLightMember[]>(
  'TrafficLightManager',
  'GetTrafficLightsMembers',
  [],
)
const defaultTemplate: CustomPhaseTemplate = {
  m_Name: 'Default',
  m_IsPrioritiseTrack: true,
  m_IsPrioritisePublicCar: false,
  m_IsPrioritisePedestrian: false,
  m_MinimumDuration: 2,
  m_MaximumDuration: 300,
  m_TargetDurationMultiplier: 1,
  m_LaneOccupiedMultiplier: 1,
  m_IntervalFactor: 1,
}
const GetSettings = bindValue<Settings>('TrafficLightManager', 'GetSettings', {
  customPhaseTemplates: [],
  defaultCustomPhaseTemplate: defaultTemplate,
})
const GetTrafficLightGroupName = bindValue<string>(
  'TrafficLightManager',
  'GetTrafficLightManagerGroupName',
  '',
)
const GetSystemDefaultTemplate = bindValue<CustomPhaseTemplate>(
  'TrafficLightManager',
  'GetSystemDefaultTemplate',
  defaultTemplate,
)

export function useGetToolStateCmd() {
  return useValue(GetToolState)
}
export function useGetDisplayPhaseIndexCmd() {
  return useValue(GetDisplayPhaseIndex)
}
export function useGetCityConfigurationCmd() {
  return useValue(GetCityConfiguration)
}
export function useGetCameraCmd() {
  return useValue(GetCamera)
}
export function useGetEdgeInfoCmd() {
  return useValue(GetEdgeInfo)
}
export function useGetCustomPhaseItemsCmd() {
  return useValue(GetCustomPhaseItems)
}
export function useGetTrafficLightGroupCmd() {
  return useValue(GetTrafficLightGroup)
}
export function useGetTrafficLightsMembersCmd() {
  return useValue(GetTrafficLightsMembers)
}
export function useGetSettingsCmd() {
  return useValue(GetSettings)
}
export function useGetTrafficLightGroupNameCmd() {
  return useValue(GetTrafficLightGroupName)
}
export function useGetSystemDefaultTemplateCmd() {
  return useValue(GetSystemDefaultTemplate)
}

export async function setToolStateCmd(inputValue: ToolState): Promise<void> {
  return await call('TrafficLightManager', 'SetToolState', inputValue)
}
export async function setDisplayPhaseIndexCmd(
  inputValue: number,
): Promise<void> {
  return await call('TrafficLightManager', 'SetDisplayPhaseIndex', inputValue)
}
export async function setManualPhaseIndexCmd(
  inputValue: number,
): Promise<void> {
  return await call('TrafficLightManager', 'SetManualPhaseIndex', inputValue)
}
export type UpdateCustomPhaseInput = { index: number } & (
  | { key: UpdateCustomPhaseType.MinimumDuration; value: number }
  | { key: UpdateCustomPhaseType.MaximumDuration; value: number }
  | { key: UpdateCustomPhaseType.TargetDurationMultiplier; value: number }
  | { key: UpdateCustomPhaseType.LaneOccupiedMultiplier; value: number }
  | { key: UpdateCustomPhaseType.IntervalFactor; value: number }
  | { key: UpdateCustomPhaseType.PrioritiseTrack; value: boolean }
  | { key: UpdateCustomPhaseType.PrioritisePublicCar; value: boolean }
  | { key: UpdateCustomPhaseType.PrioritisePedestrian; value: boolean }
  | { key: UpdateCustomPhaseType.LinkedWithNextPhase; value: boolean }
  | { key: UpdateCustomPhaseType.EndPhasePrematurely }
  | { key: UpdateCustomPhaseType.Name; value: string }
  | {
      key: UpdateCustomPhaseType.BindWithTemplate
      value:
        | {
            bindWithTemplate: true
            templateName: string
          }
        | {
            bindWithTemplate: false
          }
    }
  | { key: UpdateCustomPhaseType.ApplyTemplate; value: string }
)

export async function setCustomPhaseDataCmd(
  inputValue: UpdateCustomPhaseInput,
): Promise<void> {
  return await call('TrafficLightManager', 'SetCustomPhaseData', inputValue)
}
export async function setTemplateCmd(
  inputValue: CustomPhaseTemplate,
): Promise<void> {
  return await call('TrafficLightManager', 'SetTemplate', inputValue)
}
export async function removeTemplateCmd(inputValue: string): Promise<void> {
  return await call('TrafficLightManager', 'RemoveTemplate', inputValue)
}
export async function setTrafficLightGroupNameCmd(
  inputValue: string,
): Promise<void> {
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
export async function swapCustomPhaseCmd(
  index1: number,
  index2: number,
): Promise<void> {
  return await call('TrafficLightManager', 'SwapCustomPhase', {
    index1,
    index2,
  })
}
export async function updateEdgeGroupMaskCmd(
  groupMaskArray: EdgeGroupMask[],
  entity: Entity,
): Promise<void> {
  return await call('TrafficLightManager', 'UpdateEdgeGroupMask', {
    groupMaskArray,
    entity,
  })
}
export async function updateSubLaneGroupMaskCmd(
  groupMaskArray: SubLaneGroupMask[],
  entity: Entity,
): Promise<void> {
  return await call('TrafficLightManager', 'UpdateSubLaneGroupMask', {
    groupMaskArray,
    entity,
  })
}
export async function worldToScreenPoint(inputValue: {
  x: number
  y: number
  z: number
}): Promise<{ top: number; left: number }> {
  return await call('TrafficLightManager', 'WorldToScreenPoint', inputValue)
}
export async function lookAt(
  x: number,
  y: number,
  z: number,
  distance: number,
): Promise<void> {
  return await call('TrafficLightManager', 'LookAt', {
    position: { x, y, z },
    distance,
  })
}
export async function copyPhaseCmd(index: number): Promise<number> {
  return (await call('TrafficLightManager', 'CopyPhase', index)) as number
}
export async function getStorage(key: string): Promise<object | null> {
  const jsonString = (await call(
    'TrafficLightManager',
    'GetStorage',
    key,
  )) as string
  if (!jsonString) {
    return null
  }
  return JSON.parse(jsonString) as object
}
export async function updateStorage(key: string, value: object): Promise<void> {
  return await call('TrafficLightManager', 'UpdateStorage', {
    key,
    value: JSON.stringify(value),
  })
}
