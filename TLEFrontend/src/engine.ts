import { useEffect, useState } from 'react';

export function useEngineOn(event: string, initialState: string) {
  const [result, setResult] = useState(initialState);

  useEffect(() => {
    if ("engine" in window && isEngine(window.engine)) {
      const listener = window.engine.on(event, setResult);

      return () => {
        listener.clear();
      };
    }
  }, [event]);

  return result;
}

export async function useEngineCall(event: string, data?: string) {
  return await engineCall(event, data);
}

export async function translatePosition(worldPosition: WorldPosition) {
  const result = await engineCall("C2VM.TLE.CallTranslatePosition", JSON.stringify(worldPosition));
  const screenPoint = {left: 0, top: 0};
  if (result) {
    const parsedObj = JSON.parse(result);
    if (parsedObj.left) {
      screenPoint.left = parsedObj.left;
    }
    if (parsedObj.top) {
      screenPoint.top = parsedObj.top;
    }
  }
  return screenPoint;
}

export async function engineCall(event: string, data?: string) {
  if ("engine" in window && isEngine(window.engine)) {
    if (data) {
      return await window.engine.call(event, data);
    } else {
      return await window.engine.call(event);
    }
  }
}

interface Engine {
  call: (event: string, data?: string) => Promise<string>,
  on: (event: string, callback: (result: string) => void) => ({clear: () => void})
}

export function isEngine(engine: unknown): engine is Engine {
  return typeof engine === "object";
}