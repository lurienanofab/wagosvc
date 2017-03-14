using LNF.Control;
using LNF.Repository.Control;
using System;

namespace WagoService.Actions
{
    public class BlockAction : ControlActionBase
    {
        public Block Block { get; private set; }

        public override int ActionID
        {
            get { return Block.BlockID; }
        }

        public BlockAction(Block block)
        {
            Block = block;
        }

        public override ControlResponse ExecuteCommand(IControlConnection service)
        {
            return service.SendGetBlockStateCommand(this);
        }

        public override string GetLogMessage()
        {
            return string.Format("GetBlockStateAction:{0}:BlockID={1}:{2:N}", Block.BlockName, Block.BlockID, MessageID);
        }
    }
}
