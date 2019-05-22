using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SharpBoi
{
    class Register8
    {
        public byte value;
        private byte high;
        private byte low;
        public Register8(byte value)
        {
            this.value = value;
            high = Convert.ToByte((value >> 4) & 0x0F);
            low = Convert.ToByte(value & 0x0F);
        }
        public byte GetHigh()
        {
            return high;
        }
        public void SetHigh(byte high)
        {
            this.high = high;
            value = Convert.ToByte(high << 4 | low);
        }
        public byte GetLow()
        {
            return low;
        }
        public void SetLow(byte low)
        {
            this.low = low;
        }
        public bool GetCertainBit(int bit)
        {
            return (value & (1 << bit)) != 0;
        }
        public void SetCertainBit(int bit, bool value)
        {
            bit = 7 - bit;
            char[] tmp = Convert.ToString(this.value, 2).PadLeft(8, '0').ToCharArray();
            if (value)
                tmp[bit] = '1';
            else
                tmp[bit] = '0';
            string str = new string(tmp);
            this.value = Convert.ToByte(str, 2);
        }
    }
}
