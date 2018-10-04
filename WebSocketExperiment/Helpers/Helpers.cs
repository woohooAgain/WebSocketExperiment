using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketExperiment.Game;

namespace WebSocketExperiment.Helpers
{
    public static class Helpers
    {
        public static void SetGuid(WebSocket websocket)
        {
            var guid = Guid.NewGuid();
            Program.IdToSocketDictionary.Add(guid, websocket);
            websocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes($"Your id: {guid}")), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public class DataFromClient
    {
        public Guid Client { get; set; }
        public Ball Ball { get; set; }
    }
}
