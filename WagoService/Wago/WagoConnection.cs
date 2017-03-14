using LNF.Control;
using LNF.Repository.Control;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using WagoService.Actions;

namespace WagoService.Wago
{
    public class WagoConnection : IControlConnection
    {
        private Socket Connect(Block block)
        {
            // Create a TCP/IP socket for block communications
            try
            {
                Socket result = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                result.Connect(CreateEndPoint(block));
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to connect to block: {0}[{1}]{2}-----{2}{3}", block.BlockName, block.IPAddress, Environment.NewLine, ex));
            }
        }

        public BlockResponse SendGetBlockStateCommand(BlockAction action)
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
                result.BlockState.Points = action.Block.Points.Select(x => WagoUtility.GetPointState(x, recvBuffer)).ToArray();

                return result;
            }
        }

        public PointResponse SendSetPointStateCommand(PointAction action)
        {
            using (var sender = Connect(action.Point.Block))
            {
                byte[] buffer = CreateSetPointStateBuffer(action.Point, action.State);

                Log.Write(action.Point.Block.BlockID, "WagoConnection: Sending SetPointState message to block: BlockID = {0}, PointID = {1}, Data = {2}",
                    action.Point.Block.BlockID, action.Point.PointID, WagoUtility.GetDataString(buffer));

                byte[] recvBuffer = new byte[65];

                int bytesRecv = SendMessageToBlock(sender, buffer, recvBuffer, false);

                // the recvBuffer is not used for SetPointState because there is no response

                PointResponse result = action.Point.CreatePointResponse();

                return result;
            }
        }

        private int SendMessageToBlock(Socket socket, byte[] buffer, byte[] recvBuffer, bool hasReturnValue)
        {
            // Encode the data string into a byte array.
            if (socket.Connected)
            {
                byte[] sendBuffer = new byte[65];
                int bytesRecv = 0;

                // Send/receive get block configuration command - check send/recv for each
                sendBuffer[0] = 0x2;
                sendBuffer[1] = 0x0;

                bool wagoCheck = false;

                if (wagoCheck)
                {
                    if (SendAndReceivePacket(socket, sendBuffer, recvBuffer, true) >= 0)
                    {
                        //do something for WAGO's PI check in future
                    }
                    else
                    {
                        // Error, we shutdown and return immediately
                        throw new Exception("Received 0 bytes on first packet send/receive");
                    }
                }

                // Send / Receive commands
                bytesRecv = SendAndReceivePacket(socket, buffer, recvBuffer, hasReturnValue);

                //The error "connection forcibly closed" occurs when multiple requests come in too fast.
                //I think the wago needs a half second to recover and be ready for the next request.
                Thread.Sleep(500);

                //Compose return message
                return bytesRecv;
            }
            else
                throw new Exception("Socket is not connected");
        }

        protected virtual IPEndPoint CreateEndPoint(Block block)
        {
            return new IPEndPoint(IPAddress.Parse(block.IPAddress), 7777);
        }

        private int SendAndReceivePacket(Socket socket, byte[] sendBuffer, byte[] recvBuffer, bool hasReturnValue)
        {
            int bytesRecv = 0;
            int bytesSent = socket.Send(sendBuffer, 0, sendBuffer[0], SocketFlags.None);

            if (hasReturnValue)
            {
                socket.ReceiveTimeout = 3000;
                bytesRecv = socket.Receive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None);

                if (bytesRecv <= 0)
                {
                    if (bytesSent == 2)
                        throw new Exception(string.Format("Receiving packet failed during first packet, {0} bytes received, {1} bytes sent", bytesRecv, bytesSent));
                    else if (bytesSent == 7)
                        throw new Exception(string.Format("Receiving packet failed during second packet, {0} bytes received, {1} bytes sent", bytesRecv, bytesSent));
                    else
                        throw new Exception(string.Format("Receiving packet failed, {0} bytes received, {1} bytes sent", bytesRecv, bytesSent));
                }
                else
                    return bytesRecv;
            }

            //Sucesss without needing the return data
            return 0;
        }

        private byte[] CreateGetBlockStateBuffer()
        {
            byte[] result = new byte[65];
            result[0] = 0x2;
            result[1] = 0x3;
            return result;
        }

        protected virtual byte[] CreateSetPointStateBuffer(Point point, bool state)
        {
            // When setting point state filterDirection must be true!!
            WagoAddress addr = WagoUtility.MapToMemory(point, filterDirection: true);
            byte[] result = addr.GetBuffer(state);
            return result;
        }
    }
}
