import './App.css'
import Dashboard from './components/Dashboard'
import MessageBox from './components/MessageBox'

function App() {

  return (
    <>
      <div className='flex'>
        <div id="detail" className='w-1/4 h-screen min-w-75'>
          <div className='h-1/2 p-4 overflow-y-auto'>
            <Dashboard></Dashboard>
          </div>
          <div className='max-h-1/2 min-h-1/2 overflow-y-auto break-words pl-4 border-4 border-gray-400'>
            <MessageBox></MessageBox>
          </div>
        </div>
      </div>
    </>
  )
}

export default App
