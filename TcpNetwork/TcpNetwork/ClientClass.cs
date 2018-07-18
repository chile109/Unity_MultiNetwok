using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpNetwork
{
	public class ClientClass
	{
		public string client_ID;
		public string ip;
		public TcpClient mySocket;
		public NetworkStream myStream;  //通訊流
		private byte[] _buffer;  //暫存

		public void Start()
		{
			//封包傳送與接收的大小設定
			mySocket.SendBufferSize = 4096;
			mySocket.ReceiveBufferSize = 4096;

			myStream = mySocket.GetStream();
			_buffer = new byte[4096];
			myStream.BeginRead(_buffer, 0, mySocket.ReceiveBufferSize, ReceiveDataCallBack, null);
			Send("%NAME");
		}

		private void ReceiveDataCallBack(IAsyncResult result)
		{
			try
			{

				int readbytes = myStream.EndRead(result);
				if (readbytes <= 0)
				{
					return;
				}

				string text = Encoding.UTF8.GetString(_buffer, 0, readbytes);
				byte[] newBytes = new byte[readbytes];  //處理過後的byte
				Buffer.BlockCopy(_buffer, 0, newBytes, 0, readbytes);  //擷取剛好的byte長度而不是整個Buffer
				ApiResponce(text);
			}
			catch (SocketException e)
			{
				Console.WriteLine(e.ToString());
				KillSocket();
				return;
			}


		}
		internal void Send(string msg)
		{
			byte[] data = Encoding.UTF8.GetBytes(msg);
			myStream.Write(data, 0, data.Length);
		}

		
		//收到data呼叫的委派
		private void ApiResponce(string text)
		{
			string responce = string.Empty;
			switch (text.ToLower())
			{
				case "get time":    //取得系統時間
					RebackTime();
					return;

				case "exit":        // Client離線
					KillSocket();
					return;

				default:            //不合法請求
					if (text.Contains("%NAME"))
					{
						client_ID = text.Split('|')[1];
						responce = client_ID + " Is Online!";
					}
					else
					{
						responce = client_ID + ": " + text;
					}
					break;

			}
			myStream.BeginRead(_buffer, 0, mySocket.ReceiveBufferSize, ReceiveDataCallBack, null);   //重新接收資料
			ServerClass.Broadcast(responce);
			Console.WriteLine(client_ID + ": " + text);
		}

		private void RebackTime()
		{
			Send(DateTime.Now.ToLongTimeString());
			myStream.BeginRead(_buffer, 0, mySocket.ReceiveBufferSize, ReceiveDataCallBack, null); 
		}

		private void KillSocket()
		{
			Send("disconnected");
			mySocket.Close();
			ServerClass._Clients.Remove(this);
			Console.WriteLine("Client disconnected");
		}
	}
}
