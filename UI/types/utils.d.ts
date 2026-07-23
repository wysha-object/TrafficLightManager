declare module 'cs2/utils' {
  export type EqualityComparer<T> = (a: T, b: T) => boolean
  export interface Entity {
    index: number
    version: number
  }
  export function entityKey({ index, version }: Entity): string
  export function parseEntityKey(value: any): Entity | undefined
  export function entityEquals(
    a: Entity | null | undefined,
    b: Entity | null | undefined,
  ): boolean
  export function isNullOrEmpty(s: string | null | undefined): boolean
  /**
   * Performs equality by iterating through keys on an object and returning false
   * when any key has values which are not strictly equal between the arguments.
   * Returns true when the values of all keys are strictly equal.
   */
  export function shallowEqual(a: any, b: any, depth?: number): boolean
  export function useMemoizedValue<T>(
    value: T,
    equalityComparer: EqualityComparer<T>,
  ): T
  export function formatLargeNumber(value: number): string
  export function useFormattedLargeNumber(value: number): string
  export function useRem(): number
  export function useCssLength(length: string): number

  export {}
}
