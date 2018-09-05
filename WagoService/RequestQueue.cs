using LNF;
using LNF.Control;
using LNF.Repository.Control;
using System;
using System.Collections.Concurrent;
using System.Threading;
using WagoService.Actions;

namespace WagoService
{
    public interface IRequestQueue
    {
        event EventHandler<ControlActionEventArgs> RequestReceived;
        Block Block { get; }
        void Start(Block block);
        void Stop();
        void Push(IControlAction action);
        ControlResponse GetResponse(Guid messageID);
    }

    public class RequestQueue : IRequestQueue
    {
        public event EventHandler<ControlActionEventArgs> RequestReceived;

        private readonly object locker = new object();
        private IControlAction _lastMessage;

        // Each queue has one action collection so all types of actions must be the same Type.
        // So the challenge is how to create a class that handles GetBlockState message and the
        // SetPointState actions. 
        private BlockingCollection<IControlAction> _queue = new BlockingCollection<IControlAction>();

        //we are not using another MessageQueue here because a ConcurrentDictionary allows searching so we can
        //get a particular message (based on MessageID) not just whichever one happens to be next in the queue
        private ControlResponseCollection _responses = new ControlResponseCollection();

        private readonly IControlConnection _connection;

        public Block Block { get; private set; }

        public RequestQueue(IControlConnection connection)
        {
            _connection = connection;
        }

        public void Start(Block block)
        {
            Block = block;

            new Thread(() =>
            {
                Log.Write(Block.BlockID, $"Started thread for queue {Block.BlockName}");

                while (true)
                {
                    IControlAction action = null;
                    ControlResponse response;

                    try
                    {

                        action = _queue.Take();

                        Log.Write(Block.BlockID, $"RECV:{action.GetType().Name}:{action.MessageID}");

                        RequestReceived?.Invoke(this, new ControlActionEventArgs() { ID = action.MessageID, TimeStamp = action.Timestamp, MessageType = "Request" });

                        if (action is StopAction) break;

                        //process the message
                        using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
                        {
                            response = action.Execute(_connection);
                        }

                        _responses.Add(action.MessageID, response);

                        lock (locker)
                        {
                            _lastMessage = action;
                        }
                    }
                    catch (Exception ex)
                    {
                        // If a return message is expected make sure one is sent back even when an error occurred.
                        // There is no way to get a message back via _responses if action is null because we don't
                        // know which MessageID to use. The only thing we can do in this case is just log it.

                        if (action != null)
                        {
                            response = new ErrorResponse(ex);
                            _responses.Add(action.MessageID, response);
                        }

                        Log.Write(Block.BlockID, ex.ToString());
                    }
                }
            }).Start();
        }

        public void Stop()
        {
            var action = new StopAction();
            Push(action);
        }

        public void Push(IControlAction action)
        {
            //put a WagoMessage on the queue
            Log.Write(Block.BlockID, $"SEND:{action.GetType().Name}:{action.MessageID}");

            _queue.Add(action);

            //when WagoService sees the message it will communicate with the wago block and add an entry to the response collection
        }

        public ControlResponse GetResponse(Guid id)
        {
            DateTime cutoff = DateTime.Now.AddSeconds(QueueCollection.WagoTimeout);

            while (DateTime.Now < cutoff)
            {
                if (_responses.TryRemove(id, out ControlResponse result))
                {
                    EnsureSuccess(result);
                    return result;
                }
            }

            throw new TimeoutException(string.Format("A timeout occured while waiting for message {0}", id));
        }

        private void EnsureSuccess(ControlResponse response)
        {
            if (response is ErrorResponse err)
                throw err.Exception;
        }

        public IControlAction GetLastMessage()
        {
            lock (locker)
            {
                return _lastMessage;
            }
        }
    }
}
