import {
  createContext,
  createElement,
  ProviderProps,
  useCallback,
  useMemo,
  useState,
} from "react";

export interface ClipboardContext<T> {
  history: T[];
  selectedIndex: number;
  save: (t: T) => void;
  deleteAt: (index: number) => void;
  select: (index: number) => void;
}

export function createClipboard<T>() {
  const context = createContext<ClipboardContext<T>>({
    history: [],
    selectedIndex: -1,
    save: () => {},
    deleteAt: () => {},
    select: () => {},
  });

  return {
    context,
    Provider: (props: Omit<ProviderProps<ClipboardContext<T>>, "value">) => {
      const [history, setHistory] = useState<T[]>([]);
      const [index, setIndex] = useState<number>(-1);

      const save = useCallback(
        (t: T) => {
          if (t) {
            setHistory((array) => {
              if (array.length >= 10) {
                array = array.slice(0, 9);
              }
              array = [t, ...array];
              return array;
            });
            setIndex(0);
          }
        },
        [setHistory, setIndex],
      );
      const removeAt = useCallback(
        (index: number) => {
          setHistory((array) => array.filter((_, i) => i !== index));
        },
        [setHistory],
      );
      const selectedIndex = useMemo(() => {
        if (history.length === 0) {
          return -1;
        }
        if (index < 0 || index >= history.length) {
          return 0;
        }
        return index;
      }, [index, history.length]);
      const select = useCallback(
        (index: number) => {
          setIndex(index);
        },
        [setIndex],
      );

      return createElement(context.Provider, {
        value: { history, selectedIndex, save, deleteAt: removeAt, select },
        ...props,
      });
    },
  };
}
