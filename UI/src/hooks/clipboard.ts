import {
  createContext,
  createElement,
  ProviderProps,
  useCallback,
  useEffect,
  useState,
} from 'react'
import { getStorage, updateStorage } from './cmds'

export interface Clipboard<T> {
  history: ClipboardHistoryItem<T>[]
  selectedIndex: number
}

export interface ClipboardHistoryItem<T> {
  value: T
  name: string
}

export interface ClipboardContext<T> extends Clipboard<T> {
  save: (t: T, name?: string) => void
  updateAt: (index: number, value: Partial<ClipboardHistoryItem<T>>) => void
  deleteAt: (index: number) => void
  selectAt: (index: number) => void
}

export function createClipboard<T>(key: string) {
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
      const [clipboard, setClipboard] = useState<Clipboard<T>>({
        history: [],
        selectedIndex: -1,
      })
      const [initialized, setInitialized] = useState(false)

      const save = useCallback(
        (t: T, name?: string) => {
          if (t) {
            setClipboard((prev) => {
              let newHistory = prev.history
              if (newHistory.length >= 10) {
                newHistory = newHistory.slice(0, 9)
              }
              let nameValue = name
              if (!nameValue) {
                const date = new Date()
                nameValue = `Created on ${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')}:${date.getSeconds().toString().padStart(2, '0')}`
              }
              newHistory = [{ value: t, name: nameValue }, ...newHistory]
              return { history: newHistory, selectedIndex: 0 }
            })
          }
        },
        [setClipboard],
      )
      const updateAt = useCallback(
        (index: number, value: Partial<ClipboardHistoryItem<T>>) => {
          setClipboard((prev) => ({
            ...prev,
            history: prev.history.map((item, i) => (i === index ? { ...item, ...value } : item)),
          }))
        },
        [setClipboard],
      )
      const deleteAt = useCallback(
        (index: number) => {
          setClipboard((prev) => {
            const newValue = {
              ...prev,
              history: prev.history.filter((_, i) => i !== index),
            }
            if (newValue.selectedIndex === index) {
              newValue.selectedIndex = -1
            } else if (newValue.selectedIndex > index) {
              newValue.selectedIndex -= 1
            }
            return newValue
          })
        },
        [setClipboard],
      )
      const selectAt = useCallback(
        (index: number) => {
          setClipboard((prev) => ({
            ...prev,
            selectedIndex: index,
          }))
        },
        [setClipboard],
      )

      const loadClipboard = async () => {
        const stored = await getStorage(key) as Clipboard<T>
        if (!stored) {
          setClipboard({ history: [], selectedIndex: -1 })
        } else {
          setClipboard(stored)
        }
      }
      useEffect(() => {
        loadClipboard().then(() => setInitialized(true))
      }, [])
      useEffect(() => {
        if (!initialized) return
        updateStorage(key, clipboard)
      }, [JSON.stringify(clipboard)])

      return createElement(context.Provider, {
        value: { ...clipboard, save, updateAt, deleteAt, selectAt },
        ...props,
      })
    },
  }
}
