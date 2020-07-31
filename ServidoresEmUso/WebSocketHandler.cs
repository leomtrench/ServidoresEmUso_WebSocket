using Microsoft.AspNetCore.Builder;
using ServidoresEmUso.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServidoresEmUso
{
    public static class WebSocketHandler
    {
        public static void InitializeWebSocket(IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await WebSocketHandler.Echo(webSocket);
                    return;
                }
                await next();
            });
        }

        public static List<WebSocket> ActiveWebSockets = new List<WebSocket>();
        public static async Task Echo(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];

            buffer = await SendMessageAsync(webSocket, ServidorBusiness.Instance.JsonServidores);

            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            ActiveWebSockets.Add(webSocket);
            while (!result.CloseStatus.HasValue)
            {
                result = await HandleReceivedMessageAsync(webSocket, buffer, result);
            }

            await RemoveFromActiveWebSockets(webSocket);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private static async Task RemoveFromActiveWebSockets(WebSocket webSocket)
        {
            await SemaphoreSlim.WaitAsync();
            try
            {
                ActiveWebSockets.Remove(webSocket);
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        static SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
        private static async Task<WebSocketReceiveResult> HandleReceivedMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketReceiveResult result)
        {
            var receivedMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

            ServidorBusiness.Instance.ChangeStatusServidor(receivedMessage);
            await SendMessageToActiveWebSockets();

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            return result;
        }

        private static async Task SendMessageToActiveWebSockets()
        {
            await SemaphoreSlim.WaitAsync();
            try
            {
                List<WebSocket> webSocketsWithError = new List<WebSocket>();
                foreach (var activeWebSocket in ActiveWebSockets)
                {
                    try
                    {
                        await SendMessageAsync(activeWebSocket, ServidorBusiness.Instance.JsonServidores);
                    }
                    catch (Exception ex)
                    {
                        webSocketsWithError.Add(activeWebSocket);
                    }
                }
                ActiveWebSockets.RemoveAll(x => webSocketsWithError.Contains(x));
                webSocketsWithError.Clear();
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        private static async Task<byte[]> SendMessageAsync(WebSocket webSocket, string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            return buffer;
        }
    }
}
