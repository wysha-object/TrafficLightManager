export const enum EdgeGroupMaskOptions {
  PerLaneSignal = 1 << 0,
}

export const enum ToolState {
  Disabled = 0,
  ChooseGroup = 1,
  Choosed = 2,
  AddTrafficLights = 3,
  RemoveTrafficLights = 4,
  Editing = 5,
}