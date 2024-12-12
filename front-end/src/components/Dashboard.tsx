import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import ListItemAvatar from '@mui/material/ListItemAvatar';
import PublicOffIcon from '@mui/icons-material/PublicOff';
import PublicIcon from '@mui/icons-material/Public';
import DeleteForeverIcon from '@mui/icons-material/DeleteForever';
import { Button, TextField, Snackbar, Alert, IconButton } from '@mui/material';
import { useState, useEffect } from 'react';


interface device {
  ip: string,
  connected: boolean,
}

export default function Dashboard() {
  const [deviceList, setDeviceList] = useState<device[]>([]);
  const [ipText, setIpText] = useState<string>('');
  const [snackbarOpen, setSnackbarOpen] = useState<boolean>(false);
  const isConnected = deviceList.some(device => device.connected)

  useEffect(() => {
    const list = localStorage.getItem("list")
    if (list != null) {
      const newList = JSON.parse(list).map((ip: string) => ({ ip, connected: false }))
      setDeviceList(newList)
    }
  }, [])

  const isValidIPv4Strict = (ip: string) => {
    const ipv4Regex = /^(([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])\.){3}([1-9]?\d|1\d\d|2[0-4]\d|25[0-5])$/;
    return ipv4Regex.test(ip);
  }

  const handleAddDevice = () => {
    if (isValidIPv4Strict(ipText)) {
      const newList = [...deviceList, {
        ip: ipText,
        connected: false
      }]
      setDeviceList(newList)
      localStorage.setItem("list", JSON.stringify(newList.map(device => device.ip)))
    } else {
      setSnackbarOpen(true)
    }
    setIpText('')
  }

  const handleDeleteDevice = (ip: string) => {
    const newList = deviceList.filter(device => device.ip != ip)
    setDeviceList(newList)
    localStorage.setItem("list", JSON.stringify(newList.map(device => device.ip)))
  }

  const handleConnectDevice = () => {
    console.log(deviceList);
    window.electron.send('connectZKTeco', deviceList)
  }
  const handleDisConnectDevice = () => {
    window.electron.send('disconnectZKTeco', deviceList)
    setDeviceList(preList => (preList.map(device => ({ ...device, connected: false }))))

  }

  useEffect(() => {
    window.electron?.onMessage('connectZKTeco', handleConnectMessage);
    return () => {
      window.electron?.removeAllListeners('connectZKTeco');
    }
  }, [])

  const handleConnectMessage = (_: unknown, data: unknown) => {
    const message = data as string[];
    const ip = message[0];
    setDeviceList(preList => (preList.map(device => ({ ...device, connected: device.ip == ip ? true : false }))))
  };

  return (
    <div>
      <div>
        <List sx={{ width: '100%', maxWidth: 240, bgcolor: 'background.paper' }}>
          {deviceList.length == 0 ? "尚未新增IP" : deviceList.map(device => {
            return <ListItem key={device.ip}
              secondaryAction={
                <IconButton disabled={isConnected} onClick={() => handleDeleteDevice(device.ip)}>
                  <DeleteForeverIcon color='error' />
                </IconButton>
              }
            >
              <ListItemAvatar>
                {device.connected ? <PublicIcon color='success' /> : <PublicOffIcon />}
              </ListItemAvatar>
              <ListItemText primary={device.ip} />
            </ListItem>
          })}
        </List>
      </div>
      <br></br>
      <div>
        <TextField
          value={ipText}
          onChange={e => setIpText(e.target.value)}
          type='text'
          color='warning'
          label='IP'
          variant="outlined"
        ></TextField >
        <br></br>
        <br></br>
        <Button variant="outlined" disabled={isConnected} onClick={handleAddDevice} style={{ marginRight: "1em" }}>新增</Button>
        <Button variant="outlined" disabled={isConnected} onClick={handleConnectDevice} color='success' style={{ marginRight: "1em" }}>連線</Button>
        <Button variant="outlined" disabled={!isConnected} onClick={handleDisConnectDevice} color='error'>斷線</Button>
      </div>
      <Snackbar
        anchorOrigin={{ vertical: "top", horizontal: "center" }}
        open={snackbarOpen}
        onClose={() => setSnackbarOpen(false)}
        autoHideDuration={5000}
      >
        <Alert
          onClose={() => setSnackbarOpen(false)}
          severity="error"
          variant="filled"
          sx={{ width: '100%' }}
        >
          IP格式錯誤
        </Alert>
      </Snackbar>
    </div >
  );
}
