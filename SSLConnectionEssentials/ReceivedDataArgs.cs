using System;
using System.Collections.Generic;
using System.Text;

namespace SSLConnectionEssentials
{
    public class ReceivedDataArgs:EventArgs
    {
        private Type ReceivedType { get; set; }
        private object ReceivedObject { get; set; }
        public ReceivedDataArgs(Type type,object receivedObject)
        {
            ReceivedType = type;
            ReceivedObject = receivedObject;
        }

        public dynamic GetReceivedObject()
        {
            return Convert.ChangeType(ReceivedObject, ReceivedType);
        }
    }
}
