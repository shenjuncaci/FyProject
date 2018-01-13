using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.TCModule
{
    public class TCModuleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TCModule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "TCModule_default",
                "TCModule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
