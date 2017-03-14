using LNF.Control;
using System.Web.Http;
using WagoService.Actions;

namespace WagoService.Controllers
{
    public class MainController : ApiController
    {
        private ControlService _manager;

        public MainController(ControlService manager)
        {
            _manager = manager;
        }

        [Route("wago")]
        public string Get()
        {
            return "wago-service";
        }

        [HttpGet, Route("wago/block/{blockId}")]
        public BlockResponse GetBlockState(int blockId)
        {
            var block = _manager.GetBlock(blockId);
            return _manager.GetBlockState(block);
        }

        [HttpGet, Route("wago/point/{pointId}")]
        public PointResponse SetPointState(int pointId, bool state, uint duration = 0)
        {
            var point = _manager.GetPoint(pointId);
            return _manager.SetPointState(point, state, duration);
        }

        [HttpGet, Route("wago/point/{pointId}/cancel")]
        public PointResponse Cancel(int pointId)
        {
            var point = _manager.GetPoint(pointId);
            return _manager.Cancel(point);
        }
    }
}
