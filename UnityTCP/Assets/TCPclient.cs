using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientTCP
{
	public TcpClient client;
	public NetworkStream myStream;
	private byte[] asyncBuffer;
	public bool isConnected;

	private string IP_Adress = "10.211.55.3";
	private int Port = 100;

	public void Connect()
	{
		Debug.Log("try to connecting!");
		client = new TcpClient();
		client.ReceiveBufferSize = 4096;
		client.SendBufferSize = 4096;
		asyncBuffer = new byte[8192];

		try
		{
			client.BeginConnect(IP_Adress, Port, new AsyncCallback(ConnectCallBack), client);
		}
		catch (Exception)
		{
			Debug.Log("Fail Connect");
		}
	}

	private void ConnectCallBack(IAsyncResult result)
	{
		Debug.Log("callback");
		try
		{
			Debug.Log("callback1");
			client.EndConnect(result);
			Debug.Log("callback2");
			if (client.Connected == false) { return; }
			else
			{
				myStream = client.GetStream();
				myStream.BeginRead(asyncBuffer, 0, 8192, OnReceiveData, null);
				isConnected = true;
				Debug.Log("connect successful");
			}

		}
		catch (Exception ex)
		{
			Debug.Log(ex);
			isConnected = false;
			return;
		}
	}

	private void OnReceiveData(IAsyncResult result)
	{
		try
		{

		}
		catch (Exception)
		{
			throw;
		}
	}
}

