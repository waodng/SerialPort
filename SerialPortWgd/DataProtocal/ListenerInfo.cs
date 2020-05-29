using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net.Sockets;
using System.Collections.Specialized;

namespace SerialPortWgd.DataProtocal
{
    public class ListenerInfo
    {
        /// <summary>
        /// 缓存大小
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// 监听端口
        /// </summary>
        public int LocalPort
        {
            get;
            set;
        }

        /// <summary>
        /// 最大连接
        /// </summary>
        public int MaxConnections
        {
            get;
            set;
        }

        /// <summary>
        /// 超时设置
        /// </summary>
        public int TimeOut
        {
            get;
            set;
        }
        /// <summary>
        /// 结束位字符数组
        /// </summary>
        public static Dictionary<string, object> endBytes
        {
            get;
            set;
        }

        /// <summary>
        /// 结束位字符数组
        /// </summary>
        public static byte[] endByte
        {
            get;
            set;
        }

        public void LoadFromConfig()
        {
            this.LocalPort = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            this.MaxConnections = Convert.ToInt32(ConfigurationManager.AppSettings["MaxConn"]);
            this.BufferSize = Convert.ToInt32(ConfigurationManager.AppSettings["BufferSize"]);
            this.TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["ReviceTimeOut"]);
            endBytes = ConvertToList((NameValueCollection)ConfigurationManager.GetSection("StartEnd"));
            endByte = ConvertToByte(ConfigurationManager.AppSettings.Get("EndByte"));
        }

        private byte[] ConvertToByte(string srcData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(srcData))
                {
                    return null;
                }
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^[0-9]{1,3}$");

                string it = srcData.Trim();
                if (reg.IsMatch(it))
                {
                    byte[] b = new byte[] { Convert.ToByte(it) };
                    return b;
                }
                else
                {
                    byte[] b = System.Text.Encoding.Default.GetBytes(it);
                    return b;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private Dictionary<string,object> ConvertToList(NameValueCollection srcData)
        {
            Dictionary<string, object> lstRes = new Dictionary<string, object>();

            try
            {
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("^[0-9]{1,3}$");

                foreach (string item in srcData)
                {
                    string it = srcData.GetValues(item)[0];
                    if (reg.IsMatch(it))
                    {
                        lstRes.Add(item, Convert.ToByte(it));
                    }
                    else
                    {
                        byte[] b = System.Text.Encoding.Default.GetBytes(it);
                        lstRes.Add(item, b);
                    }
                }
            }
            catch (Exception ex)
            {
                return lstRes;
            }

            return lstRes;
        }
    }
}
