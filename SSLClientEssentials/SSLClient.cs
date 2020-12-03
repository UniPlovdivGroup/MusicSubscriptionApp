using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using SSLConnectionEssentials;
using LoggerDebug;

namespace SSLClientEssentials
{
    public class SSLClient
    {
        // Informs whether there is a connection currently
        public bool IsConnected { get; private set; }

        public EventHandler ConnectionLost { get; set; }

        public EventHandler OnNewDataFullyReceived { get; set; }

        // Represents the current remote end point
        private IPEndPoint CurrentRemoteEndPoint { get; set; }

        // Represents the client certificate
        private X509Certificate ClientX509Certificate { get; set; }

        // Represents the current client socket
        private Socket CurrentClientSocket { get; set; }

        // Represents the current validates SslStream
        private SslStream CurrentValidatedSslStream { get; set; }

        // Represents the current connection to the server
        private Connection ConnectionToServer { get; set; }


        // Constructor
        public SSLClient()
        {
            ClientX509Certificate =
                X509Certificate.CreateFromCertFile(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "cert.crt");
        }


        // Attempts to connect to the server once
        public bool Connect(IPEndPoint RemoteEndPointToConnectTo)
        {
            if (IsConnected) return false;

            
            CurrentClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                CurrentClientSocket.ConnectAsync(RemoteEndPointToConnectTo).Wait(4000);
            }
            catch
            {
                CurrentClientSocket = null;
                return false;
            }

            if (CurrentClientSocket.Connected)
            {
                CurrentRemoteEndPoint = RemoteEndPointToConnectTo;
                CurrentValidatedSslStream = CreateAndValidateSslStream(CurrentClientSocket);
                ConnectionToServer = new Connection(CurrentClientSocket, CurrentValidatedSslStream);
                ConnectionToServer.OnConnectionLost += ConnectionLostHandler;
                ConnectionToServer.OnNewDataFullyReceived += HandleFullyReceivedPackage;
                IsConnected = true;

                return true;
            }
            return false;
        }

        // Attempts to connect to the server until connection is achieved
        public void LoopConnect(IPEndPoint RemoteEndPointToConnectTo)
        {
            if (IsConnected) return;

            CurrentClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while (!CurrentClientSocket.Connected)
            {
                try
                {
                    CurrentClientSocket.ConnectAsync(RemoteEndPointToConnectTo).Wait(1000);
                }
                catch
                {
                    Logger.Log("Trying to Connect", false);
                }
            }
            CurrentRemoteEndPoint = RemoteEndPointToConnectTo;
            CurrentValidatedSslStream = CreateAndValidateSslStream(CurrentClientSocket);
            ConnectionToServer = new Connection(CurrentClientSocket, CurrentValidatedSslStream);
            ConnectionToServer.OnConnectionLost += ConnectionLostHandler;
            ConnectionToServer.OnNewDataFullyReceived += HandleFullyReceivedPackage;
            IsConnected = true;

            Logger.ClearDisplay();
            Logger.Log("Connected to server", false);
        }

        // Tells the current connection to disconnect
        public void Disconnect()
        {
            if (!IsConnected) return;
            ConnectionToServer.Disconnect();
        }


        // Sends data to the current connection
        public void SendData<T>(T objectForTransfer, string typeOfObject)
        {
            if (!IsConnected) return;

            ConnectionToServer.SendData(objectForTransfer, typeOfObject);
        }

        // Tells the current connection to start receiving data
        public void StartReceivingData()
        {
            if (!IsConnected) return;
            ConnectionToServer.StartReceivingData();
        }


        // Controls what happens when the current connection is lost
        private void ConnectionLostHandler(object s, EventArgs e)
        {
            CurrentRemoteEndPoint = null;
            CurrentValidatedSslStream = null;
            CurrentClientSocket = null;
            ConnectionToServer.Dispose();
            ConnectionToServer = null;
            IsConnected = false;
            ConnectionLost.Invoke(this, null);
            GC.Collect();
        }

        // Handles new data when it has been received
        private void HandleFullyReceivedPackage(object sender, EventArgs e)
        {
            OnNewDataFullyReceived.Invoke(this, e);
        }

        // Creates a new authenticated SSlStream from a socket
        private SslStream CreateAndValidateSslStream(Socket socket)
        {
            SslStream validatedSslStream = new SslStream(new NetworkStream(socket), false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            validatedSslStream.AuthenticateAsClient(AddressFamily.InterNetwork.ToString());
            return validatedSslStream;
        }

        // Validation between the client and server certificates
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return ClientX509Certificate.Equals(certificate);
        }
    }
}