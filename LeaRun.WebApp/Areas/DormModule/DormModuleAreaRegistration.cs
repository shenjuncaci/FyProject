using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.DormModule
{
    public class DormModuleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DormModule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DormModule_default",
                "DormModule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
