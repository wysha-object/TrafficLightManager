interface Settings {
  customPhaseTemplates: CustomPhaseTemplate[],
  defaultCustomPhaseTemplate: CustomPhaseTemplate
}

interface CustomPhaseItem {
  turnsSinceLastRun: number,
  lowFlowTimer: number,
  carFlow: number,
  carLaneOccupied: number,
  publicCarLaneOccupied: number,
  trackLaneOccupied: number,
  pedestrianLaneOccupied: number,
  weightedWaiting: number,
  targetDuration: number,
  priority: number,
  minimumDuration: number,
  maximumDuration: number,
  targetDurationMultiplier: number,
  laneOccupiedMultiplier: number,
  intervalExponent: number,
  prioritiseTrack: boolean,
  prioritisePublicCar: boolean,
  prioritisePedestrian: boolean,
  linkedWithNextPhase: boolean,
  endPhasePrematurely: boolean,
  bindWithTemplate: boolean,
  name: string,
  bindTemplate: string
}

interface TrafficLightGroup {
  timer: number,
  currentPhaseIndex: number,
  manualPhaseIndex: number,
  targetDuration: number
}

interface WorldPosition {
  x: number,
  y: number,
  z: number,
  key: string
}

interface ScreenPoint {
  left: number,
  top: number
}

interface ScreenPointMap {
  [key: string]: ScreenPoint
}

interface CityConfiguration {
  leftHandTraffic: boolean
}

interface CustomPhaseLane {
  type: CustomPhaseLaneType,
  left: CustomPhaseSignalState,
  straight: CustomPhaseSignalState,
  right: CustomPhaseSignalState,
  uTurn: CustomPhaseSignalState,
  all: CustomPhaseSignalState
}

type CustomPhaseLaneType = "carLane" | "publicCarLane" | "trackLane" | "pedestrianLaneStopLine" | "pedestrianLaneNonStopLine";

type CustomPhaseLaneDirection = "left" | "straight" | "right" | "uTurn" | "all";

type CustomPhaseSignalState = "stop" | "go" | "yield" | "none";

interface GroupMaskSignal {
  m_GoGroupMask: number,
  m_YieldGroupMask: number
}

interface GroupMaskTurn {
  m_Left: GroupMaskSignal,
  m_Straight: GroupMaskSignal,
  m_Right: GroupMaskSignal,
  m_UTurn: GroupMaskSignal
}

interface EdgeGroupMask {
  m_Edge: Entity,
  m_Position: WorldPosition,
  m_Options: number,
  m_Car: GroupMaskTurn,
  m_PublicCar: GroupMaskTurn,
  m_Track: GroupMaskTurn,
  m_PedestrianStopLine: GroupMaskSignal,
  m_PedestrianNonStopLine: GroupMaskSignal
}

interface EdgeInfo {
  m_Edge: Entity,
  m_Position: WorldPosition,
  m_CarLaneLeftCount: number,
  m_CarLaneStraightCount: number,
  m_CarLaneRightCount: number,
  m_CarLaneUTurnCount: number,
  m_PublicCarLaneLeftCount: number,
  m_PublicCarLaneStraightCount: number,
  m_PublicCarLaneRightCount: number,
  m_PublicCarLaneUTurnCount: number,
  m_TrackLaneLeftCount: number,
  m_TrackLaneStraightCount: number,
  m_TrackLaneRightCount: number,
  m_TrainTrackCount: number,
  m_PedestrianLaneStopLineCount: number,
  m_PedestrianLaneNonStopLineCount: number,
  m_SubLaneInfoList: SubLaneInfo[],
  m_EdgeGroupMask: EdgeGroupMask
  // 所属路口实体
  m_TrafficLightsEntity: Entity,
}

interface SubLaneGroupMask {
  m_SubLane: Entity,
  m_Position: WorldPosition,
  m_Options: number,
  m_Car: GroupMaskTurn,
  m_Track: GroupMaskTurn,
  m_Pedestrian: GroupMaskSignal
}

interface SubLaneInfo {
  m_SubLane: Entity,
  m_Position: WorldPosition,
  m_CarLaneLeftCount: number,
  m_CarLaneStraightCount: number,
  m_CarLaneRightCount: number,
  m_CarLaneUTurnCount: number,
  m_TrackLaneLeftCount: number,
  m_TrackLaneStraightCount: number,
  m_TrackLaneRightCount: number,
  m_PedestrianLaneCount: number,
  m_SubLaneGroupMask: SubLaneGroupMask
}

interface ToolTooltipMessage {
  image: string,
  message: string
}

interface CustomPhaseTemplate {
  m_Name: string,

  m_IsPrioritiseTrack: boolean,
  m_IsPrioritisePublicCar: boolean,
  m_IsPrioritisePedestrian: boolean,

  m_MinimumDuration: number,

  m_MaximumDuration: number,

  m_TargetDurationMultiplier: number,

  m_LaneOccupiedMultiplier: number,

  m_IntervalExponent: number,
}

interface TrafficLightGroupName {
  name: string,
  worldPosition: WorldPosition
}