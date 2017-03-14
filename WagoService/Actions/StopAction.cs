using LNF.Control;
using System;

namespace WagoService.Actions
{
    public class StopResponse : ControlResponse { }

    public class StopAction : IControlAction
    {
        public int ActionID
        {
            get { return 0; }
        }

        public Guid MessageID { get; private set; }

        public DateTime TimeStamp { get; private set; }

        public StopAction()
        {
            MessageID = Guid.NewGuid();
            TimeStamp = DateTime.Now;
        }

        public ControlResponse ExecuteCommand(IControlConnection service)
        {
            return new StopResponse();
        }

        public string GetLogMessage()
        {
            return string.Format("Stop:{0:N}", MessageID);
        }
    }
}
