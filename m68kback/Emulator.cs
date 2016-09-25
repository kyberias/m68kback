using System;
using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class Emulator
    {
        private IList<M68kInstruction> instructions;

        public Emulator(IList<M68kInstruction> insts)
        {
            instructions = insts;
        }

        private int pc;

        public void RunFunction(string name, params int[] pars)
        {
            var start = instructions.First(i => i.Opcode == M68kOpcode.Label && i.Label == name);
            pc = instructions.IndexOf(start);
            Sp = (uint)memory.Length - 4;

            foreach (var par in pars)
            {
                Sp-=4;
                Write32(Sp, (uint)par);
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

        uint Read32(uint addr)
        {
            return BitConverter.ToUInt32(memory, (int)addr);
        }

        void Write32(uint addr, uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, memory, addr, 4);
        }

        void Run()
        {
            while (true)
            {
                var i = instructions[pc];

                switch (i.Opcode)
                {
                    case M68kOpcode.Label:
                        break;
                    case M68kOpcode.Rts:
                        if (Read32(Sp) == uint.MaxValue)
                        {
                            Sp+=4;
                            return;
                        }
                        break;
                    case M68kOpcode.Move:
                        {
                            var val = i.AddressingMode1 == M68kAddressingMode.AddressWithOffset
                                ? Read32(Regs[(int)i.FinalRegister1.Value] + (uint)i.Offset.Value)
                                : Regs[(int) i.FinalRegister1];

                            switch (i.AddressingMode2)
                            {
                                case M68kAddressingMode.Register:
                                    Regs[(int) i.FinalRegister2] = val;
                                    break;
                                case M68kAddressingMode.AddressWithOffset:
                                    Write32(Regs[(uint)i.FinalRegister2.Value] + (uint)i.Offset, val);
                                    break;
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                        break;
                    case M68kOpcode.Sub:
                        Regs[(int) i.FinalRegister2] = Regs[(int) i.FinalRegister2] - (uint)i.Immediate;
                        break;
                    case M68kOpcode.Add:

                        var valToAdd = i.AddressingMode1 == M68kAddressingMode.Immediate
                            ? (uint) i.Immediate
                            : Regs[(int) i.FinalRegister1];

                        Regs[(int)i.FinalRegister2] = Regs[(int)i.FinalRegister2] + valToAdd;
                        break;
                    default:
                        throw new Exception("Unknown opcode");
                }

                pc++;
            }
        }
    }
}
