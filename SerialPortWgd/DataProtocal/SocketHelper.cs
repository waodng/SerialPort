using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/* ==============================================================================
 * 创建日期：2020/5/11 17:22:43
 * 创 建 者：wgd
 * 功能描述：TcpLister  
 * ==============================================================================*/
namespace SerialPortWgd.DataProtocal
{
    public class SocketHelper
    {
        public class StateObject
        {
            public Socket workSocket = null;

            public byte[] buffer = null;

            public StringBuilder sb = new StringBuilder();
            public List<byte> list = new List<byte>();
            public int offset = 0;
        }

        private static ManualResetEvent Done = new ManualResetEvent(false);
        private static object obj = new object();

        /// <summary>
        /// 监听配置
        /// </summary>
        public static ListenerInfo ConfigInfo { get; set; }

        /// <summary>
        /// Socke连接
        /// </summary>
        private static Socket serverSocket = null;

        public static bool IsRuning
        {
            get
            {
                return serverSocket == null ? false :
                    serverSocket.LingerState.Enabled;
            }
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        public static void BeginListen()
        {
            IPAddress localAddress = IPAddress.Any;
            SocketType sockType = SocketType.Stream;
            ProtocolType sockProtocol = ProtocolType.Tcp;

            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(localAddress, ConfigInfo.LocalPort);
                serverSocket = new Socket(localAddress.AddressFamily, sockType, sockProtocol);

                serverSocket.Bind(localEndPoint);
                //CommonHelper.AppLogger.WarnFormat(string.Format("服务器监听端口：{0}", localEndPoint.Address + ":" + localEndPoint.Port + Environment.NewLine));
                serverSocket.Listen(ConfigInfo.MaxConnections);

                while (true)
                {
                    //侦听到一个新传入的连接
                    Done.Reset();//将状态设为非终止
                    serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), serverSocket);
                    Done.WaitOne();
                }
            }
            catch (SocketException ex)
            {
                //LogHelper.LogError(ex, LogCatagories.Sock);
            }
            catch (Exception ex)
            {
                //CommonHelper.AppLogger.ErrorFormat(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        static void AcceptCallBack(IAsyncResult ar)//ar表示异步操作的状态。
        {
            Socket socket;    //用于接受来自客户端的请求连接
            Socket handler = null;
            try
            {
                Done.Set();//设为终止
                socket = (Socket)ar.AsyncState; //获取状态
                handler = socket.EndAccept(ar);

                StateObject stateObject = new StateObject();
                stateObject.buffer = new byte[ConfigInfo.BufferSize];
                stateObject.workSocket = handler;
                //Console.WriteLine("开始 线程id：{0}", System.Threading.Thread.CurrentThread.ManagedThreadId);
                handler.BeginReceive(stateObject.buffer, 0, ConfigInfo.BufferSize, 0, new AsyncCallback(ReceiveCallBack), stateObject);
            }
            catch (SocketException ex)
            {
                //CommonHelper.AppLogger.ErrorFormat(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            catch (Exception ex)
            {
                //CommonHelper.AppLogger.ErrorFormat(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                lock (obj)
                {
                    //Console.WriteLine("结束 线程id：{0}",System.Threading.Thread.CurrentThread.ManagedThreadId);
                    StateObject state = (StateObject)ar.AsyncState;
                    handler = state.workSocket;
                    int bytesRead = handler.EndReceive(ar);

                    if (bytesRead > 0)
                    {
                        //byte[] endDcid = new byte[] { 0x3C, 0x2F, 0x44, 0x43, 0x49, 0x44, 0x61, 0x74, 0x61, 0x3E };
                        int endByteLen = 0;
                        KeyValuePair<string, int> endIdx = IndexOfByte(state.buffer, ListenerInfo.endBytes, out endByteLen);
                        if (endIdx.Value > -1)
                        {
                            int sumLen = endIdx.Value + endByteLen;
                            state.list.AddRange(state.buffer.Take(sumLen));
                            if (sumLen < state.buffer.Length)
                            {
                                List<byte> tmp = state.buffer.Skip(sumLen).ToList();
                                tmp.AddRange(new byte[sumLen]);
                                state.buffer = tmp.ToArray();
                            }
                            else
                            {
                                state.buffer = new byte[ConfigInfo.BufferSize];
                            }

                            byte[] bs = state.list.ToArray();
                            state.list.Clear();
                            handler.BeginReceive(state.buffer, 0, ConfigInfo.BufferSize, 0,
                           new AsyncCallback(ReceiveCallBack), state);

                            //Regex reg = new Regex("<DCIData>(.*?)</DCIData>",RegexOptions.IgnoreCase);
                            string pattern = @"(?<=^\[len=)(\d+)(?=\])";//数据包正则，获取数据包长度
                            if (MessageReviced != null)
                            {
                                MessageReviced(null, new MessageRevicedEventArgs()
                                {
                                    MsgType = endIdx.Key,
                                    Contents = bs,
                                    SocketHandler = handler
                                });
                            };
                        }
                        else
                        {
                            state.list.AddRange(state.buffer);
                            // Not all data received. Get more.
                            handler.BeginReceive(state.buffer, 0, ConfigInfo.BufferSize, 0,
                            new AsyncCallback(ReceiveCallBack), state);
                        }
                    }
                    else
                    {
                        //LogHelper.LogError("没有接受到数据", LogCatagories.Sock);
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.LogError(ex, LogCatagories.Sock);
            }

        }

        private static KeyValuePair<string, int> IndexOfByte(byte[] data, byte[] find, out int len)
        {
            len = find.Length;
            for (int i = 0; i < data.Length - find.Length; i++)
            {
                if (data.Skip(i).Take(find.Length).SequenceEqual(find))
                    return new KeyValuePair<string, int>("DCIData", i);
            }
            return new KeyValuePair<string, int>("", -1);
        }

        private static KeyValuePair<string, int> IndexOfByte(byte[] data, Dictionary<string, object> lstFind, out int len)
        {
            len = 1;
            foreach (KeyValuePair<string, object> item in lstFind)
            {
                byte b = new byte();
                if (byte.TryParse(item.Value.ToString(), out b))
                {
                    return new KeyValuePair<string, int>(item.Key, data.ToList().IndexOf(b));
                }
                else
                {
                    byte[] find = (byte[])item.Value;
                    len = find.Length;
                    for (int i = 0; i < data.Length - find.Length; i++)
                    {
                        if (data.Skip(i).Take(find.Length).SequenceEqual(find))
                            return new KeyValuePair<string, int>(item.Key, i);
                    }
                }
            }
            return new KeyValuePair<string, int>("", -1);
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public static void StopListen()
        {
            if (serverSocket.LingerState.Enabled)
            {
                serverSocket.Shutdown(SocketShutdown.Both);
            }
            serverSocket.Close();
            serverSocket = null;
        }

        /// <summary>
        /// 回复
        /// </summary>
        public static void SendAck(string ackMessage, Socket handler)
        {
            try
            {
                if (handler.Connected)
                {
                    Byte[] byteSend = Encoding.UTF8.GetBytes(ackMessage);
                    handler.SendTimeout = 6000;
                    handler.BeginSend(byteSend, 0, byteSend.Length, 0, new AsyncCallback(SendCallBack), handler);
                }
                else
                {
                    //CommonHelper.AppLogger.ErrorFormat(string.Format("连接已关闭,回复消息内容是:{0}", ackMessage));
                }
            }
            catch (SocketException ex)
            {
                //CommonHelper.AppLogger.ErrorFormat(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            catch (Exception ex)
            {
               // CommonHelper.AppLogger.ErrorFormat(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        static void SendCallBack(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                handler.EndSend(ar);
            }
            catch (SocketException ex)
            {
                //CommonHelper.AppLogger.ErrorFormat(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                handler.Shutdown(SocketShutdown.Send);
                //LingerOption option = new LingerOption(true, 60);
                //handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, true);
                //handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                handler.Close();
            }
        }

        /// <summary>
        /// 消息到达事件
        /// </summary>
        public static event EventHandler<MessageRevicedEventArgs> MessageReviced;
    }
}
