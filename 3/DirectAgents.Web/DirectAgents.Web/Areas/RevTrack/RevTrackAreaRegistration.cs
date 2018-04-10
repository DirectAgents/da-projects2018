using System.Web.Mvc;

namespace DirectAgents.Web.Areas.RevTrack
{
    public class RevTrackAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "RevTrack";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "RevTrack_default",
                "RevTrack/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}