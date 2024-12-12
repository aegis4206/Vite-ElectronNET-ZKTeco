declare global {
  interface Window {
    electron: {
      send: (channel: string, data: unknown) => void;
      onMessage: (channel: string, callback: (event: unknown, data: unknown) => void) => void;
      removeAllListeners: (channel: string) => void;
    };
  }
}

export { };