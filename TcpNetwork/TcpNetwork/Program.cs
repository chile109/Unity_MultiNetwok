using System;

namespace TcpNetwork
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Title = "TCP server";
			ServerClass Server_inst = new ServerClass();
			Server_inst.SetServer();
			Console.ReadLine();
		}
	}
}
