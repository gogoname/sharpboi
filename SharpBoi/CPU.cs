using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Data;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpBoi
{
    class CPU
    {
        RAM ram;
        Register8 A;
        Register8 F;
        Register8 B;
        Register8 C;
        Register8 D;
        Register8 E;
        Register8 H;
        Register8 L;
        Register16 AF;
        Register16 BC;
        Register16 DE;
        Register16 HL;
        Register16 PC;
        Register16 SP;
        Stack<byte> st = new Stack<byte>();
        bool isStopped;
        bool isHalted;
        bool isIntsEnabled;
        bool isDI;
        bool isEI;
        sbyte addr;
        byte tmp;
        bool old;
        char[] temp;
        public CPU(RAM ram)
        {
            this.ram = ram;
            A = new Register8(0);
            F = new Register8(0); // [7] - Zero ||| [6] - N(Subtract) ||| [5] - Half Carry (Lower nibble) ||| [4] - Carry flag
            B = new Register8(0);
            C = new Register8(0);
            D = new Register8(0);
            E = new Register8(0);
            H = new Register8(0);
            L = new Register8(0);
            AF = new Register16(A, F);
            BC = new Register16(B, C);
            DE = new Register16(D, E);
            HL = new Register16(H, L);
            PC = new Register16(0x100);
            SP = new Register16(0xFFFE);
            isStopped = false;
            isHalted = false;
            isIntsEnabled = false;
            isDI = false;
            isEI = false;
            old = true;
            addr = 0;
            tmp = 0;
        }

        public void Start(byte[] opcodes)
        {
            ram.Write(opcodes, 0);
            while (true)
            {
                Console.WriteLine(PC.value.ToString("x"));
                if (PC.value == 0x2817)
                {
                    using (StreamWriter sw = new StreamWriter("tile.txt"))
                    {
                        byte[] tiles = ram.Read(PC.value, 16);
                        foreach (byte tile in tiles) {
                            sw.Write(tile);
                        }
                    }
                }
                if (isDI)
                {
                    isIntsEnabled = false;
                    isDI = false;
                }

                if (isEI)
                {
                    isIntsEnabled = true;
                    isEI = false;
                }
                if (!(isHalted | isStopped))
                {
                    switch (ram.Read(PC.value))
                    {
                        case 0x00: //NOP
                            break;
                        case 0x01: //LD BC, d16
                            BC.value = BitConverter.ToUInt16(
                                new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                            PC.value += 2;
                            break;
                        case 0x02: //LD [BC], A
                            ram.Write(AF.high.value, BC.value + 0xFF00);
                            break;
                        case 0x03: //INC BC
                            BC.value++;
                            break;
                        case 0x04: //INC B
                            BC.high.value++;
                            BC.Sync();
                            break;
                        case 0x05: //DEC B
                            BC.high.value--;
                            BC.Sync();
                            break;
                        case 0x06: //LD B, d8
                            BC.high.value = ram.Read(PC.value + 1);
                            PC.value++;
                            BC.Sync();
                            break;
                        case 0x07: //RLCA
                            RLCH(ref AF);
                            break;
                        case 0x08: //LD [a16], SP
                            SP.value = ram.Read(
                                BitConverter.ToUInt16(new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0));
                            PC.value += 2;
                            break;
                        case 0x09: //ADD HL, BC
                            HL.value += BC.value;
                            break;
                        case 0x0A: //LD A, [BC]
                            AF.high.value = ram.Read(BC.value + 0xFF00);
                            AF.Sync();
                            break;
                        case 0x0B: //DEC BC
                            BC.value--;
                            break;
                        case 0x0C: //INC C
                            BC.low.value++;
                            break;
                        case 0x0D: //DEC C
                            BC.low.value--;
                            break;
                        case 0x0E: //LD C, d8
                            BC.low.value = ram.Read(PC.value + 1);
                            PC.value++;
                            break;
                        case 0x0F: //RRCA
                            RRCH(ref AF);
                            break;
                        case 0x10: //STOP
                            isStopped = true;
                            break;
                        case 0x11: //LD DE, d16
                            DE.value = BitConverter.ToUInt16(
                                new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                            PC.value += 2;
                            break;
                        case 0x12: //LD [DE], A
                            ram.Write(AF.high.value, DE.value + 0xFF00);
                            break;
                        case 0x13: //INC DE
                            DE.value++;
                            break;
                        case 0x14: //INC D
                            DE.high.value++;
                            DE.Sync();
                            break;
                        case 0x15: //DEC D
                            DE.high.value--;
                            DE.Sync();
                            break;
                        case 0x16: //LD D, d8
                            DE.high.value = ram.Read(PC.value + 1);
                            PC.value++;
                            DE.Sync();
                            break;
                        case 0x17: //RLA
                            RLH(ref AF);
                            break;
                        case 0x18: //JR r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            PC.value++;
                            if (addr > 0)
                                PC.value += Convert.ToUInt16(addr - 1);
                            else
                                PC.value -= Convert.ToUInt16((addr - 1) * -1);
                            break;
                        case 0x19: //ADD HL, DE
                            HL.value += DE.value;
                            break;
                        case 0x1A: //LD A, [DE]
                            AF.high.value = ram.Read(DE.value + 0xFF00);
                            AF.Sync();
                            break;
                        case 0x1B: //DEC DE
                            DE.value--;
                            break;
                        case 0x1C: //INC E
                            DE.low.value++;
                            DE.Sync();
                            break;
                        case 0x1D: //DEC E
                            DE.low.value--;
                            DE.Sync();
                            break;
                        case 0x1E: //LD E, d8
                            DE.low.value = ram.Read(PC.value + 1);
                            PC.value++;
                            DE.Sync();
                            break;
                        case 0x1F: //RRA
                            RRH(ref AF);
                            break;
                        case 0x20: //JR NZ, r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            if (!AF.low.GetCertainBit(7))
                            {
                                if (addr > 0)
                                    PC.value += Convert.ToUInt16(addr - 1);
                                else
                                    PC.value -= Convert.ToUInt16((addr - 1) * -1);
                            }
                            PC.value += 2;
                            break;
                        case 0x21: //LD HL, d16
                            HL.value = BitConverter.ToUInt16(
                                new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                            PC.value += 2;
                            break;
                        case 0x22: //LD [HL+], A
                            ram.Write(AF.high.value, HL.value);
                            HL.value++;
                            break;
                        case 0x23: //INC HL
                            HL.value++;
                            break;
                        case 0x24: //INC H
                            HL.high.value++;
                            HL.Sync();
                            break;
                        case 0x25: //DEC H
                            HL.high.value--;
                            HL.Sync();
                            break;
                        case 0x26: //LD H, d8
                            HL.high.value = ram.Read(PC.value + 1);
                            PC.value++;
                            HL.Sync();
                            break;
                        case 0x27: //DAA (By FAR the most retarded opcode here)
                            DAA(4);
                            break;
                        case 0x28: //JR Z, r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            PC.value++;
                            if (AF.low.GetCertainBit(7))
                            {
                                if (addr > 0)
                                    PC.value += Convert.ToUInt16(addr - 1);
                                else
                                    PC.value -= Convert.ToUInt16((addr - 1) * -1);
                            }

                            break;
                        case 0x29: //ADD HL, HL
                            HL.value *= 2;
                            break;
                        case 0x2A: //LD A, [HL+]
                            AF.high.value = ram.Read(HL.value);
                            HL.value++;
                            AF.Sync();
                            break;
                        case 0x2B: //DEC HL
                            HL.value--;
                            break;
                        case 0x2C: //INC L
                            HL.low.value++;
                            HL.Sync();
                            break;
                        case 0x2D: //DEC L
                            HL.low.value--;
                            HL.Sync();
                            break;
                        case 0x2E: //L, d8
                            HL.low.value = ram.Read(PC.value + 1);
                            PC.value++;
                            HL.Sync();
                            break;
                        case 0x2F: //CPL
                            AF.high.value = Convert.ToByte(~AF.high.value);
                            AF.Sync();
                            break;
                        case 0x30: //JR NC, r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            PC.value++;
                            if (!AF.low.GetCertainBit(4))
                            {
                                if (addr > 0)
                                    PC.value += Convert.ToUInt16(addr - 1);
                                else
                                    PC.value -= Convert.ToUInt16((addr - 1) * -1);
                            }

                            break;
                        case 0x31: //LD SP, d16
                            SP.value = ram.Read(PC.value + 1);
                            PC.value += 2;
                            break;
                        case 0x32: //LD [HL-], A
                            ram.Write(AF.high.value, HL.value);
                            HL.value--;
                            break;
                        case 0x33: //INC SP
                            SP.value++;
                            break;
                        case 0x34: //INC [HL]
                            ram.Edit(HL.value, 1);
                            break;
                        case 0x35: //DEC [HL]
                            ram.Edit(HL.value, -1);
                            break;
                        case 0x36: //LD [HL], d8
                            ram.Write(ram.Read(PC.value + 1), HL.value);
                            PC.value++;
                            break;
                        case 0x37: //SCF
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, true);
                            break;
                        case 0x38: //JR C, r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            PC.value++;
                            if (AF.low.GetCertainBit(4))
                            {
                                if (addr > 0)
                                    PC.value += Convert.ToUInt16(addr - 1);
                                else
                                    PC.value -= Convert.ToUInt16((addr - 1) * -1);
                            }

                            break;
                        case 0x39: //ADD HL, SP
                            HL.value += SP.value;
                            break;
                        case 0x3A: //LD A, [HL-]
                            AF.high.value = ram.Read(HL.value);
                            HL.value--;
                            AF.Sync();
                            break;
                        case 0x3B: //DEC SP
                            SP.value--;
                            break;
                        case 0x3C: //INC A
                            AF.high.value++;
                            AF.Sync();
                            break;
                        case 0x3D: //DEC A
                            AF.high.value--;
                            AF.Sync();
                            break;
                        case 0x3E: //LD A, d8
                            AF.high.value = ram.Read(PC.value + 1);
                            PC.value++;
                            AF.Sync();
                            break;
                        case 0x3F: //CCF
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, !AF.low.GetCertainBit(4));
                            break;
                        case 0x41: //LD B, C
                            BC.high.value = BC.low.value;
                            BC.Sync();
                            break;
                        case 0x42: //LD B, D
                            BC.high.value = DE.high.value;
                            BC.Sync();
                            break;
                        case 0x43: //LD B, E
                            BC.high.value = DE.low.value;
                            BC.Sync();
                            break;
                        case 0x44: //LD B, H
                            BC.high.value = HL.high.value;
                            BC.Sync();
                            break;
                        case 0x45: //LD B, L
                            BC.high.value = HL.low.value;
                            BC.Sync();
                            break;
                        case 0x46: //LD B, [HL]
                            BC.high.value = ram.Read(HL.value);
                            BC.Sync();
                            break;
                        case 0x47: //LD B, A
                            BC.high.value = AF.high.value;
                            BC.Sync();
                            break;
                        case 0x48: //LD C, B
                            BC.low.value = BC.high.value;
                            BC.Sync();
                            break;
                        case 0x4A: //LD C, D
                            BC.low.value = DE.high.value;
                            BC.Sync();
                            break;
                        case 0x4B: //LD C, E
                            BC.low.value = DE.low.value;
                            BC.Sync();
                            break;
                        case 0x4C: //LD C, H
                            BC.low.value = HL.high.value;
                            BC.Sync();
                            break;
                        case 0x4D: //LD C, L
                            BC.low.value = HL.low.value;
                            BC.Sync();
                            break;
                        case 0x4E: //LD C, [HL]
                            BC.low.value = ram.Read(HL.value);
                            BC.Sync();
                            break;
                        case 0x4F: //LD C, A
                            BC.low.value = AF.high.value;
                            BC.Sync();
                            break;
                        case 0x50: //LD D, B
                            DE.high.value = BC.high.value;
                            DE.Sync();
                            break;
                        case 0x51: //LD D, C
                            DE.high.value = BC.low.value;
                            DE.Sync();
                            break;
                        case 0x53: //LD D, E
                            DE.high.value = DE.low.value;
                            DE.Sync();
                            break;
                        case 0x54: //LD D, H
                            DE.high.value = HL.high.value;
                            DE.Sync();
                            break;
                        case 0x55: //LD D, L
                            DE.high.value = HL.low.value;
                            DE.Sync();
                            break;
                        case 0x56: //LD D, [HL]
                            DE.high.value = ram.Read(HL.value);
                            DE.Sync();
                            break;
                        case 0x57: //LD D, A
                            DE.high.value = AF.high.value;
                            DE.Sync();
                            break;
                        case 0x58: //LD E, B
                            DE.low.value = BC.high.value;
                            DE.Sync();
                            break;
                        case 0x59: //LD E, C
                            DE.low.value = BC.low.value;
                            DE.Sync();
                            break;
                        case 0x5A: //LD E, D
                            DE.low.value = DE.high.value;
                            DE.Sync();
                            break;
                        case 0x5C: //LD E, H
                            DE.low.value = HL.high.value;
                            DE.Sync();
                            break;
                        case 0x5D: //LD E, L
                            DE.low.value = HL.low.value;
                            DE.Sync();
                            break;
                        case 0x5E: //LD E [HL]
                            DE.low.value = ram.Read(HL.value);
                            DE.Sync();
                            break;
                        case 0x5F: //LD E, A
                            DE.low.value = AF.high.value;
                            DE.Sync();
                            break;
                        case 0x60: //LD H, B
                            HL.high.value = BC.high.value;
                            HL.Sync();
                            break;
                        case 0x61: //LD H, C
                            HL.high.value = BC.low.value;
                            HL.Sync();
                            break;
                        case 0x62: //LD H, D
                            HL.high.value = DE.high.value;
                            HL.Sync();
                            break;
                        case 0x63: //LD H, E
                            HL.high.value = DE.low.value;
                            HL.Sync();
                            break;
                        case 0x65: //LD H, L
                            HL.high.value = HL.low.value;
                            HL.Sync();
                            break;
                        case 0x66: //LD H, [HL]
                            HL.high.value = ram.Read(HL.value);
                            HL.Sync();
                            break;
                        case 0x67: //LD H, A
                            HL.high.value = AF.high.value;
                            HL.Sync();
                            break;
                        case 0x68: //LD L, B
                            HL.low.value = BC.high.value;
                            HL.Sync();
                            break;
                        case 0x69: //LD L, C
                            HL.low.value = BC.low.value;
                            HL.Sync();
                            break;
                        case 0x6A: //LD L, D
                            HL.low.value = DE.high.value;
                            HL.Sync();
                            break;
                        case 0x6B: //LD L, E
                            HL.low.value = DE.low.value;
                            HL.Sync();
                            break;
                        case 0x6C: //LD L, H
                            HL.low.value = HL.high.value;
                            HL.Sync();
                            break;
                        case 0x6E: //LD L, [HL]
                            HL.low.value = ram.Read(HL.value);
                            HL.Sync();
                            break;
                        case 0x6F: //LD L, A
                            HL.low.value = AF.high.value;
                            HL.Sync();
                            break;
                        case 0x70: //LD [HL], B
                            ram.Write(BC.high.value, HL.value);
                            break;
                        case 0x71: //LD [HL], C
                            ram.Write(BC.low.value, HL.value);
                            break;
                        case 0x72: //LD [HL], D
                            ram.Write(DE.high.value, HL.value);
                            break;
                        case 0x73: //LD [HL], E
                            ram.Write(DE.low.value, HL.value);
                            break;
                        case 0x74: //LD [HL], H
                            ram.Write(HL.high.value, HL.value);
                            break;
                        case 0x75: //LD [HL], L
                            ram.Write(HL.low.value, HL.value);
                            break;
                        case 0x76: //HALT
                            isHalted = true;
                            break;
                        case 0x77: //LD [HL], A
                            ram.Write(AF.high.value, HL.value);
                            break;
                        case 0x78: //LD A, B
                            AF.high.value = BC.high.value;
                            AF.Sync();
                            break;
                        case 0x79: //LD A, C
                            AF.high.value = BC.low.value;
                            AF.Sync();
                            break;
                        case 0x7A: //LD A, D
                            AF.high.value = DE.high.value;
                            AF.Sync();
                            break;
                        case 0x7B: //LD A, E
                            AF.high.value = DE.low.value;
                            AF.Sync();
                            break;
                        case 0x7C: //LD A, H
                            AF.high.value = HL.high.value;
                            AF.Sync();
                            break;
                        case 0x7D: //LD A, L
                            AF.high.value = HL.low.value;
                            AF.Sync();
                            break;
                        case 0x7E: //LD A, [HL]
                            AF.high.value = ram.Read(HL.value);
                            AF.Sync();
                            break;
                        case 0x80: //ADD A, B
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += BC.high.value;
                            AF.Sync();
                            break;
                        case 0x81: //ADD A, C
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += BC.low.value;
                            AF.Sync();
                            break;
                        case 0x82: //ADD A, D
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += DE.high.value;
                            AF.Sync();
                            break;
                        case 0x83: //ADD A, E
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += DE.low.value;
                            AF.Sync();
                            break;
                        case 0x84: //ADD A, H
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += HL.high.value;
                            AF.Sync();
                            break;
                        case 0x85: //ADD A, L
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += HL.low.value;
                            AF.Sync();
                            break;
                        case 0x86: //ADD A, [HL]
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += ram.Read(HL.value);
                            AF.Sync();
                            break;
                        case 0x87: //ADD A, A
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value *= 2;
                            AF.Sync();
                            break;
                        case 0x88: //ADC A, B
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(BC.high.value + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x89: //ADC A, C
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(BC.low.value + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x8A: //ADC A, D
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(DE.high.value + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x8B: //ADC A, E
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(DE.low.value + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x8C: //ADC A, H
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(HL.high.value + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x8D: //ADC A, L
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(HL.low.value + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x8E: //ADC A, [HL]
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value +=
                                Convert.ToByte(ram.Read(HL.value) + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.Sync();
                            break;
                        case 0x90: //SUB B
                            AF.low.SetCertainBit(5, AF.high.low > BC.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > BC.high.high);
                            AF.high.value -= BC.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x91: //SUB C
                            AF.low.SetCertainBit(5, AF.high.low > BC.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > BC.low.high);
                            AF.high.value -= BC.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x92: //SUB D
                            AF.low.SetCertainBit(5, AF.high.low > DE.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > DE.high.high);
                            AF.high.value -= DE.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x93: //SUB E
                            AF.low.SetCertainBit(5, AF.high.low > DE.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > DE.low.high);
                            AF.high.value -= DE.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x94: //SUB H
                            AF.low.SetCertainBit(5, AF.high.low > HL.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > HL.high.high);
                            AF.high.value -= HL.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x95: //SUB L
                            AF.low.SetCertainBit(5, AF.high.low > HL.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > HL.low.high);
                            AF.high.value -= HL.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x96: //SUB [HL]
                            AF.low.SetCertainBit(5, AF.high.low > (ram.Read(HL.value) & 0x0F));
                            AF.low.SetCertainBit(4, AF.high.high > (ram.Read(HL.value) >> 4 & 0x0F));
                            AF.high.value -= ram.Read(HL.value);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x97: //SUB A
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.high.value = 0;
                            AF.low.SetCertainBit(7, true);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0x98: //SBC A, B
                            tmp = BC.high.value;
                            BC.high.value += Convert.ToByte(AF.low.GetCertainBit(4));
                            AF.low.SetCertainBit(5, AF.high.low > BC.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > BC.high.high);
                            AF.high.value -= BC.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            BC.high.value = tmp;
                            BC.Sync();
                            break;
                        case 0x99: //SBC A, C
                            tmp = BC.low.value;
                            BC.low.value += Convert.ToByte(AF.low.GetCertainBit(4));
                            AF.low.SetCertainBit(5, AF.high.low > BC.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > BC.low.high);
                            AF.high.value -= BC.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            BC.low.value = tmp;
                            BC.Sync();
                            break;
                        case 0x9A: //SBC A, D
                            tmp = DE.high.value;
                            DE.high.value += Convert.ToByte(AF.low.GetCertainBit(4));
                            AF.low.SetCertainBit(5, AF.high.low > DE.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > DE.high.high);
                            AF.high.value -= DE.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            DE.high.value = tmp;
                            DE.Sync();
                            break;
                        case 0x9B: //SBC A, E
                            tmp = DE.low.value;
                            DE.low.value += Convert.ToByte(AF.low.GetCertainBit(4));
                            AF.low.SetCertainBit(5, AF.high.low > DE.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > DE.low.high);
                            AF.high.value -= DE.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            DE.low.value = tmp;
                            DE.Sync();
                            break;
                        case 0x9C: //SBC A, H
                            tmp = HL.high.value;
                            HL.high.value += Convert.ToByte(AF.low.GetCertainBit(4));
                            AF.low.SetCertainBit(5, AF.high.low > HL.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > HL.high.high);
                            AF.high.value -= HL.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            HL.high.value = tmp;
                            HL.Sync();
                            break;
                        case 0x9D: //SBC A, L
                            tmp = HL.low.value;
                            HL.low.value += Convert.ToByte(AF.low.GetCertainBit(4));
                            AF.low.SetCertainBit(5, AF.high.low > HL.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > HL.low.high);
                            AF.high.value -= HL.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            HL.low.value = tmp;
                            HL.Sync();
                            break;
                        case 0x9E: //SBC A, [HL]
                            tmp = ram.Read(HL.value);
                            ram.Edit(HL.value, Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(5, AF.high.low > (ram.Read(HL.value) & 0x0F));
                            AF.low.SetCertainBit(4, AF.high.high > (ram.Read(HL.value) >> 4 & 0x0F));
                            AF.high.value -= ram.Read(HL.value);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            ram.Write(tmp, HL.value);
                            break;
                        case 0x9F: //SBC A, A                   PROBLEMATIC!!!
                            AF.high.value = Convert.ToByte(0 - Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.Sync();
                            break;
                        case 0xA0: //AND B
                            AF.high.value &= BC.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.Sync();
                            AF.low.SetCertainBit(4, false);
                            break;
                        case 0xA1: //AND C
                            AF.high.value &= BC.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA2: //AND D
                            AF.high.value &= DE.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA3: //AND E
                            AF.high.value &= DE.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA4: //AND H
                            AF.high.value &= HL.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA5: //AND L
                            AF.high.value &= HL.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA6: //AND [HL]
                            AF.high.value &= ram.Read(HL.value);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA7: //AND A
                            AF.high.value &= AF.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA8: //XOR B
                            AF.high.value ^= BC.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xA9: //XOR C
                            AF.high.value ^= BC.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xAA: //XOR D
                            AF.high.value ^= DE.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xAB: //XOR E
                            AF.high.value ^= DE.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xAC: //XOR H
                            AF.high.value ^= HL.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xAD: //XOR L
                            AF.high.value ^= HL.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xAE: //XOR [HL]
                            AF.high.value ^= ram.Read(HL.value);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xAF: //XOR A
                            AF.high.value ^= AF.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB0: //OR B
                            AF.high.value |= BC.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB1: //OR C
                            AF.high.value |= BC.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB2: //OR D
                            AF.high.value |= DE.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB3: //OR E
                            AF.high.value |= DE.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB4: //OR H
                            AF.high.value |= HL.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB5: //OR L
                            AF.high.value |= HL.low.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB6: //OR [HL]
                            AF.high.value |= ram.Read(HL.value);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB7: //OR A
                            AF.high.value |= AF.high.value;
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            break;
                        case 0xB8: //CP B
                            AF.low.SetCertainBit(5, AF.high.low > BC.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > BC.high.high);
                            AF.low.SetCertainBit(7, AF.high.value == BC.high.value);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xB9: //CP C
                            AF.low.SetCertainBit(5, AF.high.low > BC.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > BC.low.high);
                            AF.low.SetCertainBit(7, AF.high.value == BC.low.value);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xBA: //CP D
                            AF.low.SetCertainBit(5, AF.high.low > DE.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > DE.high.high);
                            AF.low.SetCertainBit(7, AF.high.value == DE.high.value);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xBB: //CP E
                            AF.low.SetCertainBit(5, AF.high.low > DE.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > DE.low.high);
                            AF.low.SetCertainBit(7, AF.high.value == DE.low.value);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xBC: //CP H
                            AF.low.SetCertainBit(5, AF.high.low > HL.high.low);
                            AF.low.SetCertainBit(4, AF.high.high > HL.high.high);
                            AF.low.SetCertainBit(7, AF.high.value == HL.high.value);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xBD: //CP L
                            AF.low.SetCertainBit(5, AF.high.low > HL.low.low);
                            AF.low.SetCertainBit(4, AF.high.high > HL.low.high);
                            AF.low.SetCertainBit(7, AF.high.value == HL.low.value);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xBE: //CP [HL]
                            AF.low.SetCertainBit(5, AF.high.low > ram.Read(HL.value));
                            AF.low.SetCertainBit(4, AF.high.high > ram.Read(HL.value));
                            AF.low.SetCertainBit(7, AF.high.value == ram.Read(HL.value));
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xBF: //CP A
                            AF.low.SetCertainBit(5, true);
                            AF.low.SetCertainBit(4, true);
                            AF.low.SetCertainBit(7, true);
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            break;
                        case 0xC0: //RET NZ
                            if (!AF.low.GetCertainBit(7))
                            {
                                PC.value = ram.Read(Convert.ToUInt16(new byte[] {st.Pop(), st.Pop()}));
                                SP.value += 2;
                            }
                            break;
                        case 0xC1: //POP BC
                            BC.value = Convert.ToUInt16(new byte[] {st.Pop(), st.Pop()});
                            SP.value += 2;
                            break;
                        case 0xC2: //JP NZ, a16
                            if (!AF.low.GetCertainBit(7))
                            {
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                                PC.value--;
                            }
                            break;
                        case 0xC3: //JP a16
                            PC.value = BitConverter.ToUInt16(
                                new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                            PC.value--;
                            break;
                        case 0xC4: //CALL NZ, a16
                            if (!AF.low.GetCertainBit(7))
                            {
                                st.Push(Convert.ToByte((PC.value + 1) >> 8));
                                st.Push(Convert.ToByte((PC.value + 1) & 0x00FF));
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                                PC.value--;
                            }
                            break;
                        case 0xC5: //PUSH BC
                            st.Push(BC.high.value);
                            st.Push(BC.low.value);
                            SP.value -= 2;
                            break;
                        case 0xC6: //ADD A, d8
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            AF.high.value += ram.Read(PC.value + 1);
                            PC.value++;
                            AF.Sync();
                            break;
                        case 0xC7: //RST 00H
                            PC.value = ushort.MaxValue;
                            break;
                        case 0xC8: //RET Z
                            if (AF.low.GetCertainBit(7))
                            {
                                PC.value = ram.Read(Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() }));
                                SP.value += 2;
                            }
                            break;
                        case 0xC9: //RET
                            PC.value = ram.Read(Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() }));
                            SP.value += 2;
                            break;
                        case 0xCA: //JP Z, a16
                            if (AF.low.GetCertainBit(7))
                            {
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            }
                            break;
                        case 0xCB: //THE MIGHTY CB INSTRUCTION (Opens up a bunch of more instructions)
                            CB();
                            break;
                        case 0xCC: //CALL Z, a16
                            if (AF.low.GetCertainBit(7))
                            {
                                st.Push(Convert.ToByte((PC.value + 1) >> 8));
                                st.Push(Convert.ToByte((PC.value + 1) & 0x00FF));
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            }
                            break;
                        case 0xCD: //CALL a16
                                st.Push(Convert.ToByte((PC.value + 1) >> 8));
                                st.Push(Convert.ToByte((PC.value + 1) & 0x00FF));
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            break;
                        case 0xCE: //ADC A, d8
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.high.value += Convert.ToByte(ram.Read(PC.value + 1) + Convert.ToByte(AF.low.GetCertainBit(4)));
                            AF.low.SetCertainBit(4, AF.high.value == 255);
                            PC.value++;
                            AF.Sync();
                            break;
                        case 0xCF: //RST 08H
                            PC.value = 0x8 - 1;
                            break;
                        case 0xD0: //RET NC
                            if (!AF.low.GetCertainBit(4))
                            {
                                PC.value = ram.Read(Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() }));
                                SP.value += 2;
                            }
                            break;
                        case 0xD1: //POP DE
                            DE.value = Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() });
                            SP.value += 2;
                            break;
                        case 0xD2: //JP NC, a16
                            if (!AF.low.GetCertainBit(4))
                            {
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            }
                            break;
                        case 0xD4: //CALL NC, a16
                            if (!AF.low.GetCertainBit(4))
                            {
                                st.Push(Convert.ToByte((PC.value + 1) >> 8));
                                st.Push(Convert.ToByte((PC.value + 1) & 0x00FF));
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            }
                            break;
                        case 0xD5: //PUSH DE
                            st.Push(DE.high.value);
                            st.Push(DE.low.value);
                            SP.value -= 2;
                            break;
                        case 0xD6: //SUB d8
                            AF.low.SetCertainBit(5, AF.high.low > Convert.ToByte(ram.Read(PC.value + 1) & 0x0F));
                            AF.low.SetCertainBit(4, AF.high.high > Convert.ToByte((ram.Read(PC.value + 1) >> 4) & 0x0F));
                            AF.high.value -= ram.Read(PC.value + 1);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, true);
                            PC.value++;
                            AF.Sync();
                            break;
                        case 0xD7: //RST 10H
                            PC.value = 0x10 - 1;
                            break;
                        case 0xD8: //RET C
                            if (AF.low.GetCertainBit(4))
                            {
                                PC.value = ram.Read(Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() }));
                                SP.value += 2;
                            }
                            break;
                        case 0xD9: //RETI
                            PC.value = ram.Read(Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() }));
                            SP.value += 2;
                            isIntsEnabled = true;
                            break;
                        case 0xDA: //JP C, a16
                            if (AF.low.GetCertainBit(4))
                            {
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            }
                            break;
                        case 0xDC: //CALL C, a16
                            if (AF.low.GetCertainBit(4))
                            {
                                st.Push(Convert.ToByte((PC.value + 1) >> 8));
                                st.Push(Convert.ToByte((PC.value + 1) & 0x00FF));
                                PC.value = BitConverter.ToUInt16(
                                    new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
                                PC.value--;
                            }
                            break;
                        case 0xDE: //SBC A, d8      DEAL WITH IT LATER
                            break;
                        case 0xDF: //RST 18H
                            PC.value = 0x18;
                            PC.value--;
                            break;
                        case 0xE0: //LDH [a8], A
                            ram.Write(AF.high.value, ram.Read(0xFF00 + PC.value + 1));
                            PC.value++;
                            break;
                        case 0xE1: //POP HL
                            HL.value = Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() });
                            SP.value += 2;
                            break;
                        case 0xE2: //LD [C], A
                            ram.Write(AF.high.value, ram.Read(0xFF00 + BC.low.value));
                            break;
                        case 0xE5: //PUSH HL
                            st.Push(HL.high.value);
                            st.Push(HL.low.value);
                            SP.value -= 2;
                            break;
                        case 0xE6: //AND d8
                            AF.high.value &= ram.Read(PC.value + 1);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, true);
                            AF.Sync();
                            AF.low.SetCertainBit(4, false);
                            PC.value++;
                            break;
                        case 0xE7: //RST 20H
                            PC.value = 0x20 - 1;
                            break;
                        case 0xE8: //ADD SP, r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            AF.low.SetCertainBit(7, SP.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, AF.high.low == 15);
                            AF.low.SetCertainBit(4, SP.value == ushort.MaxValue);
                            if (addr > 0)
                                SP.value += ram.Read(PC.value + 1);
                            else
                                SP.value -= ram.Read(PC.value + 1);
                            PC.value++;
                            AF.Sync();
                            break;
                        case 0xE9: //JP [HL]
                            PC.value = ram.Read(HL.value);
                            PC.value--;
                            break;
                        case 0xEA: //LD [a16], A
                            ram.Write(AF.high.value, ram.Read(PC.value + 1));
                            PC.value++;
                            break;
                        case 0xEE: //XOR d8
                            AF.high.value ^= ram.Read(PC.value + 1);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            PC.value++;
                            break;
                        case 0xEF: //RST 28H
                            PC.value = 0x28 - 1;
                            break;
                        case 0xF0: //LDH A, [a8]
                            AF.high.value = ram.Read(0xFF00 + ram.Read(PC.value + 1));
                            PC.value++;
                            AF.Sync();
                            break;
                        case 0xF1: //POP AF
                            AF.value = Convert.ToUInt16(new byte[] { st.Pop(), st.Pop() });
                            SP.value += 2;
                            break;
                        case 0xF2: //LD A, [C]
                            AF.high.value = ram.Read(0xFF00 + BC.low.value);
                            AF.Sync();
                            break;
                        case 0xF3: //DI
                            isDI = true;
                            isEI = false;
                            break;
                        case 0xF5: //PUSH AF
                            st.Push(AF.high.value);
                            st.Push(AF.low.value);
                            SP.value -= 2;
                            break;
                        case 0xF6: //OR d8
                            AF.high.value |= ram.Read(PC.value + 1);
                            AF.low.SetCertainBit(7, AF.high.value == 0);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, false);
                            AF.low.SetCertainBit(4, false);
                            AF.Sync();
                            PC.value++;
                            break;
                        case 0xF7: //RST 30H
                            PC.value = 0x30 - 1;
                            break;
                        case 0xF8: //LD HL, SP + r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            AF.low.SetCertainBit(7, false);
                            AF.low.SetCertainBit(6, false);
                            AF.low.SetCertainBit(5, HL.low.value == 255);
                            AF.low.SetCertainBit(4, HL.value == ushort.MaxValue);
                            if (addr > 0)
                                HL.value = Convert.ToUInt16(SP.value + addr);
                            else
                                HL.value = Convert.ToUInt16(SP.value - addr);
                            AF.Sync();
                            break;
                        case 0xF9: //LD SP, HL
                            SP.value = HL.value;
                            break;
                        case 0xFA: //LD A, [a16]
                            AF.high.value = ram.Read(BitConverter.ToUInt16(new byte[] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0));
                            PC.value += 2;
                            AF.Sync();
                            break;
                        case 0xFB: //EI
                            isEI = true;
                            isDI = false;
                            break;
                        case 0xFE: //CP d8
                            AF.low.SetCertainBit(5, AF.high.low > Convert.ToByte((ram.Read(PC.value + 1)) & 0x0F));
                            AF.low.SetCertainBit(4, AF.high.high > Convert.ToByte((ram.Read(PC.value + 1) >> 4) & 0x0F));
                            AF.low.SetCertainBit(7, AF.high.value == ram.Read(PC.value + 1));
                            AF.low.SetCertainBit(6, true);
                            AF.Sync();
                            PC.value++;
                            break;
                        case 0xFF: //RST 38H
                            PC.value = 0x38 - 1;
                            break;
                    }
                    PC.value++;
                }
            }
        }

        public void RLCH(ref Register16 reg) //Rotate Left through Carry - High
        {
            AF.low.SetCertainBit(4, reg.high.GetCertainBit(7)); // Set carry flag to the left most bit
            reg.high.value = Convert.ToByte((reg.high.value << 1) & 0xFF); // Shift left
            reg.high.SetCertainBit(7, AF.low.GetCertainBit(4)); // Set left most bit to the carry flag
            AF.low.SetCertainBit(7, reg.high.value != 0); // Activate zero flag if A is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false); // Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RLCL(ref Register16 reg) //Rotate Left through Carry - Low
        {
            AF.low.SetCertainBit(4, reg.low.GetCertainBit(7)); // Set carry flag to the left most bit
            reg.low.value = Convert.ToByte((reg.low.value << 1) & 0xFF); // Shift left
            reg.low.SetCertainBit(7, AF.low.GetCertainBit(4)); // Set left most bit to the carry flag
            AF.low.SetCertainBit(7, reg.low.value != 0); // Activate zero flag if A is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false); // Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RRCH(ref Register16 reg) //Rotate Right through Carry - High
        {
            AF.low.SetCertainBit(4, reg.high.GetCertainBit(0)); // Set carry flag to the right most bit
            reg.high.value = Convert.ToByte((reg.high.value >> 1) & 0xFF); // Shift right
            reg.high.SetCertainBit(0, AF.low.GetCertainBit(4)); // Set right most bit to the carry flag
            AF.low.SetCertainBit(7, reg.high.value != 0); // Activate zero flag if A is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false); // Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RRCL(ref Register16 reg) //Rotate Right through Carry - Low
        {
            AF.low.SetCertainBit(4, reg.low.GetCertainBit(0)); // Set carry flag to the right most bit
            reg.low.value = Convert.ToByte((reg.low.value >> 1) & 0xFF); // Shift right
            reg.low.SetCertainBit(0, AF.low.GetCertainBit(4)); // Set right most bit to the carry flag
            AF.low.SetCertainBit(7, reg.low.value != 0); // Activate zero flag if A is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false); // Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RLH(ref Register16 reg) //Rotate Left - High
        {
            old = reg.high.GetCertainBit(7); // Save old MSB
            reg.high.value = Convert.ToByte((reg.high.value << 1) & 0xFF); // Shift left
            AF.low.SetCertainBit(4, old); // Set old MSB to carry
            AF.low.SetCertainBit(7, (reg.high.value != 0) && !old); // Activate zero flag if the whole thing is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false);// Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RLL(ref Register16 reg) //Rotate Left - Low
        {
            old = reg.low.GetCertainBit(7); // Save old MSB
            reg.low.value = Convert.ToByte((reg.low.value << 1) & 0xFF); // Shift left
            AF.low.SetCertainBit(4, old); // Set old MSB to carry
            AF.low.SetCertainBit(7, (reg.low.value != 0) && !old); // Activate zero flag if the whole thing is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false);// Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RRH(ref Register16 reg) //Rotate Right - High
        {
            old = reg.high.GetCertainBit(0); // Save old LSB
            reg.high.value = Convert.ToByte((reg.high.value >> 1) & 0xFF); // Shift right
            AF.low.SetCertainBit(4, old); // Set old LSB to carry
            AF.low.SetCertainBit(7, (reg.high.value != 0) && !old); // Activate zero flag if the whole thing is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false);// Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void RRL(ref Register16 reg) //Rotate Right - Low
        {
            old = reg.low.GetCertainBit(0); // Save old LSB
            reg.low.value = Convert.ToByte((reg.low.value >> 1) & 0xFF); // Shift right
            AF.low.SetCertainBit(4, old); // Set old LSB to carry
            AF.low.SetCertainBit(7, (reg.low.value != 0) && !old); // Activate zero flag if the whole thing is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false);// Reset half carry flag
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SLAH (ref Register16 reg) //Shift Left through carry - High
        {
            old = reg.high.GetCertainBit(7);
            reg.high.value = Convert.ToByte((reg.high.value << 1) & 0xFF);
            AF.low.SetCertainBit(7, reg.high.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, old);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SLAL(ref Register16 reg) //Shift Left through carry - Low
        {
            old = reg.low.GetCertainBit(7);
            reg.low.value = Convert.ToByte((reg.low.value << 1) & 0xFF);
            AF.low.SetCertainBit(7, reg.low.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, old);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SRAH(ref Register16 reg) //Shift Right through carry - High
        {
            old = reg.high.GetCertainBit(0);
            reg.high.value = Convert.ToByte((reg.high.value >> 1) & 0xFF);
            AF.low.SetCertainBit(7, reg.high.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, old);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SRAL(ref Register16 reg) //Shift Right through carry - Low
        {
            old = reg.low.GetCertainBit(0);
            reg.low.value = Convert.ToByte((reg.low.value >> 1) & 0xFF);
            AF.low.SetCertainBit(7, reg.low.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, old);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SWAPH(ref Register16 reg) //SWAP the nibbles - High
        {
            byte oldLow = reg.high.low;
            reg.high.low = reg.high.high;
            reg.high.high = oldLow;
            AF.low.SetCertainBit(7, reg.high.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, false);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SWAPL(ref Register16 reg) //SWAP the nibbles - Low
        {
            byte oldLow = reg.low.low;
            reg.low.low = reg.low.high;
            reg.low.high = oldLow;
            AF.low.SetCertainBit(7, reg.low.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, false);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SRLH(ref Register16 reg) //Shift Right through carry - High
        {
            old = reg.high.GetCertainBit(0);
            reg.high.value = Convert.ToByte((reg.high.value >> 1) & 0xFF);
            AF.low.SetCertainBit(7, reg.high.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, old);
            AF.Sync();
            reg.Sync();
            return;
        }
        public void SRLL(ref Register16 reg) //Shift Right through carry - Low
        {
            old = reg.low.GetCertainBit(0);
            reg.low.value = Convert.ToByte((reg.low.value >> 1) & 0xFF);
            AF.low.SetCertainBit(7, reg.low.value == 0);
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, false);
            AF.low.SetCertainBit(4, old);
            AF.Sync();
            reg.Sync();
            return;
        }

        public void BITH(ref Register16 reg, byte b) //Test bit at location b in the high byte of reg
        {
            AF.low.SetCertainBit(7, !reg.high.GetCertainBit(b));
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, true);
            AF.Sync();
            return;
        }
        public void BITL(ref Register16 reg, byte b)//Test bit at location b in the low byte of reg
        {
            AF.low.SetCertainBit(7, !reg.low.GetCertainBit(b));
            AF.low.SetCertainBit(6, false);
            AF.low.SetCertainBit(5, true);
            AF.Sync();
            return;
        }
        public void RESH(ref Register16 reg, byte b) //Reset bit at location b in the high byte of reg
        {
            reg.high.SetCertainBit(b, false);
            return;
        }
        public void RESL(ref Register16 reg, byte b) //Reset bit at location b in the low byte of reg
        {
            reg.low.SetCertainBit(b, false);
            return;
        }
        public void SETH(ref Register16 reg, byte b) //Set bit at location b in the high byte of reg
        {
            reg.high.SetCertainBit(b, true);
            return;
        }
        public void SETL(ref Register16 reg, byte b) //Set bit at location b in the low byte of reg
        {
            reg.low.SetCertainBit(b, true);
            return;
        }
        public void DAA(int duration) //Someone actually thought this was a good idea. Weird 90s people
        {
            if ((AF.high.value & 0x0F) > 9)
                AF.high.value += 6;
            if ((AF.high.value & 0xF0) > 9)
                AF.high.value += 60;
            AF.Sync();
            return;
        }

        public void CB() //A WHOLE NEW SET OF INSTRUCTIONS.
        {
            PC.value++;
            switch (ram.Read(PC.value))
            {
                case 0x00: //RLC B
                    RLCH(ref BC);
                    break;
                case 0x01: //RLC C
                    RLCL(ref BC);
                    break;
                case 0x02: //RLC D
                    RLCH(ref DE);
                    break;
                case 0x03: //RLC E
                    RLCL(ref DE);
                    break;
                case 0x04: //RLC H
                    RLCH(ref HL);
                    break;
                case 0x05: //RLC L
                    RLCL(ref HL);
                    break;
                case 0x06: //RLC [HL]
                    AF.low.SetCertainBit(4, (ram.Read(HL.value) & (1 << 6)) != 0); // Set carry flag to the left most bit
                    ram.Write(Convert.ToByte((ram.Read(HL.value) << 1) & 0xFF), HL.value); // Shift left
                    ram.Edit(ram.Read(HL.value),Convert.ToByte(AF.low.GetCertainBit(4)) * 128); // Set left most bit to the carry flag
                    AF.low.SetCertainBit(7, ram.Read(HL.value) != 0); // Activate zero flag if A is 0
                    AF.low.SetCertainBit(6, false); // Reset subtract flag
                    AF.low.SetCertainBit(5, false); // Reset half carry flag
                    AF.Sync();
                    break;
                case 0x07: //RLC A
                    RLCH(ref AF);
                    break;
                case 0x08: //RRC B
                    RRCH(ref BC);
                    break;
                case 0x09: //RRC C
                    RRCL(ref BC);
                    break;
                case 0x0A: //RRC D
                    RRCH(ref DE);
                    break;
                case 0x0B: //RRC E
                    RRCL(ref DE);
                    break;
                case 0x0C: //RRC H
                    RRCH(ref HL);
                    break;
                case 0x0D: //RRC L
                    RRCL(ref HL);
                    break;
                case 0x0E: //RRC [HL]
                    AF.low.SetCertainBit(4, (ram.Read(HL.value) & (1)) != 0); // Set carry flag to the right most bit
                    ram.Write(Convert.ToByte((ram.Read(HL.value) >> 1) & 0xFF), HL.value); // Shift right
                    ram.Edit(ram.Read(HL.value), Convert.ToByte(AF.low.GetCertainBit(4))); // Set right most bit to the carry flag
                    AF.low.SetCertainBit(7, ram.Read(HL.value) != 0); // Activate zero flag if A is 0
                    AF.low.SetCertainBit(6, false); // Reset subtract flag
                    AF.low.SetCertainBit(5, false); // Reset half carry flag
                    AF.Sync();
                    break;
                case 0x0F: //RRC A
                    RRCH(ref AF);
                    break;
                case 0x10: //RL B
                    RLH(ref BC);
                    break;
                case 0x11: //RL C
                    RLL(ref BC);
                    break;
                case 0x12: //RL D
                    RLH(ref DE);
                    break;
                case 0x13: //RL E
                    RLL(ref DE);
                    break;
                case 0x14: //RL H
                    RLH(ref HL);
                    break;
                case 0x15: //RL L
                    RLL(ref HL);
                    break;
                case 0x16: //RL [HL]
                    old = (ram.Read(HL.value) & (1 << 6)) != 0; // Save old MSB
                    ram.Write(Convert.ToByte((ram.Read(HL.value) << 1) & 0xFF), HL.value); // Shift left
                    AF.low.SetCertainBit(4, old); // Set old MSB to carry
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) != 0) && !old); // Activate zero flag if the whole thing is 0
                    AF.low.SetCertainBit(6, false); // Reset subtract flag
                    AF.low.SetCertainBit(5, false);// Reset half carry flag
                    AF.Sync();
                    break;
                case 0x17: //RL A
                    RLH(ref AF);
                    break;
                case 0x18: //RR B
                    RRH(ref BC);
                    break;
                case 0x19: //RR C
                    RRL(ref BC);
                    break;
                case 0x1A: //RR D
                    RRH(ref DE);
                    break;
                case 0x1B: //RR E
                    RRL(ref DE);
                    break;
                case 0x1C: //RR H
                    RRH(ref HL);
                    break;
                case 0x1D: //RR L
                    RRL(ref HL);
                    break;
                case 0x1E: //RR [HL]
                    old = (ram.Read(HL.value) & (1)) != 0; // Save old LSB
                    ram.Write(Convert.ToByte((ram.Read(HL.value) >> 1) & 0xFF), HL.value); // Shift right
                    AF.low.SetCertainBit(4, old); // Set old LSB to carry
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) != 0) && !old); // Activate zero flag if the whole thing is 0
                    AF.low.SetCertainBit(6, false); // Reset subtract flag
                    AF.low.SetCertainBit(5, false);// Reset half carry flag
                    AF.Sync();
                    break;
                case 0x1F: //RR A
                    RRH(ref AF);
                    break;
                case 0x20: //SLA B
                    SLAH(ref BC);
                    break;
                case 0x21: //SLA C
                    SLAL(ref BC);
                    break;
                case 0x22: //SLA D
                    SLAH(ref DE);
                    break;
                case 0x23: //SLA E
                    SLAL(ref DE);
                    break;
                case 0x24: //SLA H
                    SLAH(ref HL);
                    break;
                case 0x25: //SLA L
                    SLAL(ref HL);
                    break;
                case 0x26: //SLA [HL]
                    old = (ram.Read(HL.value) & (1 << 7)) != 0;
                    ram.Write(Convert.ToByte((ram.Read(HL.value) << 1) & 0xFF), HL.value);
                    AF.low.SetCertainBit(7, ram.Read(HL.value) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, false);
                    AF.low.SetCertainBit(4, old);
                    AF.Sync();
                    break;
                case 0x27: //SLA A
                    SLAH(ref AF);
                    break;
                case 0x28: //SRA B
                    SRAH(ref BC);
                    break;
                case 0x29: //SRA C
                    SRAL(ref BC);
                    break;
                case 0x2A: //SRA D
                    SRAH(ref DE);
                    break;
                case 0x2B: //SRA E
                    SRAL(ref DE);
                    break;
                case 0x2C: //SRA H
                    SRAH(ref HL);
                    break;
                case 0x2D: //SRA L
                    SRAL(ref HL);
                    break;
                case 0x2E: //SRA [HL]
                    old = (ram.Read(HL.value) & 1) != 0;
                    ram.Write(Convert.ToByte((ram.Read(HL.value) >> 1) & 0xFF), HL.value);
                    AF.low.SetCertainBit(7, ram.Read(HL.value) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, false);
                    AF.low.SetCertainBit(4, old);
                    AF.Sync();
                    break;
                case 0x2F: //SRA A
                    SRAH(ref AF);
                    break;
                case 0x30: //SWAP B
                    SWAPH(ref BC);
                    break;
                case 0x31: //SWAP C
                    SWAPL(ref BC);
                    break;
                case 0x32: //SWAP D
                    SWAPH(ref DE);
                    break;
                case 0x33: //SWAP E
                    SWAPL(ref DE);
                    break;
                case 0x34: //SWAP H
                    SWAPH(ref HL);
                    break;
                case 0x35: //SWAP L
                    SWAPL(ref HL);
                    break;
                case 0x36: //SWAP [HL]
                    byte oldLow = Convert.ToByte(ram.Read(HL.value) & 0x0F);
                    byte high = Convert.ToByte(ram.Read(HL.value >> 4) & 0x0F);
                    ram.Write(Convert.ToByte(high >> 4 & oldLow << 4), HL.value);
                    AF.low.SetCertainBit(7, ram.Read(HL.value) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, false);
                    AF.low.SetCertainBit(4, false);
                    AF.Sync();
                    break;
                case 0x37: //SWAP A
                    SWAPH(ref AF);
                    break;
                case 0x38: //SRL B
                    SRLH(ref BC);
                    break;;
                case 0x39: //SRL C
                    SRLL(ref BC);
                    break;
                case 0x3A: //SRL D
                    SRLH(ref DE);
                    break;
                case 0x3B: //SRL E
                    SRLL(ref DE);
                    break;
                case 0x3C: //SRL H
                    SRLH(ref HL);
                    break;
                case 0x3D: //SRL L
                    SRLL(ref HL);
                    break;
                case 0x3E: //SRL [HL]
                    old = (ram.Read(HL.value) & 1) != 0;
                    ram.Write(Convert.ToByte((ram.Read(HL.value) >> 1) & 0xFF), HL.value);
                    AF.low.SetCertainBit(7, ram.Read(HL.value) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, false);
                    AF.low.SetCertainBit(4, old);
                    AF.Sync();
                    break;
                case 0x3F: //SRL A
                    SRLH(ref AF);
                    break;
                case 0x40: //BIT 0, B
                    BITH(ref BC, 0);
                    break;
                case 0x41: //BIT 0, C
                    BITL(ref BC, 0);
                    break;
                case 0x42: //BIT 0, D
                    BITH(ref DE, 0);
                    break;
                case 0x43: //BIT 0, E
                    BITL(ref DE, 0);
                    break;
                case 0x44: //BIT 0, H
                    BITH(ref HL, 0);
                    break;
                case 0x45: //BIT 0, L
                    BITL(ref HL, 0);
                    break;
                case 0x46: //BIT 0, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1)) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x47: //BIT 0, A
                    BITH(ref AF, 0);
                    break;
                case 0x48: //BIT 1, B
                    BITH(ref BC, 1);
                    break;
                case 0x49: //BIT 1, C
                    BITL(ref BC, 1);
                    break;
                case 0x4A: //BIT 1, D
                    BITH(ref DE, 1);
                    break;
                case 0x4B: //BIT 1, E
                    BITL(ref DE, 1);
                    break;
                case 0x4C: //BIT 1, H
                    BITH(ref HL, 1);
                    break;
                case 0x4D: //BIT 1, L
                    BITL(ref HL, 1);
                    break;
                case 0x4E: //BIT 1, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1 << 1)) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x4F: //BIT 1, A
                    BITH(ref AF, 1);
                    break;
                case 0x50: //BIT 2, B
                    BITH(ref BC, 2);
                    break;
                case 0x51: //BIT 2, C
                    BITL(ref BC, 2);
                    break;
                case 0x52: //BIT 2, D
                    BITH(ref DE, 2);
                    break;
                case 0x53: //BIT 2, E
                    BITL(ref DE, 2);
                    break;
                case 0x54: //BIT 2, H
                    BITH(ref HL, 2);
                    break;
                case 0x55: //BIT 2, L
                    BITL(ref HL, 2);
                    break;
                case 0x56: //BIT 2, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1 << 2)) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x57: //BIT 2, A
                    BITH(ref AF, 2);
                    break;
                case 0x58: //BIT 3, B
                    BITH(ref BC, 3);
                    break;
                case 0x59: //BIT 3, C
                    BITL(ref BC, 3);
                    break;
                case 0x5A: //BIT 3, D
                    BITH(ref DE, 3);
                    break;
                case 0x5B: //BIT 3, E
                    BITL(ref DE, 3);
                    break;
                case 0x5C: //BIT 3, H
                    BITH(ref HL, 3);
                    break;
                case 0x5D: //BIT 3, L
                    BITL(ref HL, 3);
                    break;
                case 0x5E: //BIT 3, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1 << 3)) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x5F: //BIT 3, A
                    BITH(ref AF, 3);
                    break;
                case 0x60: //BIT 4, B
                    BITH(ref BC, 4);
                    break;
                case 0x61: //BIT 4, C
                    BITL(ref BC, 4);
                    break;
                case 0x62: //BIT 4, D
                    BITH(ref DE, 4);
                    break;
                case 0x63: //BIT 4, E
                    BITL(ref DE, 4);
                    break;
                case 0x64: //BIT 4, H
                    BITH(ref HL, 4);
                    break;
                case 0x65: //BIT 4, L
                    BITL(ref HL, 4);
                    break;
                case 0x66: //BIT 4, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1 << 4)) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x67: //BIT 4, A
                    BITH(ref AF, 4);
                    break;
                case 0x68: //BIT 5, B
                    BITH(ref BC, 5);
                    break;
                case 0x69: //BIT 5, C
                    BITL(ref BC, 5);
                    break;
                case 0x6A: //BIT 5, D
                    BITH(ref DE, 5);
                    break;
                case 0x6B: //BIT 5, E
                    BITL(ref DE, 5);
                    break;
                case 0x6C: //BIT 5, H
                    BITH(ref HL, 5);
                    break;
                case 0x6D: //BIT 5, L
                    BITL(ref HL, 5);
                    break;
                case 0x6E: //BIT 5, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1 << 5)) == 0);
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x6F: //BIT 5, A
                    BITH(ref AF, 5);
                    break;
                case 0x70: //BIT 6, B
                    BITH(ref BC, 6);
                    break;
                case 0x71: //BIT 6, C
                    BITL(ref BC, 6);
                    break;
                case 0x72: //BIT 6, D
                    BITH(ref DE, 6);
                    break;
                case 0x73: //BIT 6, E
                    BITL(ref DE, 6);
                    break;
                case 0x74: //BIT 6, H
                    BITH(ref HL, 6);
                    break;
                case 0x75: //BIT 6, L
                    BITL(ref HL, 6);
                    break;
                case 0x76: //BIT 6, [HL]
                    AF.low.SetCertainBit(7, !((ram.Read(HL.value) & (1 << 6)) != 0));
                    AF.low.SetCertainBit(6, false);
                    AF.low.SetCertainBit(5, true);
                    AF.Sync();
                    break;
                case 0x77: // BIT 6, A
                    BITH(ref AF, 6);
                    break;
                case 0x78: //BIT 7, B
                    BITH(ref BC, 7);
                    break;
                case 0x79: //BIT 7, C
                    BITL(ref BC, 7);
                    break;
                case 0x7A: //BIT 7, D
                    BITH(ref DE, 7);
                    break;
                case 0x7B: //BIT 7, E
                    BITL(ref DE, 7);
                    break;
                case 0x7C: //BIT 7, H
                    BITH(ref HL, 7);
                    break;
                case 0x7D: //BIT 7, L
                    BITL(ref HL, 7);
                    break;
                case 0x7E: //BIT 7, [HL]
                    AF.low.SetCertainBit(7, (ram.Read(HL.value) & (1 << 7)) == 0);
                    AF.low.SetCertainBit(7, false);
                    AF.low.SetCertainBit(7, true);
                    AF.Sync();
                    break;
                case 0x7F: //BIT 7, A
                    BITH(ref AF, 7);
                    break;
                case 0x80: //RES 0, B
                    RESH(ref BC, 0);
                    break;
                case 0x81: //RES 0, C
                    RESL(ref BC, 0);
                    break;
                case 0x82: //RES 0, D
                    RESH(ref DE, 0);
                    break;
                case 0x83: //RES 0, E
                    RESL(ref DE, 0);
                    break;
                case 0x84: //RES 0, H
                    RESH(ref HL, 0);
                    break;
                case 0x85: //RES 0, L
                    RESL(ref HL, 0);
                    break;
                case 0x86: //RES 0, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[0] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0x87: //RES 0, A
                    RESH(ref AF, 0);
                    break;
                case 0x88: //RES 1, B
                    RESH(ref BC, 1);
                    break;
                case 0x89: //RES 1, C
                    RESL(ref BC, 1);
                    break;
                case 0x8A: //RES 1, D
                    RESH(ref DE, 1);
                    break;
                case 0x8B: //RES 1, E
                    RESL(ref DE, 1);
                    break;
                case 0x8C: //RES 1, H
                    RESH(ref HL, 1);
                    break;
                case 0x8D: //RES 1, L
                    RESL(ref HL, 1);
                    break;
                case 0x8E: //RES 1, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[1] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0x8F: //RES 1, A
                    RESH(ref AF, 1);
                    break;
                case 0x90: //RES 2, B
                    RESH(ref BC, 2);
                    break;
                case 0x91: //RES 2, C
                    RESL(ref BC, 2);
                    break;
                case 0x92: //RES 2, D
                    RESH(ref DE, 2);
                    break;
                case 0x93: //RES 2, E
                    RESL(ref DE, 2);
                    break;
                case 0x94: //RES 2, H
                    RESH(ref HL, 2);
                    break;
                case 0x95: //RES 2, L
                    RESL(ref HL, 2);
                    break;
                case 0x96: //RES 2, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[2] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0x97: //RES 2, A
                    RESH(ref AF, 2);
                    break;
                case 0x98: //RES 3, B
                    RESH(ref BC, 3);
                    break;
                case 0x99: //RES 3, C
                    RESL(ref BC, 3);
                    break;
                case 0x9A: //RES 3, D
                    RESH(ref DE, 3);
                    break;
                case 0x9B: //RES 3, E
                    RESL(ref DE, 3);
                    break;
                case 0x9C: //RES 3, H
                    RESH(ref HL, 3);
                    break;
                case 0x9D: //RES 3, L
                    RESL(ref HL, 3);
                    break;
                case 0x9E: //RES 3, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[3] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0x9F: //RES 3, A
                    RESH(ref AF, 3);
                    break;
                case 0xA0: //RES 4, B
                    RESH(ref BC, 4);
                    break;
                case 0xA1: //RES 4, C
                    RESL(ref BC, 4);
                    break;
                case 0xA2: //RES 4, D
                    RESH(ref DE, 4);
                    break;
                case 0xA3: //RES 4, E
                    RESL(ref DE, 4);
                    break;
                case 0xA4: //RES 4, H
                    RESH(ref HL, 4);
                    break;
                case 0xA5: //RES 4, L
                    RESL(ref HL, 4);
                    break;
                case 0xA6: //RES 4, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[4] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xA7: //RES 4, A
                    RESH(ref AF, 4);
                    break;
                case 0xA8: //RES 5, B
                    RESH(ref BC, 5);
                    break;
                case 0xA9: //RES 5, C
                    RESL(ref BC, 5);
                    break;
                case 0xAA: //RES 5, D
                    RESH(ref DE, 5);
                    break;
                case 0xAB: //RES 5, E
                    RESL(ref DE, 5);
                    break;
                case 0xAC: //RES 5, H
                    RESH(ref HL, 5);
                    break;
                case 0xAD: //RES 5, L
                    RESL(ref HL, 5);
                    break;
                case 0xAE: //RES 5, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[5] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xAF: //RES 5, A
                    RESH(ref AF, 5);
                    break;
                case 0xB0: //RES 6, B
                    RESH(ref BC, 6);
                    break;
                case 0xB1: //RES 6, C
                    RESL(ref BC, 6);
                    break;
                case 0xB2: //RES 6, D
                    RESH(ref DE, 6);
                    break;
                case 0xB3: //RES 6, E
                    RESL(ref DE, 6);
                    break;
                case 0xB4: //RES 6, H
                    RESH(ref HL, 6);
                    break;
                case 0xB5: //RES 6, L
                    RESL(ref HL, 6);
                    break;
                case 0xB6: //RES 6, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[6] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xB7: //RES 6, A
                    RESH(ref AF, 6);
                    break;
                case 0xB8: //RES 7, B
                    RESH(ref BC, 7);
                    break;
                case 0xB9: //RES 7, C
                    RESL(ref BC, 7);
                    break;
                case 0xBA: //RES 7, D
                    RESH(ref DE, 7);
                    break;
                case 0xBB: //RES 7, E
                    RESL(ref DE, 7);
                    break;
                case 0xBC: //RES 7, H
                    RESH(ref HL, 7);
                    break;
                case 0xBD: //RES 7, L
                    RESL(ref HL, 7);
                    break;
                case 0xBE: //RES 7, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[7] = '0';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xBF: //RES 7, A
                    RESH(ref AF, 7);
                    break;
                case 0xC0: //SET 0, B
                    SETH(ref BC, 0);
                    break;
                case 0xC1: //SET 0, C
                    SETL(ref BC, 0);
                    break;
                case 0xC2: //SET 0, D
                    SETH(ref DE, 0);
                    break;
                case 0xC3: //SET 0, E
                    SETL(ref DE, 0);
                    break;
                case 0xC4: //SET 0, H
                    SETH(ref HL, 0);
                    break;
                case 0xC5: //SET 0, L
                    SETL(ref HL, 0);
                    break;
                case 0xC6: //SET 0, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[0] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xC7: //SET 0, A
                    SETH(ref AF, 0);
                    break;
                case 0xC8: //SET 1, B
                    SETH(ref BC, 1);
                    break;
                case 0xC9: //SET 1, C
                    SETL(ref BC, 1);
                    break;
                case 0xCA: //SET 1, D
                    SETH(ref DE, 1);
                    break;
                case 0xCB: //SET 1, E
                    SETL(ref DE, 1);
                    break;
                case 0xCC: //SET 1, H
                    SETH(ref HL, 1);
                    break;
                case 0xCD: //SET 1, L
                    SETL(ref HL, 1);
                    break;
                case 0xCE: //SET 1, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[1] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xCF: //SET 1, A
                    SETH(ref AF, 1);
                    break;
                case 0xD0: //SET 2, B
                    SETH(ref BC, 2);
                    break;
                case 0xD1: //SET 2, C
                    SETL(ref BC, 2);
                    break;
                case 0xD2: //SET 2, D
                    SETH(ref DE, 2);
                    break;
                case 0xD3: //SET 2, E
                    SETL(ref DE, 2);
                    break;
                case 0xD4: //SET 2, H
                    SETH(ref HL, 2);
                    break;
                case 0xD5: //SET 2, L
                    SETL(ref HL, 2);
                    break;
                case 0xD6: //SET 2, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[2] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xD7: //SET 2, A
                    SETH(ref AF, 2);
                    break;
                case 0xD8: //SET 3, B
                    SETH(ref BC, 3);
                    break;
                case 0xD9: //SET 3, C
                    SETL(ref BC, 3);
                    break;
                case 0xDA: //SET 3, D
                    SETH(ref DE, 3);
                    break;
                case 0xDB: //SET 3, E
                    SETL(ref DE, 3);
                    break;
                case 0xDC: //SET 3, H
                    SETH(ref HL, 3);
                    break;
                case 0xDD: //SET 3, L
                    SETL(ref HL, 3);
                    break;
                case 0xDE: //SET 3, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[3] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xDF: //SET 3, A
                    SETH(ref AF, 3);
                    break;
                case 0xE0: //SET 4, B
                    SETH(ref BC, 4);
                    break;
                case 0xE1: //SET 4, C
                    SETL(ref BC, 4);
                    break;
                case 0xE2: //SET 4, D
                    SETH(ref DE, 4);
                    break;
                case 0xE3: //SET 4, E
                    SETL(ref DE, 4);
                    break;
                case 0xE4: //SET 4, H
                    SETH(ref HL, 4);
                    break;
                case 0xE5: //SET 4, L
                    SETL(ref HL, 4);
                    break;
                case 0xE6: //SET 4, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[4] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xE7: //SET 4, A
                    SETH(ref AF, 4);
                    break;
                case 0xE8: //SET 5, B
                    SETH(ref BC, 5);
                    break;
                case 0xE9: //SET 5, C
                    SETL(ref BC, 5);
                    break;
                case 0xEA: //SET 5, D
                    SETH(ref DE, 5);
                    break;
                case 0xEB: //SET 5, E
                    SETL(ref DE, 5);
                    break;
                case 0xEC: //SET 5, H
                    SETH(ref HL, 5);
                    break;
                case 0xED: //SET 5, L
                    SETL(ref HL, 5);
                    break;
                case 0xEE: //SET 5, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[5] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xEF: //SET 5, A
                    SETH(ref AF, 5);
                    break;
                case 0xF0: //SET 6, B
                    SETH(ref BC, 6);
                    break;
                case 0xF1: //SET 6, C
                    SETL(ref BC, 6);
                    break;
                case 0xF2: //SET 6, D
                    SETH(ref DE, 6);
                    break;
                case 0xF3: //SET 6, E
                    SETL(ref DE, 6);
                    break;
                case 0xF4: //SET 6, H
                    SETH(ref HL, 6);
                    break;
                case 0xF5: //SET 6, L
                    SETL(ref HL, 6);
                    break;
                case 0xF6: //SET 6, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[6] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xF7: //SET 6, A
                    SETH(ref AF, 6);
                    break;
                case 0xF8: //SET 7, B
                    SETH(ref BC, 7);
                    break;
                case 0xF9: //SET 7, C
                    SETL(ref BC, 7);
                    break;
                case 0xFA: //SET 7, D
                    SETH(ref DE, 7);
                    break;
                case 0xFB: //SET 7, E
                    SETL(ref DE, 7);
                    break;
                case 0xFC: //SET 7, H
                    SETH(ref HL, 7);
                    break;
                case 0xFD: //SET 7, L
                    SETL(ref HL, 7);
                    break;
                case 0xFE: //SET 7, [HL]
                    temp = Convert.ToString(ram.Read(HL.value)).PadLeft(8, '0').ToCharArray();
                    temp[7] = '1';
                    ram.Write(Convert.ToByte(tmp.ToString().Substring(0, 8), 2), HL.value);
                    break;
                case 0xFF: //SET 7, A
                    SETH(ref AF, 7);
                    break;
            }
        }
    }
}
