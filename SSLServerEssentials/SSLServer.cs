using LoggerDebug;
using SSLConnectionEssentials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace SSLServerEssentials
{
    public class SSLServer
    { // Server certificate
        private X509Certificate2 ServerX509Certificate { get; set; }

        // Represents the listening socket
        private Socket ServerSocket { get; set; }

        // List of already established connection
        private List<Connection> EstablishedConnections { get; set; }


        // Creates the server
        public SSLServer(IPEndPoint endPointToListenOn)
        {
            Logger.Log("Setting up server...", false);
            ServerX509Certificate = new X509Certificate2(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Cert.pfx", "Devin900");
            EstablishedConnections = new List<Connection>();
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ServerSocket.Bind(endPointToListenOn);
        }

        private void ExceptionHandler(Action MethodToHandle)
        {
            try
            {
                MethodToHandle();
            }
            catch (Exception e)
            {
                Logger.Log(e, false);
                Logger.Log("Connection error", false);
                ServerSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
        }

        // Places the server in a listening state
        public void ListenForConnections(int countOfMaximumPendingConnections)
        {
            ExceptionHandler(() =>
            {
                ServerSocket.Listen(countOfMaximumPendingConnections);
                ServerSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            });

        }

        // Handles what happens when new connection is made
        private void AcceptCallBack(IAsyncResult AR)
        {
            ExceptionHandler(() =>
            {
                Socket socket = ServerSocket.EndAccept(AR);
                SslStream authenticatedSslStream = new SslStream(new NetworkStream(socket));
                authenticatedSslStream.AuthenticateAsServer(ServerX509Certificate, false, true);
                Connection newCon = new Connection(socket, authenticatedSslStream);
                EstablishedConnections.Add(newCon);
                newCon.OnConnectionLost += ConnectionLostHandler;
                newCon.OnNewDataFullyReceived += HandleFullyReceivedPackage;
                newCon.StartReceivingData();
                Logger.Log("Client connected", false);
                ServerSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            });
        }
        private void HandleFullyReceivedPackage(object sender, EventArgs e)
        {
            ReceivedDataArgs args = (ReceivedDataArgs)e;
            dynamic receivedObject = args.GetReceivedObject();
            if (receivedObject.GetType() == typeof(DataToSend))
            {
                DataToSend reconstructed = (receivedObject as DataToSend);
                Logger.Log(reconstructed.ToString(),false);

                (sender as Connection).SendData(reconstructed, "DataSent");
                reconstructed.Dispose();

            }
        }

        // Controls what happens whenever a connection is lost
        private void ConnectionLostHandler(object s, EventArgs e)
        {
            Connection connectionThatWasLost = s as Connection;

            if (!connectionThatWasLost.IsDisposed)
            {
                connectionThatWasLost.Dispose();
            }
            if (EstablishedConnections.Contains(connectionThatWasLost))
            {
                EstablishedConnections.Remove(connectionThatWasLost);
            }

            connectionThatWasLost = null;

            GC.Collect();
        }
    }
}
