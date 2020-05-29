using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

/* ==============================================================================
 * 创建日期：2020/5/11 17:36:27
 * 创 建 者：wgd
 * 功能描述：UDPService  
 * ==============================================================================*/
namespace SerialPortWgd.DataProtocal
{
    public class UDPService
    {
        private static UDPService m_Instance;
        private string m_strMessage;
        private bool m_bEnable;
        private List<Thread> m_lstThread;

        private int m_nPort;
        private IPEndPoint ipLocalPoint;
        private EndPoint RemotePoint;
        public Socket mySocket;
        byte[] revbufheadbak = new byte[20];  //备份包信息，重复包不要再处理 

        private Thread m_tMainTread;
        private Thread m_tDataTread;

        public static event RFIDDataEventHandler DataDeal;
        public delegate void RFIDDataEventHandler(string ip, string port, byte[] buffer);

        #region 属性
        public static string Message
        {
            get
            {
                Create();
                return m_Instance.m_strMessage;
            }
        }

        public static int WinPort
        {
            set
            {
                Create();
                m_Instance.m_nPort = value;
            }
        }

        private static bool ListionOn
        {
            set { m_Instance.m_bEnable = value; }
            get { return m_Instance.m_bEnable; }
        }
        #endregion

        #region 初始化
        private static void Create()
        {
            if (m_Instance == null)
            {
                m_Instance = new UDPService();
            }
            m_Instance.m_strMessage = string.Empty;
        }

        private UDPService()
        {

            Reset();
        }

