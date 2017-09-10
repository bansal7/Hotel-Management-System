using Microsoft.Owin;
using Owin;
using System.Web.Mvc;

[assembly: OwinStartupAttribute(typeof(HotelManagementSystem.Startup))]
namespace HotelManagementSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            RazorViewEngine rve = new RazorViewEngine();
            rve.PartialViewLocationFormats =
                rve.ViewLocationFormats =
                    rve.MasterLocationFormats =
                        new string[] {
                   "~/Views/Receptionist/{1}/{0}.cshtml",
                   "~/Views/Guest/{1}/{0}.cshtml",
                   "~/Views/{1}/{0}.cshtml"};

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(rve);
        }
    }
}
