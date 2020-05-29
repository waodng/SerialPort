using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace SerialPortWgd.DataProtocal
{
    public class ListenerService
    {
        /// <summary>
        /// 
        /// </summary>
        public ListenerService()
        {

        }

        /// <summary>
        /// 开始处理
        /// </summary>
        /// <param name="listenerInfo"></param>
        public void BeginProcess(ListenerInfo listenerInfo)
        {
            SocketHelper.ConfigInfo = listenerInfo;
            SocketHelper.MessageReviced += new EventHandler<MessageRevicedEventArgs>(SocketHelper_MessageReviced);
            try
            {
                SocketHelper.BeginListen();
            }
            catch (Exception ex)
            {
                //CommonHelper.AppLogger.ErrorFormat(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        static int reviceCount = 0;
        /// <summary>
        /// 消息达到后交个Biz处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SocketHelper_MessageReviced(object sender, MessageRevicedEventArgs e)
        {
            string message = System.Text.Encoding.UTF8.GetString(e.Contents);
            try
            {
               
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// 结束处理
        /// </summary>
        public void EndProecess()
        {
            SocketHelper.StopListen();
        }

        public bool IsRuning
        {
            get { return SocketHelper.IsRuning; }
        }

        public void Start()
        {
            //CommonHelper.AppLogger.InfoFormat("采集服务已启动...");
        }
        public void Stop()
        {
            EndProecess();
            //CommonHelper.AppLogger.InfoFormat("采集服务已停止...");
        }
    }
}
