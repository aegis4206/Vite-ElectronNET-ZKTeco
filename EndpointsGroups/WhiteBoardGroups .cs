using System.Net.WebSockets;
using System.Threading.Channels;
using ElectronNET.API;
using Interop.zkemkeeper;

namespace electronNET.EndpointsGroups
{

    public static class WhiteBoardGroups
    {

        public static RouteGroupBuilder MapWhiteBoardaApi(this RouteGroupBuilder group)
        {
            group.MapGet("/ws", async (HttpContext context) =>
           {
               Console.WriteLine("WebSocket try!");

               if (context.WebSockets.IsWebSocketRequest)
               {
                   var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                   Console.WriteLine("WebSocket connected!");
                   var logChannel = Channel.CreateUnbounded<string>();

                   var cts = new CancellationTokenSource();
                   var cancellationToken = cts.Token;

                   var logChannelTask = Task.Run(async () =>
                   {
                       while (await logChannel.Reader.WaitToReadAsync(cancellationToken))
                       {
                           while (logChannel.Reader.TryRead(out var log))
                           {
                               Console.WriteLine("send message");
                               var message = System.Text.Encoding.UTF8.GetBytes(log);
                               await webSocket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
                           }
                       }
                   }, cancellationToken);

                   try
                   {
                       var buffer = new byte[1024];
                       WebSocketReceiveResult result;

                       do
                       {
                           result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                           if (result.MessageType == WebSocketMessageType.Close)
                           {
                               Console.WriteLine("WebSocket close");
                               //停止通道處理
                               cts.Cancel();
                               await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close", cancellationToken);
                               break;
                           }
                       } while (!result.CloseStatus.HasValue);
                   }
                   catch (OperationCanceledException)
                   {
                       await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close", cancellationToken);

                       Console.WriteLine("operations cancel");
                   }
                   catch (WebSocketException ex)
                   {
                       await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "close", cancellationToken);

                       Console.WriteLine($"WebSocket error : {ex.Message}");
                   }
                   await logChannelTask;

               }
               else
               {
                   context.Response.StatusCode = 400;
               }
           }
           );


            return group;
        }



    }

    

}
