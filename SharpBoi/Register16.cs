using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoi
{
    class Register16
    {
        private Register8 low;
        private Register8 high;
        public ushort value;
        public Register16 (Register8 low, Register8 high)
        {
            this.low = low;
            this.high = high;
            value = BitConverter.ToUInt16(new byte[2] { low.value, high.value }, 0);
        }
        public Register16(ushort value)
        {
            low = new Register8(0);
            high = new Register8(0);
            low.value = Convert.ToByte(value & 0x00FF);
            high.value = Convert.ToByte(value >> 8);
            this.value = value;
        }
        public Register8 GetHigh()
        {
            return high;
        }
        public void SetHigh(byte high)
        {
            this.high.value = high;
            this.high.SetHigh(Convert.ToByte((high >> 4) & 0x0F));
            this.high.SetLow(Convert.ToByte(high & 0x0F));
            value = BitConverter.ToUInt16(new byte[2] { low.value, this.high.value }, 0);
        }
        public Register8 GetLow()
        {
            return low;
        }
        public void SetLow(byte low)
        {
            this.low.value = low;
            this.low.SetHigh(Convert.ToByte((low >> 4) & 0x0F));
            this.low.SetLow(Convert.ToByte(low & 0x0F));
            value = BitConverter.ToUInt16(new byte[2] { this.low.value, high.value }, 0);
        } 
        public void Sync()
        {
            value = BitConverter.ToUInt16(new byte[2] { low.value, high.value }, 0);
        }
    }
}
