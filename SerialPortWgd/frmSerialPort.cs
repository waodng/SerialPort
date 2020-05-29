using SerialPortWgd.DataProtocal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace SerialPortWgd
{
    /// <summary>
    /// 串口
    /// </summary>
    public partial class frmSerialPort : Form
    {
        #region 属性
        private System.Timers.Timer sendTimer = new System.Timers.Timer();
        private System.Timers.Timer sendNetTimer = new System.Timers.Timer();
        private SerialPort ComDevice = new SerialPort();
        private int receCnt = 0;
        private int sendCnt = 0;
        private static int recNum = 0;
        private const int SB_VERT = 0x1;
        private const int WM_VSCROLL = 0x115;
        private const int SB_THUMBPOSITION = 4;
        //记录网络发送区文本框的起始位置
        private static int NetSendPosition = 0;
        private string strTargetIp = "";
        private int strTargetPort = 0;
        //网络发送多条数据定义
        private static Queue<string> lstSendData = new Queue<string>();
        private static object objLock = new object();
        #endregion
        public frmSerialPort()
        {
            InitializeComponent();
            this.init();
        }
        /// <summary>
        /// 加载主窗体初始化
        /// </summary>
        public void init()
        {
            #region 串口设置
            btnSend.Enabled = false;
            cbbComList.Items.AddRange(GetPortNames());
            cbbComList.SelectedIndex = 0;
            cbbBaudRate.SelectedIndex = 5;
            cbbDataBits.SelectedIndex = 0;
            cbbParity.SelectedIndex = 0;
            cbbStopBits.SelectedIndex = 0;
            cbbStopBits.DisplayMember = "Text";
            cbbStopBits.ValueMember = "Value"; 
            #endregion

            #region 网络初始化设置

            this.NetType.SelectedIndex = 0;
            btnSendNet.Enabled = false;
            this.txtLocalIp.Text= GetRealIP();
            this.txtLocalPort.Text = Properties.Settings.Default.NetSendPort;
            this.txtSendDataArea.Text = Properties.Settings.Default.NetSendData;
            string[] strDatas = Properties.Settings.Default.NetSendDataMore.Split('◐');
            string[] strChkDatas = Properties.Settings.Default.NetSendDataMoreSelect.Split('◐');
          
            for (int i = 0; i < strDatas.Length; i++)
            {
                TextBox txtCurent = this.splitContainer4.Controls.Find("txtData" + i,true)[0] as TextBox;
                CheckBox IsSend = this.splitContainer4.Controls.Find("chkData" + i, true)[0] as CheckBox;
                if (txtCurent!=null)
                {
                    txtCurent.Text = strDatas[i];
                }
                if (IsSend != null)
                {
                    IsSend.Checked = strChkDatas[i] == "True" ? true : false;
                }
            }
            #endregion

            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.btnSend, "按Ctrl+Enter发送");

            this.toolStripStatusLabel1.Text = "当前系统时间：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//绑定事件
            
        }
      
        #region 事件
        /// <summary>
        /// 打开关闭串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblOpenSerial_Click(object sender, EventArgs e)
        {
            if (cbbComList.Items.Count <= 0)
            {
                MessageBox.Show("没有发现串口,请检查线路！");
                return;
            }

            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = cbbComList.SelectedItem.ToString();
                ComDevice.BaudRate = Convert.ToInt32(cbbBaudRate.SelectedItem.ToString());
                ComDevice.Parity = (Parity)Convert.ToInt32(cbbParity.SelectedIndex.ToString());
                ComDevice.DataBits = Convert.ToInt32(cbbDataBits.SelectedItem.ToString());
                ComDevice.StopBits = (StopBits)Convert.ToInt32(cbbStopBits.SelectedValue.ToString());
                try
                {
                    ComDevice.Open();
                    this.chkCycles.Enabled = true;
                    btnSend.Enabled = true;
                    this.txtShowData.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                lblOpenSerial.Text = "断开";
                this.lblOpenSerial.Image = SerialPortWgd.Properties.Resources.red;
            }
            else
            {
                try
                {
                    ComDevice.Close();
                    this.chkCycles.Checked = false;
                    this.chkCycles.Enabled = false;
                    btnSend.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                lblOpenSerial.Text = "连接";
                this.lblOpenSerial.Image = SerialPortWgd.Properties.Resources.black;
            }

            cbbComList.Enabled = !ComDevice.IsOpen;
            cbbBaudRate.Enabled = !ComDevice.IsOpen;
            cbbParity.Enabled = !ComDevice.IsOpen;
            cbbDataBits.Enabled = !ComDevice.IsOpen;
            cbbStopBits.Enabled = !ComDevice.IsOpen;
        }

        /// <summary>
        /// 发送数据button事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            string strSendData = txtSendData.Text.Trim();
            if (strSendData.Length == 0) return;
            byte[] sendData = null;

            #region  自动发送设置
            if (chkAutoSend.Checked)
            {
                if (sender != null)
                {
                    if (btnSend.Text == "停止发送")
                    {
                        btnSend.Text = "手动发送";
                        this.txtSendData.Enabled = true;
                        lblOpenSerial.Enabled = true;
                        this.txtInterval.Enabled = true;
                        sendTimer.Stop();
                        if (chkSendBClear.Checked)
                        {
                            this.txtSendData.Clear();
                        }
                        return;
                    }
                    else if (btnSend.Text == "手动发送")
                    {
                        btnSend.Text = "停止发送";
                        this.txtSendData.Enabled = false;
                        lblOpenSerial.Enabled = false;
                        this.txtInterval.Enabled = false;
                        sendTimer.Interval = Int32.Parse(txtInterval.Text.Trim());
                        sendTimer.Elapsed += sendTimer_Elapsed;
                        sendTimer.Start();
                    }
                }
            }
            #endregion

            if (rbtnSendHex.Checked)
            {
                sendData = HexToByte(strSendData);
            }
            else if (rbtnSendASCII.Checked)
            {
                sendData = Encoding.ASCII.GetBytes(strSendData);
            }
            else if (rbtnSendUTF8.Checked)
            {
                sendData = Encoding.UTF8.GetBytes(strSendData);
            }
            else if (rbtnSendUnicode.Checked)
            {
                sendData = Encoding.Unicode.GetBytes(strSendData);
            }
            else
            {
                sendData = Encoding.ASCII.GetBytes(strSendData);
            }

            if (this.SendData(sendData))//发送数据成功计数
            {
                if (chkSendBClear.Checked && !chkAutoSend.Checked)
                {
                    this.txtSendData.Clear();
                }
                sendCnt += txtSendData.Text.Length;
                statusStrip1.Items["toolStripStatusSend"].Text = "发送：" + sendCnt.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sendTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (chkAutoSend.Checked)
            {
                btnSend_Click(null, e);
            }
        }

        /// <summary>
        /// 自动发送选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAutoSend_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                string strSendData = txtSendData.Text.Trim();
                if (strSendData.Length == 0) return;

                if (!chkAutoSend.Checked && btnSend.Text == "停止发送")
                {
                    btnSend.Text = "手动发送";
                    this.txtSendData.Enabled = true;
                    lblOpenSerial.Enabled = true;
                    this.txtInterval.Enabled = true;
                    sendTimer.Stop();
                    if (chkSendBClear.Checked)
                    {
                        this.txtSendData.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        

        /// <summary>
        /// 清空发送区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblClearSend_Click(object sender, EventArgs e)
        {
            txtSendData.Clear();
        }
        /// <summary>
        /// 清空接收区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblClearRece_Click(object sender, EventArgs e)
        {
            txtShowData.Clear();
        }

        /// <summary>
        /// 系统时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "当前系统时间：" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        /// <summary>
        /// 复位计数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {
            receCnt = 0;
            sendCnt = 0;
            this.toolStripStatusReceive.Text = "接收：0";
            this.toolStripStatusSend.Text = "发送：0";
        }

        /// <summary>
        /// 接收区全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtShowData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                ((TextBox)sender).SelectAll();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 发送区全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSendData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                ((TextBox)sender).SelectAll();
            }
        }
        /// <summary>
        /// 接收数据保存到文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblSaveReve_Click(object sender, EventArgs e)
        {
            try
            {
                ShowSaveFileDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// 从文件载入数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblFileInput_Click(object sender, EventArgs e)
        {
            try
            {
                ShowOpenFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 快捷建处理
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control|Keys.Enter))
            {
                btnSend.PerformClick();
                return true;
            }
            if (keyData == (Keys)Shortcut.CtrlF)
            {
                MessageBox.Show("ctrl+F");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData); // 其他键按默认处理　
        }

        /// <summary>
        /// 鼠标双击托盘图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示    
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                notifyIcon1.Visible = false;
            }
        }

        /// <summary>
        /// 发送区16进制转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendHex_CheckedChanged(object sender, EventArgs e)
        {
            string strData = txtSendData.Text.Trim();
            if (rbtnSendHex.Checked && strData.Length > 0)
            {
                this.txtSendData.Text = StringToHexString(strData, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 选择发送区ascii转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendASCII_CheckedChanged(object sender, EventArgs e)
        {
            string strData = txtSendData.Text.Trim();
            if (rbtnSendASCII.Checked && strData.Length > 0)
            {
                List<byte> lstData = Encoding.ASCII.GetBytes(strData).ToList();

                this.txtSendData.Text = string.Join(" ", lstData.ToArray());
            }
        }

        /// <summary>
        /// 串口发送区unicode格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendUnicode_CheckedChanged(object sender, EventArgs e)
        {
            string strData = txtSendData.Text.Trim();
            if (rbtnSendUnicode.Checked && strData.Length > 0)
            {
                this.txtSendData.Text = System.Text.Encoding.Unicode.GetString(System.Text.Encoding.UTF8.GetBytes(strData));
            }
        }
        /// <summary>
        /// 串口发送区UTF8格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendUTF8_CheckedChanged(object sender, EventArgs e)
        {
            string strData = txtSendData.Text.Trim();
            if (rbtnSendUnicode.Checked && strData.Length > 0)
            {
                this.txtSendData.Text = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(strData));
            }
        }

        /// <summary>
        /// 主程序关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmSerialPort_FormClosing(object sender, FormClosingEventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
        }
        /// <summary>
        /// 窗体大小改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmSerialPort_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                notifyIcon1.Visible = true;
            }
        }

        private void cbbComList_Click(object sender, EventArgs e)
        {
            cbbComList.Items.Clear();
            cbbComList.Items.AddRange(GetPortNames());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkCycles_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (chkCycles.Checked)
                {
                    this.chkAutoLine.Checked = true;
                    this.txtShowData.Clear();
                    string strCom = cbbComList.SelectedItem.ToString();

                    #region  线程开启
                    
                    //开启一个线程
                    System.Threading.ThreadPool.QueueUserWorkItem((a) =>
                    {
                        ChangeSerialArag(handleData(strCom).OrderBy(r=>r.BaudRate).ToList());
                        //handleData(strCom).OrderBy(r => r.BaudRate).ToList();
                    });
                    #endregion
                }
                else
                {
                    this.chkAutoLine.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbbDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbDataBits.SelectedItem.ToString() == "5")
            {
                cbbStopBits.DataSource = getStopBit().FindAll(r => r.Text.Contains('1'));
            }
            else
            {
                cbbStopBits.DataSource = getStopBit().FindAll(r => !r.Text.Contains('.'));
            }
        }


        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strMes = @"                            串口调试助手（v1.9）
