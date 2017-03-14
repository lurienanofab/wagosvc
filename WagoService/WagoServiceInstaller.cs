using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace WagoService
{
    [RunInstaller(true)]
    public partial class WagoServiceInstaller : Installer
    {
        public WagoServiceInstaller()
        {
            InitializeComponent();

            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.ServiceName = Service1.InstallServiceName;
            serviceInstaller.DisplayName = Service1.InstallServiceName;
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
