using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace TcpClient
{
    class Program
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "TCP client";
            LoopConnect();
            SendLoop();
            Console.ReadLine();
        }
		//嘗試發訊
        private static void SendLoop()
        {
            while(true)
            {
                Console.Write("Enter a request: ");
                string req = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                _clientSocket.Send(buffer);		//發送request

                byte[] receivedBuf = new byte[1024];
                int rec = _clientSocket.Receive(receivedBuf);	//接收responce

                byte[] data = new byte[rec];
                Array.Copy(receivedBuf, data, rec);		//裁減responce
                Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
            }
        }
		//嘗試連線
        private static void LoopConnect()
        {
            int attempts = 0;		//連線次數

            while (!_clientSocket.Connected)

                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, 100);
                }
                catch(SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attemps: " + attempts.ToString());
                }
            Console.Clear();
            Console.WriteLine("connected");
        }
    }
}
