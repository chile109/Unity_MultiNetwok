using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpNetwork
{
	class ServerClass
	{
		private static byte[] _buffer = new byte[2048];     //緩存
		private static List<Socket> _Clients = new List<Socket>();  //client列表
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		static NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

		//取代不夠明確的IPAddress.Any位址
		static IPAddress GetIPV4()
		{
			foreach (IPAddressInformation ipInfo in nics[0].GetIPProperties().UnicastAddresses)
			{
				if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork)
					return ipInfo.Address;
			}
			return null;
		}

		//啟動 server
		public void SetServer()
		{
			Console.WriteLine("setting server");
			_serverSocket.Bind(new IPEndPoint(GetIPV4(), 100)); //sochet綁定IP & Port
			_serverSocket.Listen(10);    //允許連線的client佇列數量
			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);
		}

		private static void Broadcast(string msg)
		{
			byte[] data = Encoding.UTF8.GetBytes(msg);
			for (int i = 0; i < _Clients.Count; i++)
			{
				_Clients[i].Send(data);
			}
		}
		//開放連線委派
		private static void AccepCallBack(IAsyncResult result)
		{
			Socket mySocket = _serverSocket.EndAccept(result);
			_Clients.Add(mySocket);

			string msg = "Connection received from " + mySocket.RemoteEndPoint;
			Console.WriteLine(msg);
			Broadcast(msg);

			mySocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), mySocket);

			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);
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

				case "exit":        // Client離線
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
