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
        public byte high;
        public byte low;
        public Register8(byte value)
        {
            this.value = value;
            high = Convert.ToByte((value >> 4) & 0x0F);
            low = Convert.ToByte(value & 0x0F);
        }
        public bool GetCertainBit(int bit)
        {
            return (value & (1 << bit - 1)) != 0;
        }
        public void SetCertainBit(int bit, bool value)
        {
            if (value)
                this.value |= Convert.ToByte(1 << bit);
            else
                this.value ^= Convert.ToByte(1 << bit);
        }
        public void Sync()
        {
            value = Convert.ToByte(high << 4 | low);
        }
    }
}
