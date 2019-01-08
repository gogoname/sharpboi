using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoi
{
    class Register16
    {
        public Register8 low;
        public Register8 high;
        public ushort value;
        public Register16 (Register8 low, Register8 high)
        {
            this.low = low;
            this.high = high;
            value = BitConverter.ToUInt16(new byte[2] { high.value, low.value }, 0);
        }
        public Register16(ushort value)
        {
            low = new Register8(0);
            high = new Register8(0);
            low.value = Convert.ToByte(value & 0x00FF);
            high.value = Convert.ToByte(value >> 8);
            this.value = value;
        }
        public void Sync()
        {
            value = BitConverter.ToUInt16(new byte[2] { high.value, low.value }, 0);
            high.Sync();
            low.Sync();
        }
    }
}
