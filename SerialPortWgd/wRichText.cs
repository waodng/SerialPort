using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialPortWgd
{
    public class wRichText : System.Windows.Forms.RichTextBox
    {
        public wRichText()
            : base()
         {
             this.BorderStyle = System.Windows.Forms.BorderStyle.None;
             this.Dock = System.Windows.Forms.DockStyle.None;

             this.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
             | System.Windows.Forms.AnchorStyles.Left)
             | System.Windows.Forms.AnchorStyles.Right)));
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
