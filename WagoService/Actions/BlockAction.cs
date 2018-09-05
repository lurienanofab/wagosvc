using LNF.Control;

namespace WagoService.Actions
{
    public class BlockAction : ControlActionBase
    {
        public int BlockID { get; }

        public BlockAction(int blockId)
        {
            BlockID = blockId;
        }

        public override ControlResponse Execute(IControlConnection connection)
        {
            var blockResponse = connection.SendGetBlockStateCommand(BlockID);
            return blockResponse;
        }

        //public override int BlockID { get; }
        //public override int PointID => 0;

        //public zBlockAction(int blockId)
        //{
        //    BlockID = blockId;
        //}

        //public override ControlResponse ExecuteCommand(IControlConnection connection)
        //{
        //    return connection.SendGetBlockStateCommand(BlockID);
        //}

        //public override string GetLogMessage()
        //{
        //    return string.Format("GetBlockStateAction:BlockID={0}:{1:N}", BlockID, MessageID);
        //}

        //public override byte[] GetSendBuffer()
        //{
        //    byte[] result = new byte[65];
        //    result[0] = 0x2;
        //    result[1] = 0x3;

        //    Log.Write(BlockID, "WagoConnection: Sending GetBlockState message to block: BlockID = {0}, Data = {1}", BlockID, WagoUtility.GetDataString(result));

        //    return result;
        //}

        //public override ControlResponse GetResponse(Block block, int bytesRecv, byte[] recvBuffer)
        //{
        //    if (bytesRecv > 0)
        //        Log.Write(BlockID, "WagoConnection: Received {0} bytes [{1}] from block {2}", bytesRecv, WagoUtility.BytesToString(recvBuffer, bytesRecv), BlockID);
        //    else
        //        Log.Write(BlockID, "WagoConnection: Block {0} did not return any data", BlockID);

        //    BlockResponse result = block.CreateBlockResponse();
        //    result.BlockState.Points = block.Points.Select(x => WagoUtility.GetPointState(x, recvBuffer)).ToArray();

        //    return result;
        //}

    }
}
