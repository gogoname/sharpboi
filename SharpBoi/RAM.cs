using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoi
{
    class RAM
    {
        private byte[] ram = new byte[65536];
        public void Write(byte data, int location)
        {
            if (location > 0xE000 && location < 0xFE00) 
            {
                ram[location] = data;
                ram[location - 0x2000] = data;
            }
            else if (location > 0xC000 && location < 0xDE00)
            {
                ram[location] = data;
                ram[location + 0x2000] = data;
            }
            /*The conditions above are for echo memory:
                [E000] - [FE00]
                ===============
                [C000] - [DE00]
                Changes in these ranges are copied to their counterparts
                */
            else
                ram[location] = data;
        }
        public void Write(byte[] data, int initLocation)
        {

            for (int i=initLocation; i < data.Length + initLocation; i++)
            {
                if (i > 0xE000 && i < 0xFE00)
                {
                    ram[i] = data[i - initLocation];
                    ram[i - 0x2000] = data[i - initLocation];
                }
                else if (i > 0xC000 && i < 0xDE00)
                {
                    ram[i] = data[i - initLocation];
                    ram[i + 0x2000] = data[i - initLocation];
                }
                else
                    Write(data[i], initLocation + i);
            }
        }
        public byte Read(int location)
        {
            return ram[location];
        }
        public byte[] Read(int location, int length)
        {
            byte[] toReturn = new byte[length];
            for (int i = 0; i < length; i++)
            {
                toReturn[i] = ram[location + i];
            }
            return toReturn;
        }
        public void Edit(int location, int change)
        {
            if (change >= 0)
                ram[location] += Convert.ToByte(change);
            else
                ram[location] -= Convert.ToByte(change * -1);
        }
    }
}
