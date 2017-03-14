using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LNF.Control;

namespace WagoService.Actions
{
    public abstract class ControlActionBase : IControlAction
    {
        public Guid MessageID { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public abstract int ActionID { get; }

        public ControlActionBase()
        {
            MessageID = Guid.NewGuid();
            TimeStamp = DateTime.Now;
        }

        public abstract ControlResponse ExecuteCommand(IControlConnection service);

        public abstract string GetLogMessage();
    }
}
