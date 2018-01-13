using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.VPModule
{
    public class VPModuleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "VPModule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "VPModule_default",
                "VPModule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
