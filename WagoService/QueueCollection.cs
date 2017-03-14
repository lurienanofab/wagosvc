using LNF.Repository;
using LNF.Repository.Control;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System;
using System.Linq;

namespace WagoService
{
    public class QueueCollection
    {
        private static readonly QueueCollection _Current;

        static QueueCollection()
        {
            _Current = new QueueCollection();
        }

        public static QueueCollection Current
        {
            get { return _Current; }
        }

        private readonly ConcurrentDictionary<int, IRequestQueue> _items;

        private QueueCollection()
        {
            _items = new ConcurrentDictionary<int, IRequestQueue>();
        }

        public static int WagoTimeout
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["WagoServiceTimeout"]);
            }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public void StartQueues(IEnumerable<Block> blocks)
        {
            _items.Clear();
            foreach (Block b in blocks)
            {
                var queue = IOC.Container.GetInstance<IRequestQueue>();
                queue.Start(b);
                _items.TryAdd(b.BlockID, queue);
            }
        }

        public void StopQueues()
        {
            foreach (int key in _items.Keys)
            {
                IRequestQueue q;
                if (_items.TryRemove(key, out q))
                    q.Stop();
            }
        }

        public IRequestQueue Item(int blockId)
        {
            IRequestQueue result;
            _items.TryGetValue(blockId, out result);
            return result;
        }
    }
}
