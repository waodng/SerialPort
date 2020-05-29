using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialPortWgd
{
     public class  wTextBox : System.Windows.Forms.TextBox
    {
         public wTextBox():base()
         {
             this.BorderStyle = System.Windows.Forms.BorderStyle.None;
             this.Multiline = true;
             
         }
         protected override bool IsInputKey(System.Windows.Forms.Keys KeyData)
         {
             if (KeyData == System.Windows.Forms.Keys.Up || KeyData == System.Windows.Forms.Keys.Down)
                 return true;
             return base.IsInputKey(KeyData);
         }
    }
}
