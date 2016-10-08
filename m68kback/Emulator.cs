using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m68kback
{
    public class Emulator
    {
        private IList<M68kInstruction> instructions;

        uint globalUsed = 0;
        Dictionary<string, uint> globalAddress = new Dictionary<string, uint>();

        public Emulator(IList<M68kInstruction> insts, Dictionary<string, Declaration> globals)
        {
            instructions = insts;

            foreach (var g in globals)
            {
                if (g.Value.Value != null)
                {
                    globalAddress[g.Key] = globalUsed;

                    var bytes = Encoding.ASCII.GetBytes(g.Value.Value);
                    Array.Copy(bytes, 0, memory, globalUsed, bytes.Length);

                    globalUsed += (uint) g.Value.Value.Length;
                }
            }
        }

        public uint AllocGlobal(uint data)
        {
            var addr = globalUsed;

            var bytes = BitConverter.GetBytes(data);

            Array.Copy(bytes, 0, memory, addr, 4);
            globalUsed += 4;

            return addr;
        }

        public uint AllocGlobal(string data)
        {
            var addr = globalUsed;

            var bytes = Encoding.ASCII.GetBytes(data);
            Array.Copy(bytes, 0, memory, globalUsed, bytes.Length);
            globalUsed += (uint)bytes.Length;

            return addr;
        }

        private int pc;

        public void RunFunction(string name, params object[] pars)
        {
            var start = name != null ? instructions.First(i => i.Opcode == M68kOpcode.Label && i.Label == name) : instructions.First();
            pc = instructions.IndexOf(start);
            Sp = (uint)memory.Length - 4;

            foreach (var par in pars)
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
                    Array.Copy(bytes, 0, memory, globalUsed, bytes.Length);
                    memory[bytes.Length] = 0;
                    globalUsed += (uint)bytes.Length + 1;
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
        private uint ccr;

        byte[] memory = new byte[4096];

        public byte[] Memory
        {
            get { return memory; }
        }

        uint Read32(uint addr)
        {
            return BitConverter.ToUInt32(memory, (int)addr);
        }

        void Write32(uint addr, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, memory, addr, 4);
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
            var sb = new StringBuilder();
            int i = start;
            for (; arr[i] != 0; i++) ;

            return Encoding.ASCII.GetString(arr, start, i - start);
        }

        void Run()
        {
            int maxI = 1000;
            int n = 0;

            while (true)
            {
                n++;
                if (n > maxI)
                {
                    throw new Exception("Maximum number of instructions.");
                }

                Console.WriteLine($"A0: {Regs[(int)M68kRegister.A0]}, A1: {Regs[(int)M68kRegister.A1]}, D0: {Regs[(int)M68kRegister.D0]}, D1: {Regs[(int)M68kRegister.D1]}");

                var i = instructions[pc];
                Console.WriteLine(i);

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
                                    Z = val2 - val1 == 0;
                                    N = (val2 - val1) < 0;
                                }
                                break;
                                case M68Width.Byte:
                                {
                                    var val1 = i.FinalRegister1.HasValue ? (byte)(Regs[(int)i.FinalRegister1] & 0xFF) : (byte)i.Immediate;
                                    var val2 = (byte)(Regs[(int)i.FinalRegister2] & 0xFF);
                                    Z = val2 - val1 == 0;
                                    N = ((int)val2 - (int)val1) < 0;
                                }
                                break;
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
                                    var strPtr = memory[Sp + 4];
                                    Console.WriteLine(NullTerminatedToString(memory, strPtr));
                                }
                            }
                        }
                        break;
                    case M68kOpcode.Lea:
                        Regs[(int) i.FinalRegister2] = globalAddress[i.Variable];
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
                        var valToAdd = i.AddressingMode1 == M68kAddressingMode.Immediate
                            ? i.Immediate
                            : (int)Regs[(int) i.FinalRegister1];

                        Regs[(int)i.FinalRegister2] = (uint)((int)Regs[(int)i.FinalRegister2] + valToAdd);
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
                    default:
                        throw new Exception($"Unknown opcode {i.Opcode}");
                }

                pc++;
            }
        }
    }
}
