import { createContext, ProviderProps, useState } from "react";

export type ClipboardContext<T> = {
    data: T | undefined;
    setData: (data: T | undefined) => void;
}

export function createClipboard<T>() {
    const context = createContext<{
        data: T | undefined;
        setData: (data: T | undefined) => void;
    }>({
        data: undefined,
        setData: () => {}
    });

    return {
        context,
        Provider : (props: Omit<ProviderProps<ClipboardContext<T>>, "value">) => {
            const [data, setData] = useState<T | undefined>(undefined);
            return (<context.Provider value={{data, setData}} {...props}></context.Provider>)
        }
    };
}