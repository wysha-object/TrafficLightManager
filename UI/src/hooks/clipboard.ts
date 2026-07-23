import {
  createContext,
  createElement,
  ProviderProps,
  useCallback,
  useMemo,
  useState,
} from 'react'

export interface ClipboardContext<T> {
  history: ClipboardHistoryItem<T>[]
  selectedIndex: number
  save: (t: T, name?: string) => void
  updateAt: (index: number, name: string) => void
  deleteAt: (index: number) => void
  selectAt: (index: number) => void
}

export interface ClipboardHistoryItem<T> {
  value: T
  name?: string
}

export function createClipboard<T>() {
  const context = createContext<ClipboardContext<T>>({
    history: [],
    selectedIndex: -1,
    save: () => {},
    updateAt: () => {},
    deleteAt: () => {},
    selectAt: () => {},
  })

  return {
    context,
    Provider: (props: Omit<ProviderProps<ClipboardContext<T>>, 'value'>) => {
      const [history, setHistory] = useState<ClipboardHistoryItem<T>[]>([])
      const [index, setIndex] = useState<number>(-1)

      const save = useCallback(
        (t: T, name?: string) => {
          if (t) {
            setHistory((value) => {
              if (value.length >= 10) {
                value = value.slice(0, 9)
              }
              value = [{ value: t, name: name }, ...value]
              return value
            })
            setIndex(0)
          }
        },
        [setHistory, setIndex],
      )
      const updateAt = useCallback(
        (index: number, name: string) => {
          setHistory((value) =>
            value.map((item, i) => (i === index ? { ...item, name } : item)),
          )
        },
        [setHistory],
      )
      const deleteAt = useCallback(
        (index: number) => {
          setHistory((value) => value.filter((_, i) => i !== index))
        },
        [setHistory],
      )
      const selectedIndex = useMemo(() => {
        if (history.length === 0) {
          return -1
        }
        if (index < 0 || index >= history.length) {
          return 0
        }
        return index
      }, [index, history.length])
      const selectAt = useCallback(
        (index: number) => {
          setIndex(index)
        },
        [setIndex],
      )

      return createElement(context.Provider, {
        value: { history, selectedIndex, save, updateAt, deleteAt, selectAt },
        ...props,
      })
    },
  }
}
