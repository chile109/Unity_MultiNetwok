using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class NetworkManager : MonoBehaviour
{
	public string Name = "Kevin";
	public InputField Keyin;
	public Button s_btn;
	private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("10.211.55.3"), 100);

	private ClientThread ct;
	private bool isReceive;

	private void Start()
	{
		ct = new ClientThread(_clientSocket, Name, remoteEP);
		ct.StartConnect();
	}

	private void Update()
	{
		if (ct.receiveMessage != null)
		{
			Debug.Log("Server:" + ct.receiveMessage);
			ct.receiveMessage = null;
		}

		ct.Receive();
	}

	public void SendInput()
	{
		ct.Send(Keyin.text);
	}

	private void OnApplicationQuit()
	{
		ct.StopConnect();
	}


}
