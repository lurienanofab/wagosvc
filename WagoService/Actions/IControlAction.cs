using LNF.Control;
using LNF.Repository.Control;
using System;

namespace WagoService.Actions
{
    public interface IControlAction
    {
        Guid MessageID { get; }
        DateTime Timestamp { get; }
        ControlResponse Execute(IControlConnection connection);
        //int BlockID { get; }
        //int PointID { get; }
        //DateTime TimeStamp { get; }
        //string GetLogMessage();
        //ControlResponse ExecuteCommand(IControlConnection connection);
        //byte[] GetSendBuffer();
        //ControlResponse GetResponse(Block block, int bytesRecv, byte[] recvBuffer);
    }
}
