import { useState, useEffect } from 'react'
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';

const MessageBox = () => {
    const [messageList, setMessageList] = useState<string[]>([])

    useEffect(() => {
        window.electron?.onMessage('check', handleMessage);


        return () => {
            window.electron?.removeAllListeners('check');
            console.log("remove listeners");
        }
    }, [])

    const handleMessage = (_: unknown, data: unknown) => {
        const message = data as string[];
        setMessageList(pre => [...pre, message[0]])
    };


    return (
        <div>
            <List sx={{ width: '100%', maxWidth: 640, bgcolor: 'background.paper' }}>
                {messageList.map((value, index) => {
                    return <ListItem key={index}>
                        <ListItemText primary={value} />
                    </ListItem>
                })}
            </List>
        </div>
    )
}

export default MessageBox