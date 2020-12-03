using LoggerDebug;
using System;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace SSLConnectionEssentials
{
    public class IncomingDTP : IDisposable
    {
        // Returns whether the resources that the object uses are freed;
        public bool IsDisposed { get; private set; }

        // Represents the universal header size
        // Set according to the header size you need
        public static int HeaderSize { get; set; } = 54;

        // Represents the parent socket of the data stream
        private Socket ConnectionSocket { get; set; }

        // Represents the data stream through which we send data
        private SslStream DataStream { get; set; }

        // A buffer where received data is stored
        private byte[] DataReceivalBuffer { get; set; }

        // Represents the next incoming body size to read from the stream
        private int ExpectedIncomingPacketSize { get; set; }

        // Represents the whole final array size
        private int TotalArraySize { get; set; }

        // Represents the size of every packet except the last one
        private int DefaultPacketSize { get; set; }

        // Represents the size of the last package
        private int LastPacketSize { get; set; }

        // Represents the last array in the package
        private byte[] LastArray { get; set; }

        // Represents the final output byte array
        public byte[] OutputByteArray { get; private set; }

        // Represents the type of the received serialized output array
        public string ReceivedType { get; private set; }

        // Represents an external delegate that declares what happens once the output array is fully received
        public EventHandler OnDataFullyReceived { get; set; }

        // Represents an external delegate that declares what happens if the receival of data has failed
        public EventHandler OnReceiveFailed { get; set; }


        // Initializes the incoming dtp based on a received header, deserializes it,
        // reads info from it and creates the output array,
        // also determines the serialized object type
        public IncomingDTP(byte[] firstHeader, SslStream dataStream, Socket connectionSocket)
        {
            this.DataStream = dataStream;
            this.ConnectionSocket = connectionSocket;
            ReadHeader(firstHeader);
            OutputByteArray = new byte[0];
            LastArray = new byte[LastPacketSize];
            ExpectedIncomingPacketSize = LastPacketSize;
        }


        // Deserializes the header and reads info from it
        private void ReadHeader(byte[] headerByteArray)
        {
            //$"{objectType}${byteArraySize}${splitArraySize}${maxPacketSize}${lastPacketSize}";
            string decodedHeader = Encoding.UTF8.GetString(headerByteArray);
            string[] insides = decodedHeader.Split('$');
            for (int i = 0; i < insides.Length; i++)
            {
                insides[i] = insides[i].Trim();
            }

            ReceivedType = insides[0];
            TotalArraySize = int.Parse(insides[1]);
            DefaultPacketSize = int.Parse(insides[3]);
            LastPacketSize = int.Parse(insides[4]);

        }

        // Hijacks given data stream and socket and starts reading data
        public void StartReadingData()
        {
            ExceptionHandler(() =>
            {
                DataReceivalBuffer = new byte[ExpectedIncomingPacketSize];
                DataStream.BeginRead(DataReceivalBuffer, 0, DataReceivalBuffer.Length,
                    new AsyncCallback(ReceiveCallback), ConnectionSocket);
            });
        }

        // Controls what to happen on successfully received data
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
                else
                {
                    Array.Copy(DataReceivalBuffer, dataBuf, received);
                    if (ExpectedIncomingPacketSize == LastPacketSize)
                    {
                        Array.Copy(dataBuf, LastArray, LastPacketSize);
                        ExpectedIncomingPacketSize = DefaultPacketSize;
                    }
                    else
                    {
                        ConcatToOutputArray(dataBuf);
                    }
                    if (OutputByteArray.Length == TotalArraySize - LastPacketSize)
                    {
                        ConcatToOutputArray(LastArray);
                        OnDataFullyReceived.Invoke(this, null);
                    }
                    else StartReadingData();
                }
            });
        }

        // Concats given byte array to the output array
        private void ConcatToOutputArray(byte[] smallByteArray)
        {
            OutputByteArray = OutputByteArray.Concat(smallByteArray).ToArray();
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
                Logger.Log("IncomingDTP error",false);
                OnReceiveFailed.Invoke(this, null);
            }
        }

        //Dispose of the resources used by the object and set the IsDisposed variable to true 
        public void Dispose()
        {
            if (!IsDisposed)
            {
                DataReceivalBuffer = null;
                LastArray = null;
                OutputByteArray = null;
                ReceivedType = null;
                OnDataFullyReceived = null;
                OnReceiveFailed = null;
                IsDisposed = true;
            }
        }
    }
}