using LNF.Control;

namespace WagoService.Actions
{
    public class StopResponse : ControlResponse { }

    public class StopAction : ControlActionBase
    {
        public override ControlResponse Execute(IControlConnection connection)
        {
            return new StopResponse();
        }
    }
}
