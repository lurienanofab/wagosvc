using LNF.Control;

namespace WagoService.Actions
{
    public class PointAction : ControlActionBase
    {
        public int PointID { get; }
        public bool State { get; }

        public PointAction(int pointId, bool state)
        {
            PointID = pointId;
            State = state;
        }

        public override ControlResponse Execute(IControlConnection connection)
        {
            var pointResponse = connection.SendSetPointStateCommand(PointID, State);
            return pointResponse;
        }

        //public override int BlockID => Point.Block.BlockID;
        //public Point Point { get; private set; }

        //public bool State { get; private set; }

        //public override int PointID
        //{
        //    get { return Point.PointID; }
        //}

        //public zPointAction(Point point, bool state)
        //{
        //    Point = point;
        //    State = state;
        //}

        //public override ControlResponse ExecuteCommand(IControlConnection connection)
        //{
        //    return connection.SendSetPointStateCommand(this);
        //}

        //public override string GetLogMessage()
        //{
        //    return string.Format("SetPointStateAction:{0}:PointID={1}:State={2}:{3:N}", Point.Block.BlockName, Point.PointID, State, MessageID);
        //}

        //public override byte[] GetSendBuffer()
        //{
        //    throw new NotImplementedException();
        //}

        //public override ControlResponse GetResponse(Block block, int bytesRecv, byte[] recvBuffer)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
