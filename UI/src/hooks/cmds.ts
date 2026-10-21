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
  UpdateCustomPhaseInput,
  SwapCustomPhaseInput,
  UpdateGroupMaskInput,
  LookAtInput,
  UpdateStorageInput,
  Position,
  ScreenPoint,
  Camera,
  TrafficLightMember,
} from 'types'

export type { UpdateCustomPhaseInput } from 'types'

const defaultTemplate: CustomPhaseTemplate = {
  m_Name: 'Default',
  m_IsPrioritiseTrack: true,
  m_IsPrioritisePublicCar: false,
  m_IsPrioritisePedestrian: false,
  m_MinimumDuration: 2,
  m_MaximumDuration: 300,
  m_TargetDurationMultiplier: 1,
  m_CarLaneOccupiedMultiplier: 1,
  m_TrackLaneOccupiedMultiplier: 1,
  m_PedestrianLaneOccupiedMultiplier: 1,
  m_IntervalFactor: 1,
}

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
  const inputValue: SwapCustomPhaseInput = { index1, index2 }
  return await call('TrafficLightManager', 'SwapCustomPhase', inputValue)
}
export async function updateEdgeGroupMaskCmd(
  groupMaskArray: EdgeGroupMask[],
  entity: Entity,
): Promise<void> {
  const inputValue: UpdateGroupMaskInput<EdgeGroupMask> = {
    groupMaskArray,
    entity,
  }
  return await call('TrafficLightManager', 'UpdateEdgeGroupMask', inputValue)
}
export async function updateSubLaneGroupMaskCmd(
  groupMaskArray: SubLaneGroupMask[],
  entity: Entity,
): Promise<void> {
  const inputValue: UpdateGroupMaskInput<SubLaneGroupMask> = {
    groupMaskArray,
    entity,
  }
  return await call('TrafficLightManager', 'UpdateSubLaneGroupMask', inputValue)
}
export async function worldToScreenPoint(
  inputValue: Position,
): Promise<ScreenPoint> {
  return await call('TrafficLightManager', 'WorldToScreenPoint', inputValue)
}
export async function lookAt(
  x: number,
  y: number,
  z: number,
  distance: number,
): Promise<void> {
  const inputValue: LookAtInput = {
    position: { x, y, z },
    distance,
  }
  return await call('TrafficLightManager', 'LookAt', inputValue)
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
  const inputValue: UpdateStorageInput = {
    key,
    value: JSON.stringify(value),
  }
  return await call('TrafficLightManager', 'UpdateStorage', inputValue)
}
