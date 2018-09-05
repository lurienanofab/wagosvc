using LNF.Control;
using LNF.Repository.Control;
using WagoService.Actions;

namespace WagoService
{
    public interface IControlConnection
    {
        BlockResponse SendGetBlockStateCommand(int blockId);

        PointResponse SendSetPointStateCommand(int pointId, bool state);
    }
}
