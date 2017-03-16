using LNF.Control;
using LNF.Repository.Control;
using System.Linq;
using System.Net;
using WagoService.Actions;
using WagoService.Wago;
using LNF.Repository;

namespace WagoService.Test
{
    public class SingleWagoConnection : WagoConnection
    {
        protected override IPEndPoint CreateEndPoint(Block block)
        {
            return new IPEndPoint(IPAddress.Parse("192.168.1.112"), 7777);
        }

        protected override byte[] CreateSetPointStateBuffer(Point point, bool state)
        {
            var p = new Point()
            {
                PointID = point.PointID,
                Block = point.Block,
                ModPosition = 0,
                Offset = point.Offset,
                Name = point.Name
            };

            return base.CreateSetPointStateBuffer(p, state);
        }

        public override BlockResponse SendGetBlockStateCommand(BlockAction action)
        {
            using (var sender = Connect(action.Block))
            {
                byte[] buffer = CreateGetBlockStateBuffer();

                Log.Write(action.Block.BlockID, "WagoConnection: Sending GetBlockState message to block: BlockID = {0}, Data = {1}",
                    action.Block.BlockID, WagoUtility.GetDataString(buffer));

                byte[] recvBuffer = new byte[65];

                int bytesRecv = SendMessageToBlock(sender, buffer, recvBuffer, true);

                if (bytesRecv > 0)
                    Log.Write(action.Block.BlockID, "WagoConnection: Received {0} bytes [{1}] from block {2}", bytesRecv, WagoUtility.BytesToString(recvBuffer, bytesRecv), action.Block.BlockID);
                else
                    Log.Write(action.Block.BlockID, "WagoConnection: Block {0} did not return any data", action.Block.BlockID);

                BlockResponse result = action.Block.CreateBlockResponse();

                var b = DA.Current.Query<Block>().First(x => x.BlockName == "wago_test");

                result.BlockState.Points = action.Block.Points.Select(x =>
                {
                    var p = new Point() { Block = b, ModPosition = 0, Name = x.Name, Offset = x.Offset, PointID = x.PointID };
                    return WagoUtility.GetPointState(p, recvBuffer);
                }).ToArray();

                return result;
            }
        }
    }
}
