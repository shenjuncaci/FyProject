using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace LeaRun.Utilities
{
    public static class WeChatHelper
    {
        /// <summary>
        /// 根据当前日期 判断Access_Token 是否超期  如果超期返回新的Access_Token   否则返回之前的Access_Token
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static string IsExistAccess_Token()
        {
            string Token = string.Empty;
            DateTime YouXRQ;
            // 读取XML文件中的数据，并显示出来 ，注意文件路径
            string filepath = HttpContext.Current.Server.MapPath("\\Resource\\WeChat\\AccessToken.xml");

            StreamReader str = new StreamReader(filepath, Encoding.UTF8);
            XmlDocument xml = new XmlDocument();
            xml.Load(str);
            str.Close();
            str.Dispose();
            Token = xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText;
            YouXRQ = Convert.ToDateTime(xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText);

            //TimeSpan st1 = new TimeSpan(YouXRQ.Ticks); //最后刷新的时间
            //TimeSpan st2 = new TimeSpan(DateTime.Now.Ticks); //当前时间
            //TimeSpan st = st2 - st1; //两者相差时间
            if (DateTime.Now > YouXRQ)
            {
                DateTime _youxrq = DateTime.Now;
                Access_token mode = GetAccess_token();
                xml.SelectSingleNode("xml").SelectSingleNode("Access_Token").InnerText = mode.access_token;
                _youxrq = _youxrq.AddSeconds(int.Parse(mode.expires_in));
                xml.SelectSingleNode("xml").SelectSingleNode("Access_YouXRQ").InnerText = _youxrq.ToString();
                xml.Save(filepath);
                Token = mode.access_token;
            }
            return Token;
        }


        /// <summary>
        /// 获取Access_token
        /// </summary>
        /// <returns></returns>
        public static Access_token GetAccess_token()
        {
            string appid = "wxc625758e50b5ced1"; //微信公众号appid
            string secret = "k5XpFWoq9YbqGNEWUK_fiDYoS6UJLu-YlFqMAUOOF4s";  //微信公众号appsecret
            string strUrl = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=" + appid + "&corpsecret=" + secret;
            Access_token mode = new Access_token();

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(strUrl);  //用GET形式请求指定的地址 
            req.Method = "GET";

            using (WebResponse wr = req.GetResponse())
            {
                //HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();  
                StreamReader reader = new StreamReader(wr.GetResponseStream(), Encoding.UTF8);
                string content = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();

                //在这里对Access_token 赋值  
                Access_token token = new Access_token();
                token = JsonConvert.DeserializeObject<Access_token>(content);
                mode.access_token = token.access_token;
                mode.expires_in = token.expires_in;
            }
            return mode;
        }

        public static void SendWxMessage(string ToUser,string Content)
        {

            SendMessage message = new SendMessage();
            MessageContent messageconten = new MessageContent();
            messageconten.content = Content;
            message.touser = ToUser;
            message.toparty = "";
            message.totag = "";
            message.msgtype = "text";
            message.agentid = 18;
            message.text = messageconten;
            message.safe = 0;
            string mm = message.ToJson().ToString();
            HttpPost("https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=" + IsExistAccess_Token(), message.ToJson().ToString());
        }

        public class Access_token
        {
            public Access_token()
            {
                //
                //TODO: 在此处添加构造函数逻辑
                //
            }
            string _access_token;
            string _expires_in;

            /// <summary>
            /// 获取到的凭证 
            /// </summary>
            public string access_token
            {
                get { return _access_token; }
                set { _access_token = value; }
            }


            /// <summary>
            /// 凭证有效时间，单位：秒
            /// </summary>
            public string expires_in
            {
                get { return _expires_in; }
                set { _expires_in = value; }
            }
        }

        public class SendMessage
        {
            public string touser { get; set; }
            public string toparty { get; set; }
            public string totag { get; set; }
            public string msgtype { get; set; }
            public int agentid { get; set; }
            public MessageContent text { get; set; }
            public int safe { get; set; }
        }

        public class MessageContent
        {
            public string content { get; set; }
        }

        public static void HttpPost(string Url, string postDataStr)
        {
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded;charset=utf8";
            //request.ContentLength = postDataStr.Length;
            //StreamWriter writer = new StreamWriter(request.GetRequestStream(), Encoding.UTF8);
            //writer.Write(postDataStr);
            //writer.Flush();
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //string encoding = response.ContentEncoding;
            //if (encoding == null || encoding.Length < 1)
            //{
            //    encoding = "UTF-8"; //默认编码 
            //}
            //StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            //string retString = reader.ReadToEnd();
            //return retString;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            byte[] postBytes = Encoding.UTF8.GetBytes(postDataStr);
            request.ContentLength = Encoding.UTF8.GetBytes(postDataStr).Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(postBytes, 0, postBytes.Length);
            }


            
        }
    }
}