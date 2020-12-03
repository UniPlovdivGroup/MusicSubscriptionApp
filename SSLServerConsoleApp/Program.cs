using System;
using System.Net;
using SSLServerEssentials;

namespace SSLServerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SSLServer testServer = new SSLServer(new IPEndPoint(IPAddress.Any, 1258));
            testServer.ListenForConnections(1);
            Console.ReadLine();
        }
    }
}
