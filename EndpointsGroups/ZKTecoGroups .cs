using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using ElectronNET.API;
using Interop.zkemkeeper;
using Newtonsoft.Json.Linq;

namespace electronNET.EndpointsGroups
{

    public class ZKTecoGroups
    {

        public static void TCPConnect(BrowserWindow window, string ip)
        {
            //var logChannel = Channel.CreateUnbounded<string>();
            //var zkemService = new ZKEMService(ip, 4370, 987123, window);
            //zkemService.StartAsync();


            //var cts = new CancellationTokenSource();
            //var cancellationToken = cts.Token;

            //var logChannelTask = Task.Run(async () =>
            //{
            //    while (await logChannel.Reader.WaitToReadAsync(cancellationToken))
            //    {
            //        while (logChannel.Reader.TryRead(out var log))
            //        {
            //            Console.WriteLine("send message");
            //            Electron.IpcMain.Send(window, "check", log);
            //        }
            //    }
            //}, cancellationToken);
            //await logChannelTask;



        }
        public class ZKEMService
        {
            public readonly CZKEMClass _zkem;
            //private readonly Channel<string> _logChannel;
            public readonly string _ipAddress;
            public readonly int _port;
            public readonly int _password;
            public readonly BrowserWindow _window;


            public ZKEMService(string ipAddress, int port, int password, BrowserWindow window)
            {
                //_logChannel = logChannel;
                _zkem = new CZKEMClass();
                _ipAddress = ipAddress;
                _port = port;
                _password = password;
                _window = window;
            }

            public void StartAsync()
            {
                _zkem.SetCommPassword(_password);

                bool isConnected = _zkem.Connect_Net(_ipAddress, _port);

                if (isConnected)
                {

                    if (_zkem.RegEvent(0, 65535))//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                    {
                        _zkem.OnAttTransactionEx -= new _IZKEMEvents_OnAttTransactionExEventHandler(OnAttTransactionEx);
                        _zkem.OnAttTransactionEx += new _IZKEMEvents_OnAttTransactionExEventHandler(OnAttTransactionEx);
                        //_zkem.OnEnrollFingerEx += new _IZKEMEvents_OnEnrollFingerExEventHandler(_zkem_OnEnrollFingerEx);
                        _zkem.OnDisConnected -= new _IZKEMEvents_OnDisConnectedEventHandler(OnDisConnected);
                        _zkem.OnDisConnected += new _IZKEMEvents_OnDisConnectedEventHandler(OnDisConnected);
                    }
                    Electron.IpcMain.Send(_window, "connectZKTeco", _ipAddress);
                    Electron.IpcMain.Send(_window, "check", $"{_ipAddress}連線成功");
                }
                else
                {
                    //Electron.IpcMain.Send(_window, "connectZKTeco", _ipAddress);
                    Electron.IpcMain.Send(_window, "check", $"{_ipAddress}連線失敗");

                    return;
                }
            }

            public void OnAttTransactionEx(string EnrollNumber, int IsInValid, int AttState, int VerifyMethod, int Year, int Month, int Day, int Hour, int Minute, int Second, int WorkCode)
            {
                // AttState 0上班 1下班 2外出 3外出返回 4加班簽到 5加班簽退 
                string attState;
                switch (AttState)
                {
                    case 0:
                        attState = "上班";
                        break;
                    case 1:
                        attState = "下班";
                        break;
                    case 2:
                        attState = "外出";
                        break;
                    case 3:
                        attState = "外出返回";
                        break;
                    case 4:
                        attState = "加班簽到";
                        break;
                    case 5:
                        attState = "加班簽退";
                        break;
                    default:
                        attState = "";
                        break;
                }
                string logData = $"{attState} 打卡 工號: {EnrollNumber}, 日期: {Year}-{Month}-{Day}, 時間: {Hour}:{Minute}:{Second}";
                Console.WriteLine(logData);
                Console.WriteLine("send message");
                Electron.IpcMain.Send(_window, "check", logData);

                //_logChannel.Writer.TryWrite(logData);
            }
            public void OnDisConnected()
            {
                Electron.IpcMain.Send(_window, "check", $"{_ipAddress}斷線重新連線中");
                StartAsync();
            }
            public void CleanEvent()
            {
                _zkem.OnAttTransactionEx -= new _IZKEMEvents_OnAttTransactionExEventHandler(OnAttTransactionEx);
                _zkem.OnDisConnected -= new _IZKEMEvents_OnDisConnectedEventHandler(OnDisConnected);
            }
        }
    }
}
