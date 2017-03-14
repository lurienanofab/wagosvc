using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using WagoService.Actions;

namespace WagoService
{
    public sealed class ControlService : IControlService
    {
        public BlockResponse GetBlockState(Block block)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            if (block.Points.Count == 0)
                block = DA.Current.Single<Block>(block.BlockID);

            // Compose Action object
            BlockAction action = new BlockAction(block);

            try
            {
                // Get RequestQueue, and push data into queue
                IRequestQueue queue = QueueCollection.Current.Item(action.Block.BlockID);

                if (queue == null)
                    throw new InvalidOperationException(string.Format("Could not find queue with BlockID {0}", action.Block.BlockID));

                // Send request for block state
                queue.Push(action);

                // Wait a bit for the the response to appear
                BlockResponse result = (BlockResponse)queue.GetResponse(action.MessageID);

                return result;
            }
            catch (Exception ex)
            {
                return action.Block.CreateBlockResponse(ex);
            }
        }

        public PointResponse SetPointState(Point point, bool state, uint duration)
        {
            if (point.Block == null)
                point = DA.Current.Single<Point>(point.PointID);

            // Compose Action object
            PointAction action = new PointAction(point, state);

            try
            {
                // SetPointState messages do not have a return value because we do not wait for any data back from Wago like we do in GetBlockState.

                // Get RequestQueue, and push data into queue
                IRequestQueue queue = QueueCollection.Current.Item(action.Point.Block.BlockID);

                if (queue == null)
                    throw new InvalidOperationException(string.Format("Could not find queue with BlockID {0}", action.Point.Block.BlockID));

                // Send request for block state
                queue.Push(action);

                // Wait a bit for the the response to appear
                PointResponse result = (PointResponse)queue.GetResponse(action.MessageID);

                // When duration is zero only send one message otherwise send the first message and then send another using a Timer.
                if (!result.Error && duration > 0)
                {
                    PointAction futureAction = new PointAction(action.Point, !action.State);
                    HandleFutureAction(queue, action, futureAction, duration);
                }

                return result;
            }
            catch (Exception ex)
            {
                return action.Point.CreatePointResponse(ex);
            }
        }

        public PointResponse Cancel(Point point)
        {
            PointResponse resp = point.CreatePointResponse();

            try
            {
                PointTimer pt;
                if (FutureActionManager.Current.TryRemove(point.PointID, out pt))
                {
                    // Do it immediately
                    pt.Cancel(true);

                    // Cancelled timers will still execute the handler but by the time they do the corresponding 
                    // PointTimer object will already have been removed from the timers ConcurrentDictionary, or
                    // Cancel will be true so when the handler is finally called at that time nothing will happen.
                }
                else
                {
                    resp.Error = true;
                    resp.Message = "No pending action found to cancel";
                }

                return resp;
            }
            catch (Exception ex)
            {
                return point.CreatePointResponse(ex);
            }
        }

        public Point GetPoint(int pointId)
        {
            return DA.Current.Single<Point>(pointId);
        }

        public IQueryable<Point> GetAllPoints()
        {
            return DA.Current.Query<Point>();
        }

        public Block GetBlock(int blockId)
        {
            return DA.Current.Single<Block>(blockId);
        }

        public IQueryable<Block> GetAllBlocks()
        {
            return DA.Current.Query<Block>();
        }

        public ActionInstance GetInstance(ActionType action, int actionId)
        {
            string actionName = Enum.GetName(typeof(ActionType), action);
            return DA.Current.Query<ActionInstance>().FirstOrDefault(x => x.ActionName == actionName && x.ActionID == actionId);
        }

        public IEnumerable<BlockConfig> GetBlockConfigs()
        {
            return DA.Current.Query<BlockConfig>().ToList();
        }

        public IEnumerable<ModType> GetModTypes()
        {
            return DA.Current.Query<ModType>().ToList();
        }

        private void HandleFutureAction(IRequestQueue queue, IControlAction action, IControlAction futureAction, uint duration)
        {
            //send the future message

            //cancel any existing timers for this point
            PointTimer pointTimer;
            if (FutureActionManager.Current.TryRemove(action.ActionID, out pointTimer))
                pointTimer.Cancel(false);

            //create a new PointTimer for this point
            pointTimer = new PointTimer(queue, futureAction, duration);

            //keep track of this timer in case it is cancelled
            FutureActionManager.Current.TryAdd(pointTimer);
        }
    }
}
