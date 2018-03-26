using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.ProjectManageModule
{
    public class ProjectManageModuleAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ProjectManageModule";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ProjectManageModule_default",
                "ProjectManageModule/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
