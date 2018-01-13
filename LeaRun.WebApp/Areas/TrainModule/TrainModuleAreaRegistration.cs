using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.TrainModule
{
    public class TrainModuleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TrainModule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "TrainModule_default",
                "TrainModule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