---------------------------------------------------------------------------------
本工具是在实际工程应用中，根据实际的普遍的需求而开发的串口调试工具。
界面精致美观，实用性也强。
支持各种串口设置，如波特率，校验位、数据位和停止位等等。
支持ASCII/Hex等发送,发送和接收的数据可以在16进制和AscII码之间任意转换。
可以自动在发送的数据尾增加校验位，支持多种校验格式。
支持间隔发送，循环发送，批处理发送，输入数据可以从外部文件导入。


                                                                waodng
                                                                2019.09.09";
            MessageBox.Show(strMes,"帮助");
        }
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region 值保存

            try
            {
                Properties.Settings.Default.NetSendTargetPort = this.txtTargetPort.Text.Trim();
                Properties.Settings.Default.NetSendData = this.txtSendDataArea.Text.Trim();

                List<string> strDatas = new List<string>();
                List<string> strChkDatas = new List<string>();
                for (int i = 0; i < 10; i++)
                {
                    TextBox txtCurent = this.splitContainer4.Controls.Find("txtData" + i, true)[0] as TextBox;
                    CheckBox IsSend = this.splitContainer4.Controls.Find("chkData" + i, true)[0] as CheckBox;
                    if (txtCurent != null)
                    {
                        strDatas.Add(txtCurent.Text.Trim());
                    }
                    if (IsSend != null)
                    {
                        strChkDatas.Add(IsSend.Checked.ToString());
                    }
                }
                Properties.Settings.Default.NetSendDataMore = string.Join("◐", strDatas.ToArray());
                Properties.Settings.Default.NetSendDataMoreSelect = string.Join("◐",strChkDatas.ToArray());
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // 关闭所有的线程
                this.Dispose();
                this.Close();
            }
            #endregion
        }

        /// <summary>
        /// 网络区域选择协议版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (NetType.SelectedIndex == 1)
                {
                    lblOpenNet.Text = "开始监听";
                    lblOpenNet.Padding = new Padding(10, 0, 15, 0);
                }
                else
                {
                    lblOpenNet.Text = "连接";
                    lblOpenNet.Padding = new Padding(20, 0, 20, 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 网络连接开始结束点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblOpenNet_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblOpenNet.Text=="断开")
                {
                    if (NetType.SelectedIndex == 0)//UDP
                    {
                        this.lblOpenNet.Text = "连接";
                        lblOpenNet.Padding = new Padding(20, 0, 20, 0);
                        this.pnlTarget.Visible = false;
                        //发送按钮停止
                        if (this.btnSendNet.Text == "停止发送")
                        {
                            btnSendNet_Click(null, null);
                        }
                        
                        //服务停止
                        UDPService.Stop();
                    }
                    else if (NetType.SelectedIndex == 1)//TCP Server
                    {
                        this.lblOpenNet.Text = "开始监听";
                        lblOpenNet.Padding = new Padding(10, 0, 15, 0);


                    }
                    else if (NetType.SelectedIndex == 2)//TCP Client
                    {
                        this.lblOpenNet.Text = "连接";
                        lblOpenNet.Padding = new Padding(20, 0, 20, 0);

                    }

                    this.NetType.Enabled = true;
                    this.txtLocalIp.Enabled = true;
                    this.txtLocalPort.Enabled = true;
                    btnSendNet.Enabled = false;
                    this.lblOpenNet.Image = SerialPortWgd.Properties.Resources.black;
                    //多条发送选中取消
                    if (this.chkSendMore.Checked)
                    {
                        this.chkSendMore.Checked = false;
                    }
                   
                }
                else
                {
                    if (NetType.SelectedIndex == 0)//UDP
                    {
                        //端口检查
                        int port = 0;
                        if (!int.TryParse(this.txtLocalPort.Text.Trim(), out port))
                        {
                            ShowStatusMsg("端口不对");
                            return;
                        }

                        //IP地址检查
                        string strIp = this.txtLocalIp.Text;
                        if (UDPService.checkip(strIp))
                        {
                            UDPService.WinPort = port;
                            //启动监听线程            
                            if (!UDPService.Star())
                            {
                                UDPService.Stop();
                            }
                        }
                        else
                        {
                            ShowStatusMsg("IP地址格式不对");
                            return;
                        }

                        UDPService.DataDeal += ReaderData;
                        this.pnlTarget.Visible = true;
                        this.txtTargetIP.Text = strIp;
                        this.txtTargetPort.Text = Properties.Settings.Default.NetSendTargetPort;
                    }
                    else if (NetType.SelectedIndex == 1)//TCP Server
                    {

                    }
                    else if (NetType.SelectedIndex == 2)//TCP Client
                    {

                    }

                    this.lblOpenNet.Text = "断开";
                    this.lblOpenNet.Image = SerialPortWgd.Properties.Resources.red;
                    lblOpenNet.Padding = new Padding(20, 0, 20, 0);
                    this.NetType.Enabled = false;
                    this.txtLocalIp.Enabled = false;
                    this.txtLocalPort.Enabled = false;
                    btnSendNet.Enabled = true;
                    //当用户连接网络时，右边的接收区宽度自动增加，为了多条发送
                    //指令显示正常
                    int rightWidth = this.splitContainer2.Width - this.splitContainer2.SplitterDistance;
                    if (rightWidth < 605)
                    {
                        this.splitContainer2.SplitterDistance = this.splitContainer2.Width - 605;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatusMsg(ex.Message);
                return;
            }
        }

        /// <summary>
        /// 网络接收区清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblClearReceNet_Click(object sender, EventArgs e)
        {
            this.txtRecvDataArea.Clear();
        }
        /// <summary>
        /// 网络发送去清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblClearSendNet_Click(object sender, EventArgs e)
        {
            this.txtSendDataArea.Clear();
        }

        #region 网口数据发送区格式设置
        /// <summary>
        /// 网口发送区ASCII格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendASCIINet_CheckedChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 网口发送区Hex格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendHexNet_CheckedChanged(object sender, EventArgs e)
        {
            string strData = txtSendDataArea.Text.Trim();
            if (rbtnSendHex.Checked && strData.Length > 0)
            {
                this.txtSendDataArea.Text = StringToHexString(strData, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 网口发送区Unicode格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendUnicodeNet_CheckedChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 网口发送区Utf8格式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbtnSendUtf8Net_CheckedChanged(object sender, EventArgs e)
        {

        }

        #endregion
        /// <summary>
        /// 网络发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendNet_Click(object sender, EventArgs e)
        {
            try
            {
                #region 参数验证
                if (sender != null)
                {


                    strTargetIp = this.txtTargetIP.Text.Trim();
                    //检查IP
                    if (UDPService.checkip(strTargetIp))
                    {
                        int port = 0;
                        //端口转化
                        if (!int.TryParse(this.txtTargetPort.Text.Trim(), out port))
                        {
                            this.ShowStatusMsg("端口格式不正确!");
                            return;
                        }

                        if (port > 65535)
                        {
                            this.ShowStatusMsg("端口不能大于65535!");
                            return;
                        }
                        strTargetPort = port;
                    }
                    else
                    {
                        this.ShowStatusMsg("目标IP格式不对!");
                        return;
                    }
                }
                #endregion

                 //停止
                 if (btnSendNet.Text == "停止发送")
                 {
                     btnSendNet.Text = "手动发送";
                     this.txtSendDataArea.Enabled = true;
                     this.txtIntervalNet.Enabled = true;
                     if (sendNetTimer.Enabled)
                     {
                         sendNetTimer.Stop();
                     }
                     
                     if (chkSendBClearNet.Checked)
                     {
                         this.txtSendDataArea.Clear();
                     }
                     return;
                 }//发送开始
                 else if (btnSendNet.Text == "手动发送")
                 {
                     #region 否循环发送

                     if (!this.chkAutoSendNet.Checked)
                     {
                         string strContent = this.txtSendDataArea.Text.Trim();
                         if (strContent.Length == 0)
                         {
                             this.ShowStatusMsg("发送内容不能为空！");
                             return;
                         }

                         byte[] sendBuf = HexStringToByteArray(strContent);
                         UDPService.SendCleaning(strTargetIp, strTargetPort, sendBuf);
                         return;
                     } 
                     #endregion

                     #region 循环发送

                     btnSendNet.Text = "停止发送";
                     this.txtSendDataArea.Enabled = false;
                     this.txtIntervalNet.Enabled = false;
                     int interval = 0;
                     if (Int32.TryParse(this.txtIntervalNet.Text.Trim(), out interval))
                     {
                         #region 如果选中多条发送，则数据入队列供消费

                         if (this.chkSendMore.Checked)
                         {
                             for (int i = 0; i < 10; i++)
                             {
                                 TextBox item = this.splitContainer4.Controls.Find("txtData" + i, true)[0] as TextBox;
                                 CheckBox IsSend = this.splitContainer4.Controls.Find("chkData" + i, true)[0] as CheckBox;
                                 if (item != null && !string.IsNullOrEmpty(item.Text.Trim()) && IsSend != null && IsSend.Checked)
                                 {
                                     lstSendData.Enqueue(item.Text.Trim());
                                 }
                             }
                         } 
                         #endregion

                         #region 如果按分隔符发送

                         if (this.chkSplitChar.Checked)
                         {
                             string[] strData = this.txtSendDataArea.Text.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                             foreach (string item in strData)
                             {
                                 lstSendData.Enqueue(item.Replace("\r", "").Replace("\n", ""));
                             }
                         }

                         #endregion

                         sendNetTimer.Interval = interval;
                         sendNetTimer.Elapsed += sendNetTimer_Elapsed;
                         sendNetTimer.Start();
                     }
                     else
                     {
                         this.ShowStatusMsg("网络发送间隔时间格式不对！");
                         return;
                     } 
                     #endregion
                 }
            }
            catch (Exception ex)
            {
                this.ShowStatusMsg(ex.Message);
            }
        }

        /// <summary>
        /// 网络发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sendNetTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                #region 发送消息处理
                //if (lstSendData.Count == 0)
                //{
                //    //多条指令发送结束后停止
                //    if (this.chkSendEnd.Checked)
                //    {
                //        return;
                //    }
                //}

                lock (objLock)
                {
                    //网络发送多条处理判断
                    if (lstSendData.Count == 0)
                    {
                        //多条指令发送结束后停止
                        if (this.chkSendEnd.Checked)
                        {
                            if (sendNetTimer.Enabled)
                            {
                                this.sendNetTimer.Stop();
                                this.BeginInvoke(new MethodInvoker(delegate()
                                {
                                    //this.btnSendNet.Text = "";
                                    btnSendNet_Click(null, null);
                                }));
                            }
                            return;
                        }

                        string strContent = this.txtSendDataArea.Text.Trim();
                        if (strContent.Length == 0)
                        {
                            this.ShowStatusMsg("发送内容不能为空！", true);
                            return;
                        }

                        byte[] sendBuf = HexStringToByteArray(strContent);
                        UDPService.SendCleaning(strTargetIp, strTargetPort, sendBuf);
                    }
                    else
                    {
                        #region  多条处理

                        //多条数据发送
                        byte[] sendBuf = HexStringToByteArray(lstSendData.Dequeue());
                        UDPService.SendCleaning(strTargetIp, strTargetPort, sendBuf);

                        #endregion
                    }   
                }

                #endregion
            }
            catch (Exception ex)
            {
                this.ShowStatusMsg(ex.Message, true);
                sendNetTimer.Stop();
            }
        }

        /// <summary>
        /// 多条发送指令 选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSendMore_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                //选中
                if (chkSendMore.Checked)
                {
                    //自动选中card类型接收
                    this.rbtnCardInfo.Checked = true;
                    //显示发送选中指令结束后停止发送
                    this.chkSendEnd.Visible = true;
                    this.chkAutoSendNet.Checked = true;
                    this.chkAutoSendNet.Enabled = false;
                    //分隔符控制
                    this.chkSplitChar.Checked = false;
                    this.chkSplitChar.Enabled = false;
                    int bottomHeight = this.splitContainer3.Height - this.splitContainer3.SplitterDistance;
                    NetSendPosition = bottomHeight;
                    if (bottomHeight < 142)
                    {
                        this.splitContainer3.SplitterDistance = this.splitContainer3.Height - 142;
                    }

                    if (!splitContainer4.Visible)
                    {
                        this.splitContainer4.Visible = true;
                    }
                }
                else
                {
                    //显示发送选中指令结束后停止发送
                    this.chkSendEnd.Visible = false;
                    this.chkAutoSendNet.Checked = false;
                    this.chkAutoSendNet.Enabled = true;
                    this.chkSplitChar.Enabled = true;
                    int bottomHeight = this.splitContainer3.Height - this.splitContainer3.SplitterDistance;

                    if (NetSendPosition == 0)
                    {
                        NetSendPosition = 54;
                    }
                    if (bottomHeight > NetSendPosition)
                    {
                        this.splitContainer3.SplitterDistance = this.splitContainer3.Height - NetSendPosition;
                    }

                    if (splitContainer4.Visible)
                    {
                        this.splitContainer4.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCom"></param>
        /// <returns></returns>
        private List<SerialPortPara> handleData(string strCom)
        {
            List<SerialPortPara> lstPara = new List<SerialPortPara>();
            List<ListItem> lstStopBit = getStopBit();
            List<ListItem> lstTemp = new List<ListItem>();

            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
            //FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            //using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            //{
            foreach (string dbit in cbbDataBits.Items)
            {
                if (dbit == "5")
                    lstTemp = lstStopBit.FindAll(r => r.Text.Contains('1'));
                else
                    lstTemp = lstStopBit.FindAll(r => !r.Text.Contains('.'));

                foreach (var bbit in cbbBaudRate.Items)
                {
                    foreach (string pbit in cbbParity.Items)
                    {
                        foreach (var sbit in lstTemp)
                        {
                            SerialPortPara sp = new SerialPortPara();
                            sp.PortName = strCom;
                            sp.BaudRate = Convert.ToInt32(bbit);
                            sp.DataBits = Convert.ToInt32(dbit);
                            sp.Parity = (Parity)Enum.Parse(typeof(Parity), pbit);
                            sp.StopBits = (StopBits)Convert.ToInt32(sbit.Value);

                            //sw.WriteLine(Format(sp));

                            lstPara.Add(sp);
                        }
                    }
                }
            }
            //    this.BeginInvoke(new MethodInvoker(delegate {
            //        txtShowData.AppendText("参数循环完成！");
            //    }));
            //}

            return lstPara;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<ListItem> getStopBit()
        {
            return new List<ListItem> {
                new ListItem(){
                    Text="1",
                    Value="1"
                },
                new ListItem(){
                    Text="1.5",
                    Value="3"
                },
                new ListItem(){
                    Text="2",
                    Value="2"
                }
            };
        }

        private void ChangeSerialArag(List<SerialPortPara> serialData)
        {
            foreach (var item in serialData)
            {
                recNum = 0;
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    if (ComDevice.IsOpen)
                    {
                        ComDevice.Close();
                    }
                    ComDevice.PortName = item.PortName;
                    ComDevice.BaudRate = item.BaudRate;
                    ComDevice.Parity = item.Parity;
                    ComDevice.DataBits = item.DataBits;
                    ComDevice.StopBits = item.StopBits;
                    ComDevice.Open();
                }));
                while (recNum < 1)
                {
                    Thread.Sleep(20);
                }
            }
            
            this.BeginInvoke(new MethodInvoker(delegate
            {
                lblOpenSerial_Click(null, null);
                this.txtShowData.AppendText("串口循环测试完毕！");
            }));
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void ClearSelf()
        {
            if (ComDevice.IsOpen)
            {
                ComDevice.Close();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public bool SendData(byte[] data)
        {
            if (ComDevice.IsOpen)
            {
                try
                {
                    ComDevice.Write(data, 0, data.Length);//发送数据
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("串口未打开", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] ReDatas = new byte[ComDevice.BytesToRead];
            ComDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据
            this.AddData(ReDatas);//输出数据
        }

        /// <summary>
        /// 网络数据接收显示
        /// </summary>
        /// <param name="buffer"></param>
        public void ReaderData(string strIp,string strPort,byte[] buffer)
        {
            this.AddDataNet(strIp, strPort, buffer);
        }
       
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="data">字节数组</param>
        public void AddData(byte[] data)
        {
            if (rbtnHex.Checked)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.AppendFormat("{0:x2}" + " ", data[i]);
                }
                AddContent(sb.ToString().ToUpper());
            }
            else if (rbtnASCII.Checked)
            {
                AddContent(new ASCIIEncoding().GetString(data));
            }
            else if (rbtnUTF8.Checked)
            {
                AddContent(new UTF8Encoding().GetString(data));
            }
            else if (rbtnUnicode.Checked)
            {
                AddContent(new UnicodeEncoding().GetString(data));
            }
            else
            {}

            receCnt += data.Length;
            if (chkCycles.Checked)
            {
                recNum++;
            }
            this.BeginInvoke(new MethodInvoker(delegate
            {
                statusStrip1.Items["toolStripStatusReceive"].Text = "接收：" + receCnt.ToString();
            }));
        }

        /// <summary>
        /// 显示在软件状态栏的消息提醒
        /// </summary>
        /// <param name="msg"></param>
        private void ShowStatusMsg(string msg, bool isInvoke = false)
        {
            if (isInvoke)
            {
                this.BeginInvoke(new MethodInvoker(delegate()
                {
                    this.statusMsg.Text = msg;
                }));
            }
            else
            {
                this.statusMsg.Text = msg;
            }
        }

        /// <summary>
        /// 输入到串口接收区域
        /// </summary>
        /// <param name="content"></param>
        private void AddContent(string content)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                if (!chkPauseShow.Checked)
                {
                    if (chkAutoLine.Checked && content.Length > 0)
                    {
                        if (txtShowData.TextLength > 0)
                        {
                            txtShowData.AppendText("\r\n");
                        }
                    }

                    if (chkCycles.Checked&& content.Length > 0)
                    {
                        txtShowData.AppendText(Format(ComDevice));
                    }
                    txtShowData.AppendText(content);
                }
            }));
        }

        /// <summary>
        /// 添加数据网络接收区
        /// </summary>
        /// <param name="strIp">Ip</param>
        /// <param name="strPort">端口</param>
        /// <param name="data">字节数组</param>
        public void AddDataNet(string strIp,string strPort,byte[] data)
        {
            if (rbtnHexNet.Checked)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.AppendFormat("{0:x2}" + " ", data[i]);
                }
                AddContentNet(sb.ToString().ToUpper());
            }
            else if (rbtnAsciiNet.Checked)
            {
                AddContentNet(new ASCIIEncoding().GetString(data));
            }
            else if (rbtnUtf8Net.Checked)
            {
                AddContentNet(new UTF8Encoding().GetString(data));
            }
            else if (rbtnUnicodeNet.Checked)
            {
                AddContentNet(new UnicodeEncoding().GetString(data));
            }
            else if (rbtnCardInfo.Checked)
            {
                string strHead = string.Format("[ Receive from {0} : {1} ]：\r\n", strIp, strPort);
                if (data.Length==9) //服务器回发禁止多次发送命令
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sb.AppendFormat("{0:x2}" + " ", data[i]);
                    }
                    AddContentNet(strHead + sb.ToString().ToUpper());
                }
                else if (data.Length > 9)//服务器返回消息
                {
                    AddContentNet(strHead + Encoding.GetEncoding(936).GetString(data.Skip(5).ToArray()));
                }
                else
                {

                }
            }
            else
            { }

            receCnt += data.Length;
            this.BeginInvoke(new MethodInvoker(delegate
            {
                statusStrip1.Items["toolStripStatusReceive"].Text = "接收：" + receCnt.ToString();
            }));
        }

        /// <summary>
        /// 网络接收区域
        /// </summary>
        /// <param name="content"></param>
        private void AddContentNet(string content)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                if (!chkPauseShowNet.Checked)
                {
                    if (chkAutoLineNet.Checked && content.Length > 0)
                    {
                        if (txtRecvDataArea.TextLength > 0)
                        {
                            txtRecvDataArea.AppendText("\r\n");
                        }
                    }
                    txtRecvDataArea.AppendText(content);
                }
            }));
        }

        /// <summary>
        /// 得到串口名
        /// </summary>
        /// <returns></returns>
        public string[] GetPortNames()
        {
            return SerialPort.GetPortNames();
        }
        /// <summary>
        /// 判断串口是否存在
        /// </summary>
        /// <param name="port_name"></param>
        /// <returns></returns>
        public bool Exists(string port_name)
        {
            foreach (string a in SerialPort.GetPortNames())
            {
                if (a == port_name)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public string Format(SerialPort port)
        {
            return string.Format("{0} [{1},{2},{3},{4},{5}] ", new object[]
	                        {
		                        port.PortName,
		                        port.BaudRate,
		                        port.DataBits,
		                        port.StopBits,
		                        port.Parity,
		                        port.Handshake
	                        });
        }

        public string Format(SerialPortPara port)
        {
            return string.Format("{0} [{1},{2},{3},{4}] ", new object[]
	                        {
		                        port.PortName,
		                        port.BaudRate,
		                        port.DataBits,
		                        port.StopBits,
		                        port.Parity
	                        });
        }
   
        /// <summary>
        /// 16进制转二进制
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            byte[] array = new byte[msg.Length / 2];
            for (int i = 0; i < msg.Length; i += 2)
            {
                array[i / 2] = Convert.ToByte(msg.Substring(i, 2), 16);
            }
            return array;
        }

        /// <summary>
        /// 字符串转换为16进制字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        private string StringToHexString(string s, Encoding encode)
        {
            byte[] b = encode.GetBytes(s);//按照指定编码将string编程字节数组
            string result = string.Empty;
            for (int i = 0; i < b.Length; i++)//逐字节变为16进制字符
            {
                result += Convert.ToString(b[i], 16).PadLeft(3, ' ');
            }
            return result;
        }

        /// <summary>
        /// 二进制转16进制
        /// </summary>
        /// <param name="comByte"></param>
        /// <returns></returns>
        public string ByteToHex(byte[] comByte)
        {
            StringBuilder stringBuilder = new StringBuilder(comByte.Length * 3);
            foreach (byte value in comByte)
            {
                stringBuilder.Append(Convert.ToString(value, 16).PadLeft(2, '0').PadRight(3, ' '));
            }
            return stringBuilder.ToString().ToUpper();
        }

        /// <summary>
        /// 16进制转byte数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string s)
        {
            if (s.Length == 0)
                return new byte[1];

            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        } 

        /// <summary>
        /// 文件保存
        /// </summary>
        /// <returns></returns>
        private void ShowSaveFileDialog()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "接收区显示内容另存为";
            //设置文件类型 
            sfd.Filter = "文本文件（*.txt）|*.txt";

            //设置默认文件类型显示顺序 
            sfd.FilterIndex = 1;
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            sfd.FileName = string.Concat(cbbComList.SelectedItem, DateTime.Now.ToString("yyMMddHHmmss"));

            //点了保存按钮进入 
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (this.txtShowData.TextLength > 0)
                {
                    using (System.IO.FileStream fsWrite = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                    {
                        byte[] buffer = Encoding.Default.GetBytes(this.txtShowData.Text.Trim());
                        fsWrite.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        /// <summary>
        ///                                                                                                          
        /// </summary>
        private void ShowOpenFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "打开";
            fileDialog.InitialDirectory = Application.StartupPath; //初始路径,这里设置的是程序的起始位置，可自由设置
            fileDialog.Filter = "所有文件(*.*)|*.*|文本文件(*.txt)|*.txt";
            fileDialog.FilterIndex = 2;                  //文件类型的显示顺序（上一行.txt设为第二位）
            fileDialog.RestoreDirectory = true; //对话框记忆之前打开的目录
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader sr = new System.IO.StreamReader(fileDialog.FileName, Encoding.Default);
                this.txtSendData.Text = sr.ReadToEnd();
                sr.Close();
            }
        }


        /// <summary>  
        /// 运行一个控制台程序并返回其输出参数。  
        /// </summary>  
        /// <param name="filename">程序名</param>  
        /// <param name="arguments">输入参数</param>  
        /// <returns></returns>  
        public static string RunApp(string filename, string arguments, bool recordLog)
        {
            try
            {
                if (recordLog)
                {
                    Trace.WriteLine(filename + " " + arguments);
                }
                Process proc = new Process();
                proc.StartInfo.FileName = filename;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Arguments = arguments;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.Start();

                using (System.IO.StreamReader sr = new System.IO.StreamReader(proc.StandardOutput.BaseStream, Encoding.Default))
                {
                    //string txt = sr.ReadToEnd();  
                    //sr.Close();  
                    //if (recordLog)  
                    //{  
                    //    Trace.WriteLine(txt);  
                    //}  
                    //if (!proc.HasExited)  
                    //{  
                    //    proc.Kill();  
                    //}  
                    //上面标记的是原文，下面是我自己调试错误后自行修改的  
                    Thread.Sleep(100);           //貌似调用系统的nslookup还未返回数据或者数据未编码完成，程序就已经跳过直接执行  
                    //txt = sr.ReadToEnd()了，导致返回的数据为空，故睡眠令硬件反应  
                    if (!proc.HasExited)         //在无参数调用nslookup后，可以继续输入命令继续操作，如果进程未停止就直接执行  
                    {                            //txt = sr.ReadToEnd()程序就在等待输入，而且又无法输入，直接掐住无法继续运行  
                        proc.Kill();
                    }
                    string txt = sr.ReadToEnd();
                    sr.Close();
                    if (recordLog)
                        Trace.WriteLine(txt);
                    return txt;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return ex.Message;
            }
        }

        /// <summary>  
        /// 获取当前使用的本地局域网IP ，而不是返回本地回环IP 
        /// </summary>  
        /// <returns></returns>  
        public static string GetRealIP()
        {
            string result = RunApp("route", "print", true);
            Match m = Regex.Match(result, @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");
            if (m.Success)
            {
                return m.Groups[2].Value;
            }
            else
            {
                try
                {
                    System.Net.Sockets.TcpClient c = new System.Net.Sockets.TcpClient();
                    c.Connect("www.baidu.com", 80);
                    string ip = ((System.Net.IPEndPoint)c.Client.LocalEndPoint).Address.ToString();
                    c.Close();
                    return ip;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private static string GetLocalIP()
        {
            string localIP = "?";
            //IPHostEntry host;
            //host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (IPAddress ip in host.AddressList)
            //{
            //    if (ip.AddressFamily.ToString() == "InterNetwork")
            //    {
            //        localIP = ip.ToString();
            //        break;
            //    }
            //}

            localIP = GetRealIP();
            return localIP;
        }

        #endregion
     
        /// <summary>
        /// 分隔符选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSplitChar_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSplitChar.Checked)
            {
                chkAutoSendNet.Checked = true;
            }
            else
            {
                if (!this.chkSendMore.Checked)
                {
                    chkAutoSendNet.Checked = false;
                }
            }
        }

    }
}
