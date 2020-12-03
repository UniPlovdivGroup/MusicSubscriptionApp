using System;
using System.Net;
using LoggerDebug;
using SSLClientEssentials;
using SSLConnectionEssentials;

namespace SSLClientConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            SSLClient testClient = new SSLClient();
            testClient.LoopConnect(new IPEndPoint(IPAddress.Loopback, 1258));
            testClient.OnNewDataFullyReceived += HandleFullyReceivedPackage;
            DataToSend test = new DataToSend("1", "1", "1", "1", "1", "1", "1", "1");
            testClient.StartReceivingData();
            testClient.SendData<DataToSend>(test, "DataSent");
            Console.Write("Disconnect?:");
            string notImportant = Console.ReadLine();
            if (notImportant == "Disconnect") testClient.Disconnect();
            Console.ReadLine();
        }
        private static void HandleFullyReceivedPackage(object sender, EventArgs e)
        {
            ReceivedDataArgs args = (ReceivedDataArgs)e;
            dynamic receivedObject = args.GetReceivedObject();
            if (receivedObject.GetType() == typeof(DataToSend))
            {
                DataToSend reconstructed = (receivedObject as DataToSend);
                Logger.Log(reconstructed.ToString(), false);
                reconstructed.Dispose();

            }
        }
    }
}
