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
	public string chatmsg;

	public Text Chatbox;
	public InputField Keyin;
	public Button c_btn;
	public Button s_btn;

	private static Socket _clientSocket;
	static IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("10.211.55.3"), 100);

	private ClientThread ct;
	private bool isReceive;

	private void Start()
	{
		c_btn.onClick.AddListener(delegate {
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			ct = new ClientThread(_clientSocket, Name, remoteEP);
			ct.StartConnect();
		}); 
	}

	private void Update()
	{
		if (ct != null)
		{
			c_btn.interactable = !ct.clientSocket.Connected;

			if (ct.receiveMessage != null)
			{
				Debug.Log("Server:" + ct.receiveMessage);
				chatmsg += ct.receiveMessage + Environment.NewLine;
				Chatbox.text = chatmsg;
				ct.receiveMessage = null;
			}

			ct.Receive();
		}



	}

	public void SendInput()
	{
		ct.Send(Keyin.text);
	}

	private void OnApplicationQuit()
	{
		ct.Send("exit");

	}


}
