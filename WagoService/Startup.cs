using LNF;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System;
using System.Configuration;
using System.Web.Http;
using WagoService.Actions;

namespace WagoService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(IOC.Container);

            config.MapHttpAttributeRoutes();

            app.CreatePerOwinContext(Providers.DataAccess.StartUnitOfWork);

            app.UseWebApi(config);
        }
    }

    public static class IOC
    {
        public static Container Container { get; private set; }

        static IOC()
        {
            Container = new Container();
            Container.Register(typeof(IControlConnection), GetConnectionType(), Lifestyle.Singleton);
            Container.Register<IRequestQueue, RequestQueue>(Lifestyle.Transient);
            Container.Verify();
        }
        
        public static Type GetConnectionType()
        {
            string key = Providers.IsProduction() ? "ControlConnectionProduction" : "ControlConnectionDevelopment";

            string typeName = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(typeName))
                throw new InvalidOperationException(string.Format("Missing AppSetting: {0}", key));

            Type result = Type.GetType(typeName);

            if (result == null)
                throw new InvalidOperationException(string.Format("Unable to get type: {0}", typeName));

            return result;
        }
    }
}