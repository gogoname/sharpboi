using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoi
{
    class RAM
    {
        byte[] ram = new byte[65535];
        public bool Write(byte data, int location)
        {
            try
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
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Write(byte[] data, int initLocation)
        {
            try
            {
                for (int i=0; i < data.Length; i++)
                {
                    Write(data[i], initLocation + i);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
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
