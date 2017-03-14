using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WagoService.Wago
{
    public struct WagoAddress
    {
        public int Word { get; set; }
        public int Bit { get; set; }

        public byte[] GetBuffer(bool state)
        {
            byte[] result = new byte[65];
            result[0] = 0x7;
            result[1] = 0x1;
            result[2] = 0x0;
            result[3] = (byte)Word;                         // Word offset
            result[4] = 0x0;
            result[5] = (byte)Bit;                          // Bit offset
            result[6] = Convert.ToByte(state ? 0x1 : 0x0);  // Turn On/Off
            return result;
        }
    }
}
