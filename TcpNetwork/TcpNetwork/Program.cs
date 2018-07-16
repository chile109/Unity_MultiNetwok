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
		private static byte[] _buffer = new byte[2048];     //緩存
		private static List<Socket> _Clients = new List<Socket>();  //client列表
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
			_serverSocket.Bind(new IPEndPoint(GetIPV4(), 100)); //sochet綁定IP & Port
			_serverSocket.Listen(10);    //允許連線的client佇列數量
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

			//找到空的array位置Assign連接的client
			/*for (int i = 0; i < _Clients.Count; i++)
			{
				if (_Clients[i] != null)
				{
					Console.WriteLine("Connection received from " + );
					return; //中斷迴圈以免所有空array都被Assign
				}
			}*/
		}

		//訊息接收委派
		private static void ReceiveCallback(IAsyncResult result)
		{
			Socket CurrentSocket = (Socket)result.AsyncState;
			int Received;
			try
			{
				Received = CurrentSocket.EndReceive(result);
			}
			catch (SocketException)
			{
				Console.WriteLine("Client forcefully disconnected");
				// Don't shutdown because the socket may be disposed and its disconnected anyway.
				CurrentSocket.Close();
				_Clients.Remove(CurrentSocket);
				return;
			}

			byte[] _dataBuf = new byte[Received];
			Array.Copy(_buffer, _dataBuf, Received);    //ReSize Buffer

			string text = Encoding.UTF8.GetString(_dataBuf);
			Console.WriteLine("Received: " + text);
			string responce = string.Empty;
			byte[] data;

			//Api在此實作
			switch (text.ToLower())
			{
				case "get time":    //取得系統時間
					responce = DateTime.Now.ToLongTimeString();
					data = Encoding.UTF8.GetBytes(responce);
					CurrentSocket.Send(data);
					break;

				case "exit":		// Client離線
					responce = "disconnected";
					data = Encoding.UTF8.GetBytes(responce);
					CurrentSocket.Send(data);
					//CurrentSocket.BeginSend(data, 0, data.Length, SocketFlags.None,SendCallback, CurrentSocket);	//非同步方式發送

					// Always Shutdown before closing
					CurrentSocket.Shutdown(SocketShutdown.Both);
					CurrentSocket.Close();
					_Clients.Remove(CurrentSocket);
					Console.WriteLine("Client disconnected");
					return;

				default:            //不合法請求
					responce = "Invalid Request";
					data = Encoding.UTF8.GetBytes(responce);
					CurrentSocket.Send(data);
					break;

			}

			CurrentSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), CurrentSocket);
	
		}
	}
}
