using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialPortWgd
{
    /// <summary>
    /// ListItem用于ComboBox控件添加项
    /// </summary>
    public class ListItem
    {
        public string Text
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this.Text;
        }
    }
}
