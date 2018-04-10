using System.Web.Mvc;

namespace DirectAgents.Web.Areas.ProgAdmin
{
    public class ProgAdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ProgAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ProgAdmin_default",
                "ProgAdmin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
                //new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}