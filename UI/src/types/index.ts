import { Entity } from 'cs2/utils'

export const enum EdgeGroupMaskOptions {
  PerLaneSignal = 1 << 0,
}

export enum UpdateCustomPhaseType {
  MinimumDuration = 0,
  MaximumDuration = 1,
  TargetDurationMultiplier = 2,
  LaneOccupiedMultiplier = 3,
  IntervalFactor = 4,
  PrioritiseTrack = 5,
  PrioritisePublicCar = 6,
  PrioritisePedestrian = 7,
  LinkedWithNextPhase = 8,
  EndPhasePrematurely = 9,
  Name = 10,
  BindWithTemplate = 11,
  ApplyTemplate = 12,
}

export const enum ToolState {
  Disabled = 0,
  ChooseGroup = 1,
  Choosed = 2,
  AddTrafficLights = 3,
  RemoveTrafficLights = 4,
  Editing = 5,
}

export interface Position {
  x: number
  y: number
  z: number
}

export interface ScreenPoint {
  top: number
  left: number
}

export interface Camera {
  position: Position
  rotation: Position
}

export interface CityConfiguration {
  leftHandTraffic: boolean
}

export interface ToolTooltipMessage {
  image: string
  message: string
}

export interface TrafficLightMember {
  entityIndex: number
  position: Position | null
}

export interface TrafficLightGroupName {
  name: string
  worldPosition: Position
}

export interface TrafficLightGroup {
  timer: number
  currentPhaseIndex: number
  manualPhaseIndex: number
  targetDuration: number
  passedCarCount: number
}

export interface CustomPhaseItem {
  turnsSinceLastRun: number
  lowFlowTimer: number
  carLaneOccupied: number
  publicCarLaneOccupied: number
  trackLaneOccupied: number
  pedestrianLaneOccupied: number
  weightedWaiting: number
  targetDuration: number
  priority: number
  minimumDuration: number
  maximumDuration: number
  targetDurationMultiplier: number
  laneOccupiedMultiplier: number
  intervalFactor: number
  prioritiseTrack: boolean
  prioritisePublicCar: boolean
  prioritisePedestrian: boolean
  linkedWithNextPhase: boolean
  endPhasePrematurely: boolean
  bindWithTemplate: boolean
  name: string
  bindTemplate: string
}

export interface CustomPhaseTemplate {
  m_Name: string
  m_IsPrioritiseTrack: boolean
  m_IsPrioritisePublicCar: boolean
  m_IsPrioritisePedestrian: boolean
  m_MinimumDuration: number
  m_MaximumDuration: number
  m_TargetDurationMultiplier: number
  m_LaneOccupiedMultiplier: number
  m_IntervalFactor: number
}

export interface Settings {
  customPhaseTemplates: CustomPhaseTemplate[]
  defaultCustomPhaseTemplate: CustomPhaseTemplate
}

export type CustomPhaseLaneType =
  | 'carLane'
  | 'publicCarLane'
  | 'trackLane'
  | 'pedestrianLaneStopLine'
  | 'pedestrianLaneNonStopLine'

export type CustomPhaseLaneDirection =
  'left' | 'straight' | 'right' | 'uTurn' | 'all'

export type CustomPhaseSignalState = 'stop' | 'go' | 'none'

export interface CustomPhaseLane {
  type: CustomPhaseLaneType
  left: CustomPhaseSignalState
  straight: CustomPhaseSignalState
  right: CustomPhaseSignalState
  uTurn: CustomPhaseSignalState
  all: CustomPhaseSignalState
}

export interface GroupMaskSignal {
  m_GoGroupMask: number
}

export interface GroupMaskTurn {
  m_Left: GroupMaskSignal
  m_Straight: GroupMaskSignal
  m_Right: GroupMaskSignal
  m_UTurn: GroupMaskSignal
}

export interface EdgeGroupMask {
  m_Edge: Entity
  m_Position: Position
  m_Options: number
  m_Car: GroupMaskTurn
  m_PublicCar: GroupMaskTurn
  m_Track: GroupMaskTurn
  m_PedestrianStopLine: GroupMaskSignal
  m_PedestrianNonStopLine: GroupMaskSignal
}

export interface SubLaneGroupMask {
  m_SubLane: Entity
  m_Position: Position
  m_Options: number
  m_Car: GroupMaskTurn
  m_Track: GroupMaskTurn
  m_Pedestrian: GroupMaskSignal
}

export interface SubLaneInfo {
  m_SubLane: Entity
  m_Position: Position
  m_CarLaneLeftCount: number
  m_CarLaneStraightCount: number
  m_CarLaneRightCount: number
  m_CarLaneUTurnCount: number
  m_TrackLaneLeftCount: number
  m_TrackLaneStraightCount: number
  m_TrackLaneRightCount: number
  m_PedestrianLaneCount: number
  m_SubLaneGroupMask: SubLaneGroupMask
}

export interface EdgeInfo {
  m_Edge: Entity
  m_Position: Position
  m_CarLaneLeftCount: number
  m_CarLaneStraightCount: number
  m_CarLaneRightCount: number
  m_CarLaneUTurnCount: number
  m_PublicCarLaneLeftCount: number
  m_PublicCarLaneStraightCount: number
  m_PublicCarLaneRightCount: number
  m_PublicCarLaneUTurnCount: number
  m_TrackLaneLeftCount: number
  m_TrackLaneStraightCount: number
  m_TrackLaneRightCount: number
  m_TrainTrackCount: number
  m_PedestrianLaneStopLineCount: number
  m_PedestrianLaneNonStopLineCount: number
  m_SubLaneInfoList: SubLaneInfo[]
  m_EdgeGroupMask: EdgeGroupMask
  // 所属路口实体
  m_TrafficLightsEntity: Entity
}

export type BindTemplateInput =
  | { bindWithTemplate: true; templateName: string }
  | { bindWithTemplate: false }

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
  | { key: UpdateCustomPhaseType.BindWithTemplate; value: BindTemplateInput }
  | { key: UpdateCustomPhaseType.ApplyTemplate; value: string }
)

export interface SwapCustomPhaseInput {
  index1: number
  index2: number
}

export interface UpdateGroupMaskInput<T> {
  groupMaskArray: T[]
  entity: Entity
}

export interface LookAtInput {
  position: Position
  distance: number
}

export interface UpdateStorageInput {
  key: string
  value: string
}