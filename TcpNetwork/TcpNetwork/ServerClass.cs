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
		public static List<ClientClass> _Clients = new List<ClientClass>();  //client列表
		private static TcpListener _serverSocket;

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
			_serverSocket = new TcpListener(GetIPV4(), 100);
			_serverSocket.Start();
			_serverSocket.BeginAcceptTcpClient(AccepCallBack, null);
			Console.WriteLine("Server Start");
		}

		public static void Broadcast(string msg)
		{
			foreach (var c in _Clients)
			{
				c.Send(msg);
			}
		}
		//開放連線委派
		private static void AccepCallBack(IAsyncResult result)
		{
			ClientClass tempclient = new ClientClass();
			TcpClient TempSocket = _serverSocket.EndAcceptTcpClient(result);
			_serverSocket.BeginAcceptTcpClient(AccepCallBack, null);

			tempclient.mySocket = TempSocket;
			tempclient.ip = TempSocket.Client.RemoteEndPoint.ToString();
			tempclient.Start();
			_Clients.Add(tempclient);
			Console.WriteLine("Connection received from " + _Clients[0].ip);
		}
	}
}
