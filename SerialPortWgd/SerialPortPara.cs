using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace SerialPortWgd
{
    public class SerialPortPara
    {
        private Parity parity;

        public Parity Parity
        {
            get { return parity; }
            set { parity = value; }
        }

        private Int32 baudRate;

        public Int32 BaudRate
        {
            get { return baudRate; }
            set { baudRate = value; }
        }
        private Int32 dataBits;

        public Int32 DataBits
        {
            get { return dataBits; }
            set { dataBits = value; }
        }
        private StopBits stopBits;

        public StopBits StopBits
        {
            get { return stopBits; }
            set { stopBits = value; }
        }
        private string portName;

        public string PortName
        {
            get { return portName; }
            set { portName = value; }
        }
    }
}
