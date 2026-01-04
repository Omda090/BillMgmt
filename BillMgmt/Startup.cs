using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BillMgmt.Startup))]
namespace BillMgmt
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
