using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WagoService
{
    public class ControlActionEventArgs : EventArgs
    {
        public Guid ID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string MessageType { get; set; }
    }
}
