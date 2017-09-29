using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Wirehome.CloudApi.Startup))]

namespace Wirehome.CloudApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
