//=====================================================================================
// All Rights Reserved , Copyright © shenjun 20170622
//=====================================================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LeaRun.Utilities
{
    public class MailHelper
    {

       /// <summary>
       /// 
       /// </summary>
       /// <param name="reciver">收件人地址</param>
       /// <param name="Content">发送内容</param>
       /// <returns>1成功，0失败</returns>
        public static int SendEmail(string reciver, string Content)
        {


            try
            {
                var emailAcount = "shfy_it@fuyaogroup.com";
                var emailPassword = "Sj1234";
               
                var content = Content;
                MailMessage message = new MailMessage();
                //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
                MailAddress fromAddr = new MailAddress("shfy_it@fuyaogroup.com");
                message.From = fromAddr;
                //设置收件人,可添加多个,添加方法与下面的一样
                message.To.Add(reciver);
                //设置抄送人
                message.CC.Add("jun.shen@fuyaogroup.com");
                //设置邮件标题
                message.Subject = "QSB快速反应";
                //设置邮件内容
                message.Body = content;
                //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
                SmtpClient client = new SmtpClient("mail.fuyaogroup.com", 25);
                //设置发送人的邮箱账号和密码
                client.Credentials = new NetworkCredential(emailAcount, emailPassword);
                //启用ssl,也就是安全发送
                client.EnableSsl = true;
                //发送邮件
                //加这段之前用公司邮箱发送报错：根据验证过程，远程证书无效
                //加上后解决问题
                ServicePointManager.ServerCertificateValidationCallback =
delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                client.Send(message);
                return 1;
            }
            catch
            {
                return 0;
            }
           
        }
    }
}
