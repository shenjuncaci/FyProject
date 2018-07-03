using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace LeaRun.WebService
{
    /// <summary>
    /// IPBroadCast 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class IPBroadCast : System.Web.Services.WebService
    {

        [WebMethod(EnableSession = true, Description = "获取需要报警的IP")]
        public string AlarmIP()
        {
            return "172.19.0.15|172.19.0.12";
        }

        [WebMethod(EnableSession = true, Description = "取消响铃的IP")]
        public string SilentIP()
        {
            return "172.19.0.13|172.19.0.14";
        }


    }
}
