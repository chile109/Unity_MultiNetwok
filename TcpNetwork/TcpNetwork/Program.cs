using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpNetwork
{
	class Program
	{
		private static byte[] _buffer = new byte[2048];		//緩存
		private static List<Socket> _Clients = new List<Socket>();
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		static void Main(string[] args)
		{
			Console.Title = "TCP server";
			SetServer();
			Console.ReadLine();
		}

		private static void SetServer()
		{
			Console.WriteLine("setting server");
			_serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));	//sochet綁定IP & Port
			_serverSocket.Listen(1);    //僅允許連接一個client
			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);
		}

		private static void AccepCallBack(IAsyncResult result)
		{
			Socket mySocket = _serverSocket.EndAccept(result);
			_Clients.Add(mySocket);

			Console.WriteLine("Client Connected");

			//訊息接收委派
			mySocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), mySocket);

			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);
		}

		private static void ReceiveCallback(IAsyncResult result)
		{

				Socket mySocket = (Socket)result.AsyncState;
				int Received = mySocket.EndReceive(result);
				byte[] _dataBuf = new byte[Received];
				Array.Copy(_buffer, _dataBuf, Received);
				string text = Encoding.ASCII.GetString(_dataBuf);
				Console.WriteLine("Received: " + text);

				string responce = string.Empty;

				switch (text.ToLower())
				{
					case "get time":
						responce = DateTime.Now.ToLongTimeString();
						break;

					default:
						responce = "Invalid Request";
						break;

				}

				byte[] data = Encoding.ASCII.GetBytes(responce);
				mySocket.BeginSend(data, 0, data
					.Length, SocketFlags.None, new AsyncCallback(SendCallback), mySocket);
				//訊息接收委派
				mySocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), mySocket);

		}

		private static void SendCallback(IAsyncResult result)
		{
			Socket mySocket = (Socket)result.AsyncState;
			mySocket.EndSend(result);
		}
	}
}
