using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class NetworkManager : MonoBehaviour
{
	public InputField Keyin;
	public Button s_btn;
	private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("10.211.55.3"), 100);
	int attempts = 0;       //連線次數
							// Use this for initialization
	void Start()
	{
		LoopConnect();
		//s_btn.interactable = false;
		//ClientTCP tCP = new ClientTCP();
		//tCP.Connect();
	}

	//嘗試發訊
	public void Send()
	{
		string req = Keyin.text;
		byte[] buffer = Encoding.UTF8.GetBytes(req);
		_clientSocket.Send(buffer);     //發送request

		byte[] receivedBuf = new byte[1024];
		int rec = _clientSocket.Receive(receivedBuf);   //接收responce

		byte[] data = new byte[rec];
		Array.Copy(receivedBuf, data, rec);     //裁減responce
		Debug.Log("Received: " + Encoding.UTF8.GetString(data));
	}

	//private void Update()
	//{
	//	if (_clientSocket.Connected)
	//	{
	//		try
	//		{
	//			attempts++;
	//			_clientSocket.Connect(remoteEP);
	//		}
	//		catch (SocketException)
	//		{
	//			Debug.Log("Connection attemps: " + attempts.ToString());
	//		}
	//	}
	//	else
	//	{
	//		s_btn.interactable = true;
	//	}
	//}

	private static void LoopConnect()
	{
		int attempts = 0;       //連線次數

		while (!_clientSocket.Connected)

			try
			{
				attempts++;
				_clientSocket.Connect(remoteEP);
			}
			catch (SocketException)
			{
				Debug.Log("Connection attemps: " + attempts.ToString());
			}

		Debug.Log("connected");
	}

}
