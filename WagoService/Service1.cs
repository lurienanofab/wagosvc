using LNF;
using LNF.Repository;
using LNF.Repository.Control;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace WagoService
{
    public partial class Service1 : ServiceBase
    {
        public readonly static string InstallServiceName = "WagoService";

        private Host _host;

        private IDisposable _webapp;

        public Service1()
        {
            InitializeComponent();
        }

        public string GetServiceUri()
        {
            return ConfigurationManager.AppSettings["ServiceHostHttp"];
        }

        public void Start(string[] args)
        {
            OnStart(args);
            Program.ConsoleWriteLine("endpoint: {0}/wago", GetServiceUri());
            Program.ConsoleWriteLine("endpoint: {0}", _host.EndPoint);
            Program.ConsoleWriteLine("Press any key to exit.");
        }

        protected override void OnStart(string[] args)
        {
            using (Providers.DataAccess.StartUnitOfWork())
            {
                Log.Start();

                IList<Block> blocks = DA.Current.Query<Block>().ToList();

                QueueCollection.Current.StartQueues(blocks);

                _webapp = WebApp.Start<Startup>(GetServiceUri());

                _host = new Host();
                _host.Start<ControlService>();
            }
        }

        protected override void OnStop()
        {
            if (_webapp != null)
                _webapp.Dispose();

            QueueCollection.Current.StopQueues();

            Log.Stop();
        }

        public static bool IsServiceInstalled()
        {
            return ServiceController.GetServices().Any(s => s.ServiceName == InstallServiceName);
        }

        public static void InstallService()
        {
            if (IsServiceInstalled())
            {
                UninstallService();
            }

            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
        }

        public static void UninstallService()
        {
            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
        }
    }
}
