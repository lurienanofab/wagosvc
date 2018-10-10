using LNF;
using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;
using System;
using System.Web.Http;
using WagoService.Actions;

namespace WagoService.Controllers
{
    public class MainController : ApiController, IService
    {
        [Route("wago")]
        public string Get()
        {
            return "wago-service";
        }

        [HttpGet, Route("wago/block/{blockId}")]
        public BlockResponse GetBlockState(int blockId)
        {
            // Compose Action object
            BlockAction action = new BlockAction(blockId);

            // Get RequestQueue, and push data into queue
            IRequestQueue queue = QueueCollection.Current.Item(blockId);

            if (queue == null)
                throw new InvalidOperationException($"Could not find queue with BlockID {blockId}");

            // Send request for block state
            queue.Push(action);

            // Wait a bit for the the response to appear
            BlockResponse result = (BlockResponse)queue.GetResponse(action.MessageID);

            return result;
        }

        [HttpGet, Route("wago/point/{pointId}")]
        public PointResponse SetPointState(int pointId, bool state, uint duration = 0)
        {
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                var point = DA.Current.Single<Point>(pointId);
                return SetPointState(point.Block.BlockID, point.PointID, state, duration);
            }
        }

        [HttpGet, Route("wago/block/{blockId}/point/{pointId}")]
        public PointResponse SetPointState(int blockId, int pointId, bool state, uint duration = 0)
        {
            // This method is better because it doesn't need NHibernate

            // Compose Action object
            PointAction action = new PointAction(pointId, state);

            // SetPointState messages do not have a return value because we do not wait for any data back from Wago like we do in GetBlockState.

            // Get RequestQueue, and push data into queue
            IRequestQueue queue = QueueCollection.Current.Item(blockId);

            if (queue == null)
                throw new InvalidOperationException($"Could not find queue with BlockID {blockId}");

            // Send request for block state
            queue.Push(action);

            // Wait a bit for the the response to appear
            PointResponse result = (PointResponse)queue.GetResponse(action.MessageID);

            // When duration is zero only send one message otherwise send the first message and then send another using a Timer.
            if (!result.Error && duration > 0)
            {
                PointAction futureAction = new PointAction(pointId, !action.State);
                HandleFutureAction(queue, action, futureAction, duration);
            }

            return result;
        }

        [HttpGet, Route("wago/point/{pointId}/cancel")]
        public PointResponse Cancel(int pointId)
        {
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                var point = DA.Current.Single<Point>(pointId);
                return Cancel(point.Block.BlockID, point.PointID);
            }
        }

        [HttpGet, Route("wago/block/{blockId}/point/{pointId}/cancel")]
        public PointResponse Cancel(int blockId, int pointId)
        {
            // This method is better because it doesn't need NHibernate

            var result = new PointResponse()
            {
                BlockID = blockId,
                PointID = pointId,
                Error = false,
                Message = string.Empty,
                StartTime = DateTime.Now
            };

            if (FutureActionManager.Current.TryRemove(pointId, out PointTimer pt))
            {
                // Do it immediately
                pt.Cancel(true);

                // Cancelled timers will still execute the handler but by the time they do the corresponding 
                // PointTimer object will already have been removed from the timers ConcurrentDictionary, or
                // Cancel will be true so when the handler is finally called at that time nothing will happen.
            }
            else
            {
                result.Error = true;
                result.Message = "No pending action found to cancel";
            }

            return result;
        }

        private void HandleFutureAction(IRequestQueue queue, PointAction action, PointAction futureAction, uint duration)
        {
            //send the future message

            //cancel any existing timers for this point
            if (FutureActionManager.Current.TryRemove(action.PointID, out PointTimer pointTimer))
                pointTimer.Cancel(false);

            //create a new PointTimer for this point
            pointTimer = new PointTimer(queue, futureAction, duration);

            //keep track of this timer in case it is cancelled
            FutureActionManager.Current.TryAdd(pointTimer);
        }
    }
}
