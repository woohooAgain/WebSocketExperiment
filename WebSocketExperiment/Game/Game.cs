using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketExperiment.Game
{
    public class Game
    {
        private Color FinalColor;
        public Color CurrentColor;

        public Game()
        {
            FinalColor = new Color(true);
            CurrentColor = new Color(false);
            foreach (var client in Program.Clients)
            {
                client.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes("Game has begun")), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public void CountCurrentColor(Color ballColor)
        {
            //Todo Do it well
            if (ballColor.Red == 255)
            {
                if (IsClear())
                {
                    CurrentColor.Red = (FinalColor.Red + FinalColor.Blue + FinalColor.Green) / 3;
                }
                else
                {
                    CurrentColor.Red += (ballColor.Red - CurrentColor.Red) / 3;
                    CurrentColor.Blue += (ballColor.Blue - CurrentColor.Blue) / 6;
                    CurrentColor.Green += (ballColor.Green - CurrentColor.Green) / 6;
                }                
            }
            else if (ballColor.Blue == 255)
            {
                if (IsClear())
                {
                    CurrentColor.Blue = (FinalColor.Red + FinalColor.Blue + FinalColor.Green) / 3;
                }
                else
                {
                    CurrentColor.Red += (ballColor.Red - CurrentColor.Red) / 6;
                    CurrentColor.Blue += (ballColor.Blue - CurrentColor.Blue) / 3;
                    CurrentColor.Green += (ballColor.Green - CurrentColor.Green) / 6;
                }
            }
            else
            {
                if (IsClear())
                {
                    CurrentColor.Green = (FinalColor.Red + FinalColor.Blue + FinalColor.Green) / 3;
                }
                else
                {
                    CurrentColor.Red += (ballColor.Red - CurrentColor.Red) / 6;
                    CurrentColor.Blue += (ballColor.Blue - CurrentColor.Blue) / 6;
                    CurrentColor.Green += (ballColor.Green - CurrentColor.Green) / 3;
                }
            }
        }

        public bool ColorsAreEqual()
        {
            return Math.Sqrt(ColorDiff(CurrentColor.Red, FinalColor.Red) + ColorDiff(CurrentColor.Blue, FinalColor.Blue) + ColorDiff(CurrentColor.Green, FinalColor.Green)) < 20;
        }

        public void StopGame()
        {
            foreach (var client in Program.Clients)
            {
                client.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes("Game has ended")), WebSocketMessageType.Text, true, CancellationToken.None);
                client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed from server", CancellationToken.None);
            }
            Program.OpenedConnections = 0;
            Program.Clients.RemoveAll(x => true);
        }

        private double ColorDiff(int a, int b)
        {
            return Math.Pow(a - b, 2);
        }

        private bool IsClear()
        {
            return CurrentColor.Red + CurrentColor.Blue + CurrentColor.Green == 0;
        }
    }

    public class Color
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public Color(bool random)
        {
            if (random)
            {
                var rnd = new Random();
                Red = rnd.Next(256);
                Green = rnd.Next(256);
                Blue = rnd.Next(256);
            }
        }
    }

    public class Ball
    {
        //public Guid Client { get; set; }
        public Color Color { get; set; }
        public double FromX { get; set; }
        public double FromY { get; set; }
        public double FromZ { get; set; }
        public double TangentX { get; set; }
        public double TangentY { get; set; }
        public double TangentZ { get; set; }
        public DateTime StartTime { get; set; }
    }
}
