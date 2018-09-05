using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WagoService
{
    public class FutureActionManager
    {
        private static readonly FutureActionManager _Current;

        static FutureActionManager()
        {
            _Current = new FutureActionManager();
        }

        public static FutureActionManager Current
        {
            get { return _Current; }
        }

        private readonly ConcurrentDictionary<int, PointTimer> _timers = new ConcurrentDictionary<int, PointTimer>();

        public bool TryAdd(PointTimer pt)
        {
            return _timers.TryAdd(pt.Action.PointID, pt);
        }

        public bool TryRemove(int pointId, out PointTimer pt)
        {
            return _timers.TryRemove(pointId, out pt);
        }

        public bool ContainsKey(int pointId)
        {
            return _timers.ContainsKey(pointId);
        }
    }
}
