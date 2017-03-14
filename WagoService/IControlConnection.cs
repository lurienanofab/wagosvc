using LNF.Control;
using WagoService.Actions;

namespace WagoService
{
    public interface IControlConnection
    {
        BlockResponse SendGetBlockStateCommand(BlockAction action);

        PointResponse SendSetPointStateCommand(PointAction action);
    }
}
