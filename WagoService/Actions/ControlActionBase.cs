using LNF.Control;
using System;

namespace WagoService.Actions
{
    public abstract class ControlActionBase : IControlAction
    {
        public Guid MessageID { get; }
        public DateTime Timestamp { get; }

        public ControlActionBase()
        {
            MessageID = Guid.NewGuid();
            Timestamp = DateTime.Now;
        }

        public abstract ControlResponse Execute(IControlConnection connection);
    }
}
