using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebSocketExperiment.Game;
using WebSocketExperiment.Helpers;

namespace WebSocketExperiment.Handler
{
    public static class RequestHandler
    {
        public static async Task Echo(HttpContext context, WebSocket webSocket)
        {
            while(webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var data = JsonConvert.DeserializeObject<DataFromClient>(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                foreach (var pair in Program.IdToSocketDictionary)
                {
                    Program.Game.CountCurrentColor(data.Ball.Color);                    
                    var dataToSend = new Packet
                    {
                        Ball = data.Ball
                    };
                    var answer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataToSend));
                    if (pair.Key.Equals(data.Client))
                    {
                        continue;
                    }
                    if (pair.Value.State == WebSocketState.Open)
                    {
                        await pair.Value.SendAsync(new ArraySegment<byte>(answer), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                    if (Program.Game.ColorsAreEqual())
                    {
                        Program.Game.StopGame();
                    }
                }
            }
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed from server", CancellationToken.None);
        }
    }

    public class Packet
    {
        public Ball Ball { get; set; }
        public Color CurrentColor { get; set; }
    }
}
