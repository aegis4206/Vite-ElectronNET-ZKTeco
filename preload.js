const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electron', {
  send: (channel, data) => ipcRenderer.send(channel, data),
  onMessage: (channel, callback) => ipcRenderer.on(channel, (event, args) => callback(event, args)),
  removeAllListeners: (channel) => ipcRenderer.removeAllListeners(channel),
});

console.log('Preload script loaded');
