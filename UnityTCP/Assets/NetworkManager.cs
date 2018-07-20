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
	public InputField KeyHost;
	public Button c_btn;
	public Button s_btn;

	private static Socket _clientSocket;
	static IPEndPoint remoteEP;

	private ClientThread ct;

	public static NetworkManager Singleton { get; set; }
	Queue<Action> mThreadTaskOnMain = new Queue<Action>();

	void Awake()
	{
		// 確保不死
		DontDestroyOnLoad(this);
		Singleton = this;
	}

	private void Start()
	{
		KeyHost.text = "10.211.55.3:100";
		c_btn.onClick.AddListener(delegate
		{
			SetHost();
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			ct = new ClientThread(_clientSocket, Name, remoteEP);
			ct.StartConnect();
			ct.handlerString += ConnectFailed;
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

		lock (mThreadTaskOnMain)
		{
			if (mThreadTaskOnMain.Count < 1)
				return;
			// 取出
			var task = mThreadTaskOnMain.Dequeue();
			// 執行
			task();
		}
	}

	void SetHost()
	{
		string[] info = new string[2];
		info[0] = KeyHost.text.Split(':')[0];
		info[1] = KeyHost.text.Split(':')[1];
		remoteEP = new IPEndPoint(IPAddress.Parse(info[0]), int.Parse(info[1]));
	}

	public void ConnectFailed(string msg)
	{
		Chatbox.text = msg;
	}

	public void AddTask(Action task)
	{
		lock (mThreadTaskOnMain)
		{
			mThreadTaskOnMain.Enqueue(task);
		}
	}

	public void SendInput()
	{
		ct.Send(Keyin.text);
	}

	private void OnApplicationQuit()
	{
		if (ct != null)
		{
			ct.Send("exit");
		}
	}


}
