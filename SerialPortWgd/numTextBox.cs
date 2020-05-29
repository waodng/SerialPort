using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SerialPortWgd
{
    public partial class numTextBox : UserControl
    {
        public numTextBox()
        {
            InitializeComponent();
            this.txtContent.MouseWheel += new MouseEventHandler(txtContect_MouseWheel);
        }
        private ToolStripLabel _toolStripLbl = null;
        public ToolStripLabel ToolStripLbl
        {
            get { return _toolStripLbl; }
            set { _toolStripLbl = value; }
        }
        private int pageLine = 0;
        private void txtContect_TextChanged(object sender, EventArgs e)
        {
            //调用顺序不可变
            SetScrollBar();
            ShowRow();
            ShowCursorLine();
        }

        //鼠标滚动
        void txtContect_MouseWheel(object sender, MouseEventArgs e)
        {
            timer1.Enabled = true;
        }

        // 上、下键
        private void txtContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == System.Windows.Forms.Keys.Up || e.KeyData == System.Windows.Forms.Keys.Down)
                SetScrollBar();

        }
        private void txtContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == System.Windows.Forms.Keys.Up || e.KeyData == System.Windows.Forms.Keys.Down)
                ShowCursorLine();
        }

        //点击滚动条
        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            int t = SetScrollPos(this.txtContent.Handle, 1, vScrollBar1.Value, true);
            SendMessage(this.txtContent.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * vScrollBar1.Value, 0);
            ShowRow();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            ShowRow();
        }

        //显示光标行
        private void txtContent_MouseDown(object sender, MouseEventArgs e)
        {
            ShowCursorLine();
        }

        //文本框大小改变
        private void txtContent_SizeChanged(object sender, EventArgs e)
        {
            SCROLLINFO si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = SIF_ALL;
            int r = GetScrollInfo(this.txtContent.Handle, SB_VERT, ref si);
            pageLine = (int)si.nPage;
            timer1.Enabled = true;
            ShowRow();
        }

        //行显示栏宽度自适应
        private void txtRow_TextChanged(object sender, EventArgs e)
        {
            if (this.txtRow.Lines.Length > 0)
            {
                System.Drawing.SizeF s = this.txtRow.CreateGraphics().MeasureString(this.txtRow.Lines[this.txtRow.Lines.Length - 1], this.txtRow.Font);
                this.txtRow.Width = (int)s.Width;
            }
        }

        private void txtRow_SizeChanged(object sender, EventArgs e)
        {
            this.txtContent.Location = new Point(this.txtRow.Width, this.txtContent.Location.Y);
            this.txtContent.Width = this.ClientSize.Width - this.txtRow.Width;
        }

        #region Method
        private void ShowCursorLine()
        {
            ToolStripLbl.Text = "行: " + (this.txtContent.GetLineFromCharIndex(this.txtContent.SelectionStart) + 1);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SetScrollBar();
            timer1.Enabled = false;
        }
        /// <summary>
        /// 鼠标双击行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRow_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.txtRow.SelectionBackColor = Color.Blue;
            this.txtRow.SelectionColor = Color.White;
        }

        private void SetScrollBar()
        {
            SCROLLINFO si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = SIF_ALL;
            int r = GetScrollInfo(this.txtContent.Handle, SB_VERT, ref si);
            pageLine = (int)si.nPage;
            this.vScrollBar1.LargeChange = pageLine;

            if (si.nMax >= si.nPage)
            {
                this.vScrollBar1.Visible = true;
                this.vScrollBar1.Maximum = si.nMax;
                this.vScrollBar1.Value = si.nPos;
            }
            else
                this.vScrollBar1.Visible = false;
        }

        private void ShowRow()
        {
            int firstLine = txtContent.GetLineFromCharIndex(txtContent.GetCharIndexFromPosition(new Point(0, 2)));
            string[] lin = new string[pageLine];
            for (int i = 0; i < pageLine; i++)
            {
                lin[i] = (i + firstLine + 1).ToString();
            }

            txtRow.Lines = lin;
        }

        /// <summary>
        /// 选中行
        /// </summary>
        /// <param name="line">行号,从0开始</param>
        private void selectLine(int line)
        {
            int a = this.txtContent.GetFirstCharIndexFromLine(line);
            int b = this.txtContent.GetFirstCharIndexFromLine(++line);
            if (a == -1)
                return;
            else if (b == -1)
                b = this.txtContent.TextLength - a;
            else
                b = b - a;
            this.txtContent.Select(a, b);
        }


        public int TextLength
        {
            get
            {
                return this.txtContent.TextLength;
            }
        }
        /// <summary>
        /// richTextBox文本框中添加字符串
        /// </summary>
        /// <param name="text"></param>
        public void AppendText(string text)
        {
            this.txtContent.AppendText(text);
        }
        /// <summary>
        /// 清理文本
        /// </summary>
        public void Clear()
        {
            this.txtContent.Clear();
        }


        #endregion

        #region API 调用

        public static uint SIF_RANGE = 0x0001;
        public static uint SIF_PAGE = 0x0002;
        public static uint SIF_POS = 0x0004;
        public static uint SIF_TRACKPOS = 0x0010;
        public static uint SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS);
        public int SB_THUMBPOSITION = 4;
        public int SB_VERT = 1;
        public int WM_VSCROLL = 0x0115;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetScrollInfo(IntPtr hwnd, int bar, ref SCROLLINFO si);

        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hwnd, int nbar);

        [DllImport("user32.dll")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool Rush);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        #endregion        

        

        
    }
}
