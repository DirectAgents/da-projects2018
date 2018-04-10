using System.Web.Mvc;

namespace DirectAgents.Web.Areas.AB
{
    public class ABAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "AB";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "AB_default",
                "AB/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}