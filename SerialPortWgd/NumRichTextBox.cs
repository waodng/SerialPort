using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SerialPortWgd
{
    public partial class NumRichTextBox : UserControl
    {
        #region  属性

        public int TextLength
        {
            get
            {
                return this.richTextData.TextLength;
            }
        }

        #endregion

        #region  构造函数
        public NumRichTextBox()
        {
            InitializeComponent();

            lblNum.Font = new Font(richTextData.Font.FontFamily, richTextData.Font.Size + 1.01f);
        }

        #endregion

        #region  事件

        private void richTextData_TextChanged(object sender, EventArgs e)
        {
            updateNumberLabel();
        }

        private void richTextData_VScroll(object sender, EventArgs e)
        {
            //move location of numberLabel for amount of pixels caused by scrollbar
            int d = richTextData.GetPositionFromCharIndex(0).Y % (richTextData.Font.Height + 1);
            richTextData.Location = new Point(0, d);

            updateNumberLabel();
        }

        private void richTextData_Resize(object sender, EventArgs e)
        {
            richTextData_VScroll(null, null);
        }

        private void richTextData_FontChanged(object sender, EventArgs e)
        {
            richTextData_VScroll(null, null);
        }
        #endregion

        #region 方法
        /// <summary>
        /// richTextBox文本框中添加字符串
        /// </summary>
        /// <param name="text"></param>
        public void AppendText(string text)
        {
            this.richTextData.AppendText(text);
        }
        /// <summary>
        /// 清理文本
        /// </summary>
        public void Clear()
        {
            this.richTextData.Clear();
        }

        private void updateNumberLabel()
        {
            //we get index of first visible char and number of first visible line
            Point pos = new Point(0, 0);
            int firstIndex = richTextData.GetCharIndexFromPosition(pos);
            int firstLine = richTextData.GetLineFromCharIndex(firstIndex);

            //now we get index of last visible char and number of last visible line
            pos.X = ClientRectangle.Width;
            pos.Y = ClientRectangle.Height;
            int lastIndex = richTextData.GetCharIndexFromPosition(pos);
            int lastLine = richTextData.GetLineFromCharIndex(lastIndex);

            pos = richTextData.GetPositionFromCharIndex(lastIndex);

            lblNum.Text = "";
            for (int i = firstLine; i <= lastLine + 1; i++)
            {
                lblNum.Text += i + 1 + "\n";
            }
        }

        #endregion

    }
}
