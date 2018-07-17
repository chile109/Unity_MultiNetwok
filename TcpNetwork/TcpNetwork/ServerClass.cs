using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace TcpNetwork
{
	class ServerClass
	{
		private static byte[] _buffer = new byte[2048];     //緩存
		private static List<ClientClass> _Clients = new List<ClientClass>();  //client列表
		private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		static NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
		static ClientClass _tempClient = new ClientClass();
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

		private static void Broadcast(string msg, List<ClientClass> _clients)
		{
			byte[] data = Encoding.UTF8.GetBytes(msg);
			foreach (var c in _clients)
			{
				c.socket.Send(data);
			}
		}
		//開放連線委派
		private static void AccepCallBack(IAsyncResult result)
		{
			_tempClient.socket = _serverSocket.EndAccept(result);
			_tempClient.ip = _tempClient.socket.RemoteEndPoint.ToString();

			_Clients.Add(_tempClient);

			string msg = _tempClient.ip + " Connected";
			Broadcast("%NAME", new List<ClientClass>() { _Clients[_Clients.Count - 1] });
			Console.WriteLine(msg);

			_tempClient.socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _tempClient.socket);
			_serverSocket.BeginAccept(new AsyncCallback(AccepCallBack), null);
		}

		//訊息接收委派
		private static void ReceiveCallback(IAsyncResult result)
		{
			_tempClient.socket = (Socket)result.AsyncState;

			int Received;
			try
			{
				Received = _tempClient.socket.EndReceive(result);
			}
			catch (SocketException)
			{
				Console.WriteLine("Client forcefully disconnected");
				// Don't shutdown because the socket may be disposed and its disconnected anyway.
				_tempClient.socket.Close();
				_Clients.Remove(_tempClient);
				return;
			}

			byte[] _dataBuf = new byte[Received];
			Array.Copy(_buffer, _dataBuf, Received);    //ReSize Buffer

			string text = Encoding.UTF8.GetString(_dataBuf);
			string responce = string.Empty;
			byte[] data;

			//Api在此實作
			switch (text.ToLower())
			{
				case "get time":    //取得系統時間
					responce = DateTime.Now.ToLongTimeString();
					data = Encoding.UTF8.GetBytes(responce);
					_tempClient.socket.Send(data);
					break;

				case "exit":        // Client離線
					responce = "disconnected";
					data = Encoding.UTF8.GetBytes(responce);
					_tempClient.socket.Send(data);
					//_tempClient.socket.BeginSend(data, 0, data.Length, SocketFlags.None,SendCallback, _tempClient.socket);	//非同步方式發送

					// Always Shutdown before closing
					_tempClient.socket.Shutdown(SocketShutdown.Both);
					_tempClient.socket.Close();
					_Clients.Remove(_tempClient);
					Console.WriteLine("Client disconnected");
					return;

				default:            //不合法請求
					if (text.Contains("%NAME"))
					{
						_tempClient.client_ID = text.Split('|')[1];
					}
					else
					{
						responce = "Invalid Request";
						data = Encoding.UTF8.GetBytes(responce);
						_tempClient.socket.Send(data);
					}
					break;

			}
			Console.WriteLine(_tempClient.client_ID + ": " + text);

			_tempClient.socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), _tempClient.socket);
		}
	}
}
