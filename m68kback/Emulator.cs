#define PRINTSTATES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m68kback
{
    public class Emulator : IStackAccess
    {
        private IList<M68kInstruction> instructions;

        public IList<M68kInstruction> Instructions => instructions;

        uint globalUsed = 0;
        Dictionary<string, uint> globalAddress = new Dictionary<string, uint>();
        private IPrintf printf;

        public Dictionary<string, Func<IStackAccess,uint>> Functions { get; set; } = new Dictionary<string, Func<IStackAccess, uint>>();

        public Emulator(IList<M68kInstruction> insts, Dictionary<string, Declaration> globals, 
            IPrintf printf)
        {
            this.printf = printf;
            instructions = insts;

            foreach (var g in globals.Where(g => !g.Value.Declare))
            {
                //if (g.Value.Value != null)
                {
                    globalAddress[g.Key] = globalUsed;

                    if (g.Value.Value != null)
                    {
                        var bytes = Encoding.ASCII.GetBytes(g.Value.Value);
                        Array.Copy(bytes, 0, memory, (int) globalUsed, bytes.Length);
                    }

                    globalUsed += (uint) g.Value.Type.Width;

                    // align to 4 bytes
                    globalUsed = (globalUsed + 3)/4 * 4;
                }
            }
        }

        void AlignGlobal()
        {
            while (globalUsed%4 > 0) globalUsed++;
        }

        public uint AllocGlobal(uint data)
        {
            AlignGlobal();
            var addr = globalUsed;

            var bytes = BitConverter.GetBytes(data);

            Array.Copy(bytes, 0, memory, (int)addr, 4);
            globalUsed += 4;

            return addr;
        }

        public uint AllocGlobal(string data)
        {
            AlignGlobal();
            var addr = globalUsed;

            var bytes = Encoding.ASCII.GetBytes(data);
            Array.Copy(bytes, 0, memory, (int)globalUsed, bytes.Length);
            memory[globalUsed + bytes.Length] = 0;

            globalUsed += (uint)(bytes.Length + 1);

            return addr;
        }

        private int pc;

        public int MaximumInstructionsToExecute { get; set; }

        public void RunFunction(string name, params object[] pars)
        {
            var start = name != null ? instructions.First(i => i.Opcode == M68kOpcode.Label && i.Label == name) : instructions.First();
            pc = instructions.IndexOf(start);
            Sp = (uint)memory.Length - 4;

            // Push in reverse order
            foreach (var par in pars.Reverse())
            {
                if (par is int)
                {
                    Sp -= 4;
                    Write32(Sp, (uint)(int) par);
                }
                else if (par is uint)
                {
                    Sp -= 4;
                    Write32(Sp, (uint)par);
                }
                else if (par is string)
                {
                    var str = (string) par;
                    Sp -= 4;
                    Write32(Sp, (uint)globalUsed);

                    var bytes = Encoding.ASCII.GetBytes(str);
                    Array.Copy(bytes, 0, memory, (int)globalUsed, bytes.Length);
                    memory[globalUsed + bytes.Length] = 0;

                    var len = bytes.Length + 1;

                    globalUsed += (uint)len;
                    // align to 4 bytes
                    globalUsed = (globalUsed + 3) / 4 * 4;
                }
            }

            Sp-=4;
            Write32(Sp, uint.MaxValue);

            Run();
        }

        uint Sp
        {
            get { return Regs[(int) M68kRegister.SP]; }
            set { Regs[(int) M68kRegister.SP] = value; }
        }

        public uint[] Regs { get; } = new uint[16];

        byte[] memory = new byte[4096];

        public byte[] Memory
        {
            get { return memory; }
        }

        uint Read32(uint addr)
        {
            uint val = BitConverter.ToUInt32(memory, (int)addr);
#if PRINTSTATES
//            Console.WriteLine($"Read32({addr}) = {val}");
#endif
            return val;
        }

        void Write32(uint addr, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, memory, (int)addr, 4);
#if PRINTSTATES
            //Console.WriteLine($"Write32({addr}) = {value}");
#endif
        }

        byte Read8(uint addr)
        {
            return memory[addr];
        }

        void Write8(uint addr, byte val)
        {
            memory[addr] = val;
        }

        private bool N;
        private bool Z;
        private bool V;
        private bool C;

        string NullTerminatedToString(byte[] arr, int start)
        {
            int i = start;
            for (; arr[i] != 0; i++) ;

            return Encoding.ASCII.GetString(arr, start, i - start);
        }

        void Run()
        {
            int maxI = MaximumInstructionsToExecute;
            int n = 0;

            while (true)
            {
                n++;
                if (n > maxI)
                {
                    throw new Exception("Maximum number of instructions.");
                }

#if PRINTSTATES
                Console.WriteLine($"SP: {Sp} A0: {Regs[(int)M68kRegister.A0]}, A1: {Regs[(int)M68kRegister.A1]}, D0: {Regs[(int)M68kRegister.D0]}, D1: {Regs[(int)M68kRegister.D1]}");
#endif
                var i = instructions[pc];
#if PRINTSTATES
                Console.WriteLine(i);
#endif
                switch (i.Opcode)
                {
                    case M68kOpcode.Label:
                        break;
                    case M68kOpcode.Cmp:
                        switch (i.Width)
                        {
                            case M68Width.Long:
                                {
                                    var val1 = i.FinalRegister1.HasValue ? (int)Regs[(int)i.FinalRegister1] : i.Immediate;
                                    var val2 = (int)Regs[(int)i.FinalRegister2];
                                    var res = val2 - val1;
                                    Z = res == 0;
                                    N = res < 0;

                                    //V = (val2 > 0 && res < 0) || (val2 < 0 && res > 0);
                                    V = (res < val2) != (val1 > 0);
                                }
                                break;
                                case M68Width.Byte:
                                {
                                    var val1 = i.FinalRegister1.HasValue ? (byte)(Regs[(int)i.FinalRegister1] & 0xFF) : (byte)i.Immediate;
                                    var val2 = (byte)(Regs[(int)i.FinalRegister2] & 0xFF);
                                    var res = val2 - val1;
                                    Z = res == 0;
                                    N = res < 0;
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                        break;
                    case M68kOpcode.Rts:
                        {
                            var addr = Read32(Sp);
                            Sp += 4;
                            if (addr == uint.MaxValue)
                            {
                                return;
                            }
                            pc = (int)addr;
                        }
                        break;
                    case M68kOpcode.Jsr:
                        {
                            var entry = instructions.FirstOrDefault(ins => ins.Label == i.TargetLabel);
                            if (entry != null)
                            {
                                Sp -= 4;
                                Write32(Sp, (uint) pc);
                                pc = instructions.IndexOf(entry) - 1;
                            }
                            else
                            {
                                if (i.TargetLabel == "@printf")
                                {
                                    //var strPtr = memory[Sp + 4];
                                    var strPtr = Read32(Sp);
                                    Regs[0] = printf.printf(NullTerminatedToString(memory, (int) strPtr), this);
                                }
                                else
                                {
                                    var func = Functions[i.TargetLabel];
                                    Regs[0] = func(this);
                                }
                            }
                        }
                        break;
                    case M68kOpcode.Lea:
                        {
                            uint val;
                            if (i.AddressingMode1 == M68kAddressingMode.AddressWithOffset)
                            {
                                val = (uint) (Regs[(int) i.FinalRegister1] + i.Offset);
                            }
                            else
                            {
                                val = globalAddress[i.Variable];
                            }
                            Regs[(int) i.FinalRegister2] = val;
                        }
                        break;
                    case M68kOpcode.MoveQ:
                        {
                            var val = (uint)i.Immediate;
                            Regs[(int)i.FinalRegister2] = val;
                        }
                        break;
                    case M68kOpcode.Move:
                    case M68kOpcode.MoveA:
                        switch (i.Width)
                        {
                            case null:
                            case M68Width.Long:
                            {
                                uint val;
                                switch (i.AddressingMode1)
                                {
                                    case M68kAddressingMode.Address:
                                        val = Read32(Regs[(int)i.FinalRegister1.Value]);
                                        break;
                                    case M68kAddressingMode.AddressWithOffset:
                                        val = Read32(Regs[(int) i.FinalRegister1.Value] + (uint) i.Offset.Value);
                                        break;
                                    case M68kAddressingMode.Immediate:
                                        val = (uint) i.Immediate;
                                        break;
                                    case M68kAddressingMode.Register:
                                        val = Regs[(int)i.FinalRegister1];
                                        break;
                                    default:
                                        throw new NotSupportedException();
                                }

                                switch (i.AddressingMode2)
                                {
                                    case M68kAddressingMode.Register:
                                        Regs[(int) i.FinalRegister2] = val;
                                        break;
                                    case M68kAddressingMode.AddressWithOffset:
                                        Write32(Regs[(uint) i.FinalRegister2.Value] + (uint) i.Offset.Value, val);
                                        break;
                                    case M68kAddressingMode.Address:
                                        Write32(Regs[(uint)i.FinalRegister2.Value], val);
                                        break;
                                    case M68kAddressingMode.AddressWithPreDecrement:
                                        Regs[(uint) i.FinalRegister2.Value] -= (uint)i.WidthInBytes;
                                        Write32(Regs[(uint)i.FinalRegister2.Value], val);
                                        break;
                                    default:
                                        throw new NotSupportedException();
                                }
                                }
                                break;
                            case M68Width.Byte:
                            {
                                    byte val;
                                    switch (i.AddressingMode1)
                                    {
                                        case M68kAddressingMode.Address:
                                        case M68kAddressingMode.AddressWithOffset:
                                            val = Read8(Regs[(int) i.FinalRegister1.Value] + (uint) (i.Offset ?? 0));
                                            break;
                                        case M68kAddressingMode.Register:
                                            val = (byte) (Regs[(int) i.FinalRegister1] & 0xFF);
                                            break;
                                        default:
                                            throw new NotSupportedException();
                                    }

                                    switch (i.AddressingMode2)
                                    {
                                        case M68kAddressingMode.Register:
                                            Regs[(int)i.FinalRegister2] = val | (Regs[(int)i.FinalRegister2] & 0xFFFFFF00);
                                            break;
                                        case M68kAddressingMode.AddressWithOffset:
                                            Write8(Regs[(uint)i.FinalRegister2.Value] + (uint)i.Offset, val);
                                            break;
                                        case M68kAddressingMode.Address:
                                            Write8(Regs[(uint)i.FinalRegister2.Value], val);
                                            break;
                                        default:
                                            throw new NotSupportedException();
                                    }
                                }
                                break;
                        }
                        break;
                    // TODO: Update status register accordingly
                    case M68kOpcode.Sub:
                    {
                        var val1 = (i.AddressingMode1 == M68kAddressingMode.Immediate
                            ?  i.Immediate
                            : (int)Regs[(int) i.FinalRegister1.Value]);

                        Regs[(int) i.FinalRegister2] = (uint)((int)Regs[(int) i.FinalRegister2] - val1);
                        }
                        break;
                    case M68kOpcode.Add:
                    case M68kOpcode.Adda:
                        {
                            var valToAdd = i.AddressingMode1 == M68kAddressingMode.Immediate
                                ? i.Immediate
                                : (int) Regs[(int) i.FinalRegister1];

                            Regs[(int) i.FinalRegister2] = (uint) ((int) Regs[(int) i.FinalRegister2] + valToAdd);
                        }
                        break;
                    case M68kOpcode.Eor:
                        {
                            var valToAdd = i.AddressingMode1 == M68kAddressingMode.Immediate
                                ? i.Immediate
                                : (int)Regs[(int)i.FinalRegister1];
                            Regs[(int)i.FinalRegister2] = (uint)((int)Regs[(int)i.FinalRegister2] ^ valToAdd);
                        }
                        break;
                    case M68kOpcode.Jmp:
                        pc = instructions.IndexOf(instructions.First(ins => ins.Label == i.TargetLabel.Substring(1)))-1;
                        break;
                    case M68kOpcode.Bne:
                        if (!Z)
                        {
                            pc = instructions.IndexOf(instructions.First(ins => ins.Label == i.TargetLabel.Substring(1)))-1;
                        }
                        break;
                    case M68kOpcode.Beq:
                        if (Z)
                        {
                            pc = instructions.IndexOf(instructions.First(ins => ins.Label == i.TargetLabel.Substring(1)))-1;
                        }
                        break;
                    case M68kOpcode.Bgt:
                        if ((N && V && !Z) || (!N && !V && !Z))
                        {
                            pc = instructions.IndexOf(instructions.First(ins => ins.Label == i.TargetLabel.Substring(1)))-1;
                        }
                        break;
                    case M68kOpcode.Blt:
                        if ((N && !V) || (!N && V))
                        {
                            pc = instructions.IndexOf(instructions.First(ins => ins.Label == i.TargetLabel.Substring(1))) - 1;
                        }
                        break;
                    case M68kOpcode.Lsl:
                        Regs[(int) i.FinalRegister2] = Regs[(int) i.FinalRegister2] << i.Immediate.Value;
                        break;
                    case M68kOpcode.Lsr:
                    {
                        var shifter = i.AddressingMode1 == M68kAddressingMode.Immediate
                            ? (uint)i.Immediate.Value
                            : Regs[(int) i.FinalRegister1];
                            Regs[(int) i.FinalRegister2] = Regs[(int) i.FinalRegister2] >> (int)shifter;
                        }
                        break;
                    case M68kOpcode.Tst:
                        {
                            var val = i.FinalRegister1.HasValue ? (int)Regs[(int)i.FinalRegister1] : i.Immediate;
                            Z = val == 0;
                            N = val < 0;
                        }
                        break;
                    case M68kOpcode.Divs:
                        {
                            if (i.Width == M68Width.Word || i.Width == null)
                            {
                                var divResult = (int) Regs[(int) i.FinalRegister2]/(int) Regs[(int) i.FinalRegister1];
                                var modResult = (int) Regs[(int) i.FinalRegister2]%(int) Regs[(int) i.FinalRegister1];
                                var res = modResult << 16 | divResult & 0xFFFF;
                                Regs[(int) i.FinalRegister2] = (uint) res;
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        break;
                    case M68kOpcode.And:
                        {
                            var source = i.AddressingMode1 == M68kAddressingMode.Immediate
                                ? i.Immediate
                                : (int)Regs[(int) i.FinalRegister1];
                            Regs[(int) i.FinalRegister2] = (uint)(Regs[(int) i.FinalRegister2] & source);
                        }
                        break;
                    default:
                        throw new Exception($"Unknown opcode {i.Opcode}");
                }

                pc++;
            }
        }

        public string GetString(int ix)
        {
            var strPtr = Read32((uint)(Sp + 4 + ix*4)); //memory[Sp + 4 + ix * 4];
            return NullTerminatedToString(memory, (int)strPtr);
        }

        public uint GetUint(int ix)
        {
            return Read32(Sp + 4 + (uint)ix * 4);
        }

        public int GetInt(int ix)
        {
            return (int)Read32(Sp + 4 + (uint)ix * 4);
        }
    }
}
