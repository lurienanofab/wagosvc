using LNF.Control;
using System;

namespace WagoService.Actions
{
    public interface IControlAction
    {
        Guid MessageID { get; }
        int ActionID { get; }
        DateTime TimeStamp { get; }
        ControlResponse ExecuteCommand(IControlConnection service);
        string GetLogMessage();
    }
}
