using LNF;
using LNF.Impl.DependencyInjection.Default;
using Owin;
using System.Web.Http;

namespace WagoService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();
            HttpConfiguration config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);
        }
    }
}