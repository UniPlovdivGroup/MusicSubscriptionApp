using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using Serialization;
using LoggerDebug;
using System.Threading;

namespace SSLConnectionEssentials
{
    public class Connection : IDisposable
    {
        // Represents whether the resources of the object are disposed
        public bool IsDisposed { get; private set; }

        // Represents the core socket of the connection
        private Socket ConnectionSocket { get; set; }

        // Represents the stream through which data is transported
        private SslStream DataStream { get; set; }

        // Represents the current data transmission protocol which handles all the data that's coming
        private IncomingDTP CurrentIncomingDtp { get; set; }

        // Represents the temporary buffer where data is recorded upon arrival
        private byte[] DataReceivalBuffer { get; set; }

        // Represents an external delegate that declares what happens when the connection is closed or lost 
        public EventHandler OnConnectionLost { get; set; }

        public EventHandler OnNewDataFullyReceived { get; set; }

        // Constructor of the connection
        public Connection(Socket connectionSocket, SslStream authenticatedSslStream)
        {
            this.ConnectionSocket = connectionSocket;
            this.DataStream = authenticatedSslStream;
        }


        // Tells the remote host that it will disconnect and disconnects
        public void Disconnect()
        {
            ExceptionHandler(() =>
            {
                OnConnectionLost.Invoke(this, null);
            });
        }

        // Writes a byte array to the data stream
        public void SendData<T>(T objectForTransfer, string typeOfTheByteArrayBeforeSerialization)
        {
            ExceptionHandler(() =>
            {
                byte[] toSend = JsonSerialization.JsonObjectSerialize(objectForTransfer);
                OutgoingDTP dtp = new OutgoingDTP(toSend, typeOfTheByteArrayBeforeSerialization);
                dtp.OnSendFailed += OnSendFailedHandler;
                dtp.Write(DataStream);
            });
        }


        // Tells the connection to start reading from the data stream data that is written from the remote host
        public void StartReceivingData()
        {
            ExceptionHandler(() =>
            {
                if (CurrentIncomingDtp == null)
                {
                    DataReceivalBuffer = new byte[IncomingDTP.HeaderSize];
                    DataStream.BeginRead(DataReceivalBuffer, 0, DataReceivalBuffer.Length,
                        new AsyncCallback(ReceiveCallback), ConnectionSocket);
                }
            });
        }

        // Represents what happens after data is read from the data stream 
        private void ReceiveCallback(IAsyncResult AR)
        {
            ExceptionHandler(() =>
            {
                int received = DataStream.EndRead(AR);
                byte[] dataBuf = new byte[received];
                if (received <= 0)
                {
                    throw new SocketException(10054);
                }
                Array.Copy(DataReceivalBuffer, dataBuf, received);
                CurrentIncomingDtp = new IncomingDTP(dataBuf, DataStream, ConnectionSocket);
                CurrentIncomingDtp.OnReceiveFailed += OnReceiveFailedHandler;
                CurrentIncomingDtp.OnDataFullyReceived += HandleFullyReceivedPackage;
                CurrentIncomingDtp.StartReadingData();
            });
        }


        // Handles what happens whenever an exception occurs in the object
        private void ExceptionHandler(Action MethodToHandle)
        {
            if (IsDisposed)
            {
                return;
            }

            try
            {
                MethodToHandle();
            }
            catch (Exception e)
            {
                Logger.Log(e,false);
                Logger.Log("Connection error",false);
                OnConnectionLost.Invoke(this, null);
            }
        }

        // Controls what happens when data sending has failed
        private void OnSendFailedHandler(object s, EventArgs e)
        {
            OnConnectionLost.Invoke(this, null);
        }

        // Controls what happens when data receival has failed
        private void OnReceiveFailedHandler(object s, EventArgs e)
        {
            OnConnectionLost.Invoke(this, null);
        }

        // Controls what happens when a package is fully received
        private void HandleFullyReceivedPackage(object sender, EventArgs e)
        {
            byte[] fullyReceivedByteArray = CurrentIncomingDtp.OutputByteArray;
            string typeOfObjectBeforeDeseriliazaition = CurrentIncomingDtp.ReceivedType;
            switch (typeOfObjectBeforeDeseriliazaition)
            {
                case "DataSent":
                    {
                        DataToSend reconstructed =
                           JsonSerialization.JsonObjectDeserialize<DataToSend>(fullyReceivedByteArray);
                        OnNewDataFullyReceived.Invoke(this, new ReceivedDataArgs(typeof(DataToSend), reconstructed));
                        break;
                    }
            }

            typeOfObjectBeforeDeseriliazaition = null;
            fullyReceivedByteArray = null;

            CurrentIncomingDtp.Dispose();
            CurrentIncomingDtp = null;
            GC.Collect();
            StartReceivingData();
        }



        // Disposes of the used resources
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Logger.Log("Disposing of the current connection to free resources",false);
                if (!(CurrentIncomingDtp == null))
                {
                    CurrentIncomingDtp.Dispose();
                    CurrentIncomingDtp = null;
                }
                DataReceivalBuffer = null;
                DataStream.Close();
                DataStream.Dispose();
                DataStream = null;
                ConnectionSocket.Close();
                ConnectionSocket.Dispose();
                ConnectionSocket = null;
                OnConnectionLost = null;
                IsDisposed = true;
            }
            else
            {
                Logger.Log("Already disposed",false);
            }
        }
    }
}