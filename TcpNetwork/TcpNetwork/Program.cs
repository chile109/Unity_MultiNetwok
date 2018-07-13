using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpNetwork
{
	class Program
	{
		private static byte[] _buffer = new byte[2048];		//緩存
		private static List<Socket> _Clients = new List<Socket>();	//client列表
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		static NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
		static IPAddress GetIPV4()
		{
			foreach (IPAddressInformation ipInfo in nics[0].GetIPProperties().UnicastAddresses)
			{
				if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
					return ipInfo.Address;
			}
			return null;
		}
	static void Main(string[] args)
		{
			Console.Title = "TCP server";
			SetServer();
			Console.ReadLine();
		}

		//啟動 server
		private static void SetServer()
		{
			Console.WriteLine("setting server");
			_serverSocket.Bind(new IPEndPoint(GetIPV4(), 100));	//sochet綁定IP & Port
			_serverSocket.Listen(10);    //允許進入的client佇列數量
			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);	
		}

		//開放連線委派
		private static void AccepCallBack(IAsyncResult result)
		{
			Socket mySocket = _serverSocket.EndAccept(result);
			_Clients.Add(mySocket);

			Console.WriteLine("Client Connected");
			
			mySocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), mySocket);

			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);
		}

		//訊息接收委派
		private static void ReceiveCallback(IAsyncResult result)
		{
				Socket mySocket = (Socket)result.AsyncState;
				int Received = mySocket.EndReceive(result);
				byte[] _dataBuf = new byte[Received];
				Array.Copy(_buffer, _dataBuf, Received);	//ReSize Buffer

				string text = Encoding.UTF8.GetString(_dataBuf);
				Console.WriteLine("Received: " + text);

				string responce = string.Empty;

				//Api在此實作
				switch (text.ToLower())
				{
					case "get time":	//取得系統時間
						responce = DateTime.Now.ToLongTimeString();
						break;

					default:			//不合法請求
						responce = "Invalid Request";
						break;

				}

				//回傳資訊
				byte[] data = Encoding.UTF8.GetBytes(responce);
				mySocket.BeginSend(data, 0, data
					.Length, SocketFlags.None, new AsyncCallback(SendCallback), mySocket);

				mySocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), mySocket);

		}
		//回傳委派
		private static void SendCallback(IAsyncResult result)
		{
			Socket mySocket = (Socket)result.AsyncState;
			mySocket.EndSend(result);
		}
	}
}
