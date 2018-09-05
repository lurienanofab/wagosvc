using LNF;
using LNF.Repository.Control;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;

namespace WagoService
{
    public class QueueCollection
    {
        static QueueCollection()
        {
            Current = new QueueCollection();
        }

        public static QueueCollection Current { get; }

        private readonly ConcurrentDictionary<int, IRequestQueue> _items;

        private readonly IControlConnection _controlConnection;

        private QueueCollection()
        {
            _controlConnection = GetControlConnection();
            _items = new ConcurrentDictionary<int, IRequestQueue>();
        }

        public static int WagoTimeout => int.Parse(ConfigurationManager.AppSettings["WagoServiceTimeout"]);

        public int Count => _items.Count;

        public void StartQueues(IEnumerable<Block> blocks)
        {
            _items.Clear();
            foreach (Block b in blocks)
            {
                var queue = new RequestQueue(_controlConnection);
                queue.Start(b);
                _items.TryAdd(b.BlockID, queue);
            }
        }

        public void StopQueues()
        {
            foreach (int key in _items.Keys)
            {
                if (_items.TryRemove(key, out IRequestQueue q))
                    q.Stop();
            }
        }

        public IRequestQueue Item(int blockId)
        {
            _items.TryGetValue(blockId, out IRequestQueue result);
            return result;
        }

        private IControlConnection GetControlConnection()
        {
            string key = ServiceProvider.Current.IsProduction() ? "ControlConnectionProduction" : "ControlConnectionDevelopment";
            string setting = ConfigurationManager.AppSettings[key];
            var type = Type.GetType(setting);
            return (IControlConnection)Activator.CreateInstance(type);
        }
    }
}
