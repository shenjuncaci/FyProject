using System;
using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Mobile
{
    public class LoginAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// 响应前执行验证,查看当前用户是否有效 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            
            //登录是否过期
            if (!ManageProvider.Provider.IsOverdue())
            {
                filterContext.Result = new RedirectResult("~/Home");
            }
        }
    }
}