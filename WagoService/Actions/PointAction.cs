using LNF.Control;
using LNF.Repository.Control;
using System;

namespace WagoService.Actions
{
    public class PointAction : ControlActionBase
    {
        public Point Point { get; private set; }

        public bool State { get; private set; }

        public override int ActionID
        {
            get { return Point.PointID; }
        }

        public PointAction(Point point, bool state)
        {
            Point = point;
            State = state;
        }

        public override ControlResponse ExecuteCommand(IControlConnection service)
        {
            return service.SendSetPointStateCommand(this);
        }

        public override string GetLogMessage()
        {
            return string.Format("SetPointStateAction:{0}:PointID={1}:State={2}:{3:N}", Point.Block.BlockName, Point.PointID, State, MessageID);
        }
    }
}
