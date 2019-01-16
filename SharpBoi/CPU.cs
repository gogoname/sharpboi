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
            addr = 0;
            tmp = 0;
        }

        public void Start(byte[] opcodes)
        {
            int counter = 0;
            ram.Write(opcodes, 0);
            while (true)
            {
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
                            RRCA(4);
                            break;
                        case 0x10: //STOP
                            isStopped = true;
                            break;
                        case 0x11: //LD DE, d16
                            DE.value = BitConverter.ToUInt16(
                                new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                            PC.value++;
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
                            RLA(4);
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
                            RRA(4);
                            break;
                        case 0x20: //JR NZ, r8
                            addr = (sbyte) ram.Read(PC.value + 1);
                            PC.value++;
                            if (!AF.low.GetCertainBit(7))
                            {
                                if (addr > 0)
                                    PC.value += Convert.ToUInt16(addr - 1);
                                else
                                    PC.value -= Convert.ToUInt16((addr - 1) * -1);
                            }

                            break;
                        case 0x21: //LD HL, d16
                            HL.value = BitConverter.ToUInt16(
                                new byte[2] {ram.Read(PC.value + 1), ram.Read(PC.value + 2)}, 0);
                            PC.value++;
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
                            PC.value++;
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
                        case 0xCB: //THE MIGHTY CB INSTRUCTION (Opens up a bunch of more instructions
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
                    counter++;
                }
            }
        }

        public void RLCH(ref Register16 reg)
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
        public void RLCL(ref Register16 reg)
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
        public void RRCA(int duration)
        {
            AF.low.SetCertainBit(4, AF.high.GetCertainBit(0)); // Set carry flag to the right most bit
            AF.high.value = Convert.ToByte((AF.high.value >> 1) & 0xFF); // Shift right
            AF.high.SetCertainBit(0, AF.low.GetCertainBit(4)); // Set right most bit to the carry flag
            AF.low.SetCertainBit(7, AF.high.value != 0); // Activate zero flag if A is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false); // Reset half carry flag
            AF.Sync();
            return;
        }
        public void RLA(int duration)
        {
            bool old = AF.high.GetCertainBit(7); // Save old MSB
            AF.high.value = Convert.ToByte((AF.high.value << 1) & 0xFF); // Shift left
            AF.low.SetCertainBit(4, old); // Set old MSB to carry
            AF.low.SetCertainBit(7, (AF.high.value != 0) && !old); // Activate zero flag if the whole thing is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false);// Reset half carry flag
            AF.Sync();
            return;
        }
        public void RRA(int duration)
        {
            bool old = AF.high.GetCertainBit(0); // Save old LSB
            AF.high.value = Convert.ToByte((AF.high.value >> 1) & 0xFF); // Shift right
            AF.low.SetCertainBit(4, old); // Set old LSB to carry
            AF.low.SetCertainBit(7, (AF.high.value != 0) && !old); // Activate zero flag if the whole thing is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false);// Reset half carry flag
            AF.Sync();
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
            }
        }
    }
}
