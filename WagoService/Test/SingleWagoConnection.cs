using LNF.Repository.Control;
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
    }
}
