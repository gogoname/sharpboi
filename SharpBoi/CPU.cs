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
        short addr;
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
            short addr = 0;
        }
        public void ParseInstruction(byte opcode, RAM ram)
        {
            if (!(isHalted | isStopped))
            {
                switch (opcode)
                {
                    case 0x00: //NOP
                        break;
                    case 0x01: //LD BC, d16
                        BC.value = BitConverter.ToUInt16(new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
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
                        RLCA(4);
                        break;
                    case 0x08: //LD [a16], SP
                        SP.value = ram.Read(BitConverter.ToUInt16(new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0));
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
                        DE.value = BitConverter.ToUInt16(new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
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
                        addr = ram.Read(PC.value + 1);
                        PC.value++;
                        if (addr > 0)
                            PC.value += Convert.ToUInt16(addr);
                        else
                            PC.value -= Convert.ToUInt16(addr * -1);
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
                        addr = ram.Read(PC.value + 1);
                        PC.value++;
                        if (!AF.low.GetCertainBit(7))
                        {
                            if (addr > 0)
                                PC.value += Convert.ToUInt16(addr);
                            else
                                PC.value -= Convert.ToUInt16(addr * -1);
                        }
                        break;
                    case 0x21: //LD HL, d16
                        HL.value = BitConverter.ToUInt16(new byte[2] { ram.Read(PC.value + 1), ram.Read(PC.value + 2) }, 0);
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
                        addr = ram.Read(PC.value + 1);
                        PC.value++;
                        if (AF.low.GetCertainBit(7))
                        {
                            if (addr > 0)
                                PC.value += Convert.ToUInt16(addr);
                            else
                                PC.value -= Convert.ToUInt16(addr * -1);
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
                        addr = ram.Read(PC.value + 1);
                        PC.value++;
                        if (!AF.low.GetCertainBit(4))
                        {
                            if (addr > 0)
                                PC.value += Convert.ToUInt16(addr);
                            else
                                PC.value -= Convert.ToUInt16(addr * -1);
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
                        addr = ram.Read(PC.value + 1);
                        PC.value++;
                        if (AF.low.GetCertainBit(4))
                        {
                            if (addr > 0)
                                PC.value += Convert.ToUInt16(addr);
                            else
                                PC.value -= Convert.ToUInt16(addr * -1);
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
                        AF.high.value += Convert.ToByte(ram.Read(HL.value) + Convert.ToByte(AF.low.GetCertainBit(4)));
                        AF.low.SetCertainBit(4, AF.high.value == 255);
                        AF.Sync();
                        break;
                }
                PC.value++;
            }
        }

        public void RLCA(int duartion)
        {
            AF.low.SetCertainBit(4, AF.high.GetCertainBit(7)); // Set carry flag to the left most bit
            AF.high.value = Convert.ToByte((AF.high.value << 1) & 0xFF); // Shift left
            AF.high.SetCertainBit(7, AF.low.GetCertainBit(4)); // Set left most bit to the carry flag
            AF.low.SetCertainBit(7, AF.high.value != 0); // Activate zero flag if A is 0
            AF.low.SetCertainBit(6, false); // Reset subtract flag
            AF.low.SetCertainBit(5, false); // Reset half carry flag
            AF.Sync();
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
        }
        public void DAA(int duration) //Someone actually thought this was a good idea. Weird 90s people
        {
            if ((AF.high.value & 0x0F) > 9)
                AF.high.value += 6;
            if ((AF.high.value & 0xF0) > 9)
                AF.high.value += 60;
            AF.Sync();
        }
    }
}
