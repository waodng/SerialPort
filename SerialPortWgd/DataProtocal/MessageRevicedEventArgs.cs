using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SerialPortWgd.DataProtocal
{
    public class MessageRevicedEventArgs:EventArgs
    {
        public string MsgType { get; set; }
        public byte[] Contents { get; set; }
        public Socket SocketHandler { get; set; }
    }

    public enum DataType
    {
        DCIData = 1,
        XiEr = 2
    }
}
