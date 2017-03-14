using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LNF.Control;

namespace WagoService
{
    public class ControlResponseCollection : IEnumerable<KeyValuePair<Guid, ControlResponse>>
    {
        private ConcurrentDictionary<Guid, ControlResponse> items;

        public ControlResponseCollection()
        {
            items = new ConcurrentDictionary<Guid, ControlResponse>();
        }

        public bool TryRemove(Guid id, out ControlResponse response)
        {
            return items.TryRemove(id, out response);
        }

        public void Add(Guid id, ControlResponse response)
        {
            items.TryAdd(id, response);
        }

        public IEnumerator<KeyValuePair<Guid, ControlResponse>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
