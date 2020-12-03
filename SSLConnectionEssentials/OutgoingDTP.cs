using LoggerDebug;
using System;
using System.Net.Security;
using System.Text;


namespace SSLConnectionEssentials
{
    public class OutgoingDTP : IDisposable
    {
        // Returns whether the resources that the object uses are freed;
        public bool IsDisposed { get; private set; }
        
        // Represents the maximum size for each packet
        public static int PacketMaxSize { get; set; } = 65536;
        
        // Represents the header of the whole package
        private byte[] PackageHeader { get; set; }

        // Represents the byte array when it's split into packets
        private byte[][] SplitArray { get; set; }

        // Represents the type of the object before it was converted to a byte array
        private string TypeOfTheObjectBeforeSerialization { get; set; }

        // Represents the serialized object we should split
        private byte[] ByteArray { get; set; }
        
        // Represents an external delegate that controls what happens when data sending has failed
        public EventHandler OnSendFailed { get; set; }
        
        
        // Creates the data transmission protocol
        public OutgoingDTP(byte[] inputByteArray, string typeOfTheObjectBeforeSerialization)
        {
            this.ByteArray = inputByteArray;
            this.TypeOfTheObjectBeforeSerialization = typeOfTheObjectBeforeSerialization;
            this.SplitArray = ByteArraySplitter.Split(inputByteArray, PacketMaxSize);
            this.PackageHeader = FormHeader();
        }
        
        
        // Create a byte array header with info about the whole package
        private byte[] FormHeader()
        {
            string objectType =
                AddEmptySpacesToStringUntilReachedDesiredSize(TypeOfTheObjectBeforeSerialization, 20);
            string byteArraySize = AddEmptySpacesToStringUntilReachedDesiredSize(ByteArray.Length.ToString(), 10);
            string splitArraySize = AddEmptySpacesToStringUntilReachedDesiredSize(SplitArray.Length.ToString(), 10);
            string maxPacketSize = AddEmptySpacesToStringUntilReachedDesiredSize(PacketMaxSize.ToString(), 5);
            string lastPacketSize =
                AddEmptySpacesToStringUntilReachedDesiredSize(SplitArray[SplitArray.Length - 1].Length.ToString(), 5);
            string headerBeforeSerialization =
                $"{objectType}${byteArraySize}${splitArraySize}${maxPacketSize}${lastPacketSize}";
            return Encoding.UTF8.GetBytes(headerBeforeSerialization);
        }
        
        // Starts writing data to a given data stream from a given socket
        public void Write(SslStream dataStream)
        {
            ExceptionHandler(() =>
            {
                dataStream.Write(PackageHeader, 0, PackageHeader.Length);
                dataStream.Write(SplitArray[SplitArray.Length - 1], 0, SplitArray[SplitArray.Length - 1].Length);
                for (int i = 0; i < SplitArray.Length - 1; i++)
                {
                    dataStream.Write(SplitArray[i], 0, SplitArray[i].Length);
                }
            });
        }

        // Adds empty spaces to a string until it reaches a given size 
        private string AddEmptySpacesToStringUntilReachedDesiredSize(string stringToAddTo, int desiredSize)
        {
            if (stringToAddTo.Length == desiredSize)
            {
                return stringToAddTo;
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(stringToAddTo);
                while (builder.Length < desiredSize)
                {
                    builder.Append(" ");
                }

                return builder.ToString();
            }
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
                Logger.Log(e, false);
                Logger.Log("OutgoingDTP error",false);
                OnSendFailed.Invoke(this, null);
            }
        }

        // Dispose of the resources used by the object
        public void Dispose()
        {
            if (!IsDisposed)
            {
                ByteArray = null;
                TypeOfTheObjectBeforeSerialization = null;
                SplitArray = null;
                PackageHeader = null;
                OnSendFailed = null;
                IsDisposed = true;
            }
        }
    }
}