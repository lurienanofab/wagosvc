using LNF;
using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;
using System.Collections.Generic;
using System.Linq;
using WagoService.Actions;

namespace WagoService.Test
{
    public class TestConnection : IControlConnection
    {
        private readonly IEnumerable<PointState> _points;

        public TestConnection()
        {
            using (Providers.DataAccess.StartUnitOfWork())
                _points = DA.Current.Query<Point>().ToList().Select(x => x.CreatePointState(false)).ToArray();
        }

        public BlockResponse SendGetBlockStateCommand(BlockAction action)
        {
            BlockResponse result = action.Block.CreateBlockResponse();
            result.BlockState.Points = _points.Where(x => x.BlockID == action.Block.BlockID).ToArray();
            return result;
        }

        public PointResponse SendSetPointStateCommand(PointAction action)
        {
            PointResponse result = action.Point.CreatePointResponse();
            PointState ps = _points.FirstOrDefault(x => x.PointID == action.Point.PointID);

            if (ps != null)
                ps.State = action.State;
            else
            {
                result.Error = true;
                result.Message = string.Format("Point {0} not found on Block {1}.", action.Point.PointID, action.Point.Block.BlockID);
            }

            return result;
        }
    }
}
