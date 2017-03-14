using LNF.Control;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace WagoService
{
    public class Host
    {
        private ServiceHost _host;

        public Exception LastException { get; private set; }
        public Uri EndPoint { get; private set; }

        public Uri GetServiceUri()
        {
            string setting = ConfigurationManager.AppSettings["ServiceHostTcp"];
            Uri baseUri = new Uri(setting);
            return new Uri(baseUri, "WagoService");
        }

        public bool Start<T>() where T : IControlService
        {
            return Start(typeof(T));
        }

        public bool Start(Type serviceType)
        {
            Type interfaceType = typeof(IControlService);
            if (!interfaceType.IsAssignableFrom(serviceType))
                throw new ArgumentException(string.Format("The service type must implement {0}", interfaceType.FullName), "serviceType");

            bool result = true;

            _host = new ServiceHost(serviceType);

            try
            {
                EndPoint = GetServiceUri();

                if (EndPoint != null)
                    _host.AddServiceEndpoint(interfaceType, new NetTcpBinding(SecurityMode.None), EndPoint);
                else
                    throw new InvalidOperationException("EndPoint cannot be null.");

                var debugBehavior = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (debugBehavior == null)
                    _host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                else
                    debugBehavior.IncludeExceptionDetailInFaults = true;

                _host.Open();
            }
            catch (Exception ex)
            {
                LastException = ex;
                result = false;
            }

            return result;
        }

        public void Stop()
        {
            _host.Close();
        }
    }
}
