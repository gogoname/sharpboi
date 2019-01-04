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
            return new BitArray(value).Get(bit);
        }
        public void SetCertainBit(int bit, bool value)
        {
            BitArray tmp = new BitArray(this.value);
            tmp.Set(bit, value);
            byte[] tmpArr = new byte[1];
            tmp.CopyTo(tmpArr, 0);
            this.value = tmpArr[0];
        }
        public void Sync()
        {
            value = Convert.ToByte(high << 4 | low);
        }
    }
}
