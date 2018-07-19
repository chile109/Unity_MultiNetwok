using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

class ClientThread
{
	public struct Struct_Internet
	{
		public string name;
		public string ip;
		public int port;
	}

	public Socket clientSocket;//連線使用的Socket
	private Struct_Internet internet;
	public string receiveMessage;
	private string sendMessage;

	private Thread threadReceive;
	private Thread threadConnect;

	public ClientThread(Socket _socket, string _name, IPEndPoint EP)
	{
		clientSocket = _socket;
		internet.name = _name;
		internet.ip = EP.Address.ToString();
		internet.port = EP.Port;
		receiveMessage = null;
	}

	public void StartConnect()
	{
		threadConnect = new Thread(LoopConnect);
		threadConnect.Start();
	}

	public void StopConnect()
	{
		try
		{
			clientSocket.Close();
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}

	public void Send(string message)
	{
		if (message == null)
			throw new NullReferenceException("message不可為Null");
		else
			sendMessage = message;
		SendMessage();
	}

	public void Receive()
	{
		if (threadReceive != null && threadReceive.IsAlive == true)
			return;
		
		threadReceive = new Thread(ReceiveMessage);
		threadReceive.IsBackground = true;
		threadReceive.Start();
	}

	private void LoopConnect()
	{
		int attempts = 0;       //連線次數

		while (!clientSocket.Connected)

			try
			{
				attempts++;
				clientSocket.Connect(IPAddress.Parse(internet.ip), internet.port);
			}
			catch (SocketException)
			{
				Debug.Log("Connection attemps: " + attempts.ToString());
			}
	}

	private void SendMessage()
	{
		try
		{
			if (clientSocket.Connected == true)
			{
				clientSocket.Send(Encoding.UTF8.GetBytes(sendMessage));
			}
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}

	public void ReceiveMessage()
	{
		if (clientSocket.Connected == true)
		{
			byte[] receivedBuf = new byte[1024];
			int rec = clientSocket.Receive(receivedBuf);   //接收responce

			byte[] data = new byte[rec];
			Array.Copy(receivedBuf, data, rec);     //裁減responce
			receiveMessage = Encoding.UTF8.GetString(data);

			if (Encoding.UTF8.GetString(data) == "%NAME")
				Send("%NAME|" + internet.name);
			if (Encoding.UTF8.GetString(data) == "disconnected")
				StopConnect();
		}
	}

}