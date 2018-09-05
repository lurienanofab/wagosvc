using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;
using System;
using System.Linq;
using System.Net;
using WagoService.Wago;

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

        public override BlockResponse SendGetBlockStateCommand(int blockId)
        {
            var block = DA.Current.Single<Block>(blockId);

            try
            {
                using (var sender = Connect(block))
                {
                    byte[] buffer = CreateGetBlockStateBuffer();

                    Log.Write(block.BlockID, $"WagoConnection: Sending GetBlockState message to block: BlockID = {block.BlockID}, Data = {WagoUtility.GetDataString(buffer)}");

                    byte[] recvBuffer = new byte[65];

                    int bytesRecv = SendMessageToBlock(sender, buffer, recvBuffer, true);

                    if (bytesRecv > 0)
                        Log.Write(block.BlockID, $"WagoConnection: Received {bytesRecv} bytes [{WagoUtility.BytesToString(recvBuffer, bytesRecv)}] from block {block.BlockID}");
                    else
                        Log.Write(block.BlockID, $"WagoConnection: Block {block.BlockID} did not return any data");

                    BlockResponse result = block.CreateBlockResponse();

                    var testBlock = DA.Current.Query<Block>().First(x => x.BlockName == "wago_test");

                    result.BlockState.Points = block.Points.Select(x =>
                    {
                        var p = new Point() { Block = testBlock, ModPosition = 0, Name = x.Name, Offset = x.Offset, PointID = x.PointID };
                        return WagoUtility.GetPointState(p, recvBuffer);
                    }).ToArray();

                    return result;
                }
            }
            catch (Exception ex)
            {
                return block.CreateBlockResponse(ex);
            }
        }
    }
}
