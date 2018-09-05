using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;
using System;
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
            _points = DA.Current.Query<Point>().ToList().Select(x => x.CreatePointState(false)).ToArray();
        }

        public ControlResponse Execute(Block block, IControlAction action)
        {
            throw new NotImplementedException();
        }

        public BlockResponse SendGetBlockStateCommand(int blockId)
        {
            Block block = DA.Current.Single<Block>(blockId);

            try
            {
                BlockResponse result = block.CreateBlockResponse();
                result.BlockState.Points = _points.Where(x => x.BlockID == block.BlockID).ToArray();
                return result;
            }
            catch (Exception ex)
            {
                return block.CreateBlockResponse(ex);
            }
        }

        public PointResponse SendSetPointStateCommand(int pointId, bool state)
        {
            var point = DA.Current.Single<Point>(pointId);
            var result = point.CreatePointResponse();
            var ps = _points.FirstOrDefault(x => x.PointID == point.PointID);

            if (ps != null)
                ps.State = state;
            else
            {
                result.Error = true;
                result.Message = string.Format("Point {0} not found on Block {1}.", point.PointID, point.Block.BlockID);
            }

            return result;
        }
    }
}