        private void Reset()
        {
            m_strMessage = string.Empty;
            m_bEnable = false;
            m_lstThread = new List<Thread>();
            m_nPort = 0;
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

        /// <summary>
        /// 启动监听线程
        /// </summary>
        /// <returns></returns>
        private bool TurnOn()
        {
            try
            {
                //定义网络类型，数据连接类型和网络协议UDP   
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                ipLocalPoint = new IPEndPoint(IPAddress.Parse(GetLocalIP()), m_nPort); //getIPAddress()获得本地地址
                //绑定网络地址   
                mySocket.Bind(ipLocalPoint);

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, m_nPort);
                RemotePoint = (EndPoint)(ipep);

                m_bEnable = true;

                m_tMainTread = new Thread(MainThreadProc); //监听线程
                m_tMainTread.IsBackground = true;
                m_tMainTread.Start();
            }
            catch (Exception ex)
            {
                m_strMessage = string.Format("有异常发生：{0}", ex.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 停止监听
        /// </summary>
        /// <returns></returns>
        private bool TurnOff()
        {
            try
            {
                m_bEnable = false;
                if (m_tMainTread != null)
                    if (m_tMainTread.IsAlive)
                        m_tMainTread.Abort();
                if (mySocket != null)
                    mySocket.Close();

                if (m_tDataTread != null)
                    if (m_tDataTread.IsAlive)
                        m_tDataTread.Abort();

                foreach (Thread oThread in m_lstThread)
                {
                    if (oThread.IsAlive)
                        oThread.Abort();
                }
            }
            catch (Exception ex)
            {
                m_strMessage = string.Format("有异常发生：{0}", ex.Message);
                return false;
            }
            return true;
        }

        private bool CleanMethod()
        {
            try
            {
                if (DataDeal != null)
                {
                    foreach (Delegate odel in DataDeal.GetInvocationList())
                    {
                        DataDeal -= (RFIDDataEventHandler)odel;
                    }
                }
            }
            catch (Exception ex)
            {
                m_strMessage = string.Format("有异常发生：{0}", ex.Message);
                return false;
            }

            return true;
        }

        #endregion

        #region 公用方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool Star()
        {
            if (!m_Instance.CleanMethod())
                return false;
            if (!ListionOn)
                return m_Instance.TurnOn();
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void Stop()
        {
            if (ListionOn)
                m_Instance.TurnOff();
        }

        #endregion

        #region 处理线程
        private void MainThreadProc()
        {
            //接收数据处理线程   
            byte[] buf = new byte[1024];

            while (m_bEnable)
            {
                if (mySocket == null || mySocket.Available < 1)
                {
                    Thread.Sleep(30);
                    continue;
                }
                //跨线程调用控件   
                //接收UDP数据报，引用参数RemotePoint获得源地址   
                int rlen = mySocket.ReceiveFrom(buf, ref RemotePoint);
                List<byte> recvBuf = new List<byte>();
                for (int i = 0; i < rlen; i++)
                {
                    recvBuf.Add(buf[i]);
                }

                IPEndPoint endPoint = RemotePoint as IPEndPoint;
                if (endPoint != null)
                {
                    DataDeal(endPoint.Address.ToString(), endPoint.Port.ToString(), recvBuf.ToArray());
                }
                else
                {
                    DataDeal("0.0.0.0", "0000", recvBuf.ToArray());
                }
            }
            mySocket.Close();
        }

        private void ListionThreadProc(object oClient)
        {

        }
        #endregion

        #region 数据转换
        private string Byte2Hex(byte[] byts, int nLength)
        {
            string strRet = "";
            for (int i = 6; i < 10; i++)
            {
                strRet += byts[i].ToString("x").PadLeft(2, Convert.ToChar("0")) + " ";
            }

            return strRet.Trim();
        }

        private string ByteTrun(byte[] byts, int nLength)
        {
            string strRet = "";
            for (int i = 0; i < nLength; i++)
            {
                strRet += byts[i].ToString("x").PadLeft(2, Convert.ToChar("0")) + " ";
            }

            return strRet.Trim();
        }

        /// <summary>
        /// 响一声命令
        /// </summary>
        /// <returns></returns>
        private static byte[] ThreeSound()
        {
            byte[] byts = new byte[5];
            byts[0] = 0x59;
            byts[1] = 0x00;
            byts[2] = 0x01;
            byts[3] = 0;
            byts[4] = 0;
            return byts;
        }
        /// <summary>
        /// 响3声命令    /// 
        /// </summary>
        /// <returns></returns>
        private static byte[] OneSound()
        {
            byte[] byts = new byte[5];
            byts[0] = 0x59;
            byts[1] = 0x00;
            byts[2] = 1;
            byts[3] = 0;
            byts[4] = 1;
            return byts;
        }
        #endregion

        #region 回复声音

        public static bool checkip(string ipstr)   //判断IP是否合法
        {   //using System.Net;   
            IPAddress ip;
            if (IPAddress.TryParse(ipstr, out ip))
            { return true; }
            else { return false; }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="strIP">目标IP</param>
        /// <param name="strPort">目标端口</param>
        /// <param name="sendbuf">发送消息byte格式</param>
        public static void SendCleaning(String strIP, int strPort, byte[] sendbuf)//发至两行屏读卡器
        {
            if (!checkip(strIP))
            {
                //Logger.WriteLine("没有找到对应的设备！");
                return;
            }
            EndPoint RemotePointls;

            Socket mySocket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipLocalPoint2 = new IPEndPoint(IPAddress.Parse(strIP), 49169); //getIPAddress()获得本地地址
            //绑定网络地址   
            mySocket2.Bind(ipLocalPoint2);

            //广播式
            //IPEndPoint ipep = new IPEndPoint(IPAddress.Broadcast, 39170);
            //RemotePointls = (EndPoint)(ipep);
            //mySocket2.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);//设为广播式发送

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(strIP), strPort);
            RemotePointls = (EndPoint)(ipep);

            try
            {
                mySocket2.SendTo(sendbuf, sendbuf.Length, SocketFlags.None, RemotePointls);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                mySocket2.Close();
            }
        }

        #endregion

        /// <summary>
        /// 支除读卡器自动返回的字符
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        private bool IsAvalidData(string strData)
        {
            if (strData == "59 00 02 00 4f 4b")
                return false;
            return true;
        }

        private bool IsIPV4(object oInput)
        {
            if (Convert.IsDBNull(oInput) || oInput == null)
                return false;
            if (Convert.ToString(oInput).Length == 0)
                return false;
            string strRegexExpression = "^((2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.){3}(2[0-4]\\d|25[0-5]|[01]?\\d\\d?)$";
            return System.Text.RegularExpressions.Regex.IsMatch(Convert.ToString(oInput), strRegexExpression);
        }

    }
}
