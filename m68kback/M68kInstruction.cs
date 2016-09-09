using System.Collections.Generic;
using System.Text;

namespace m68kback
{

    public enum M68kOpcode
    {
        Label,
        Move,
        MoveA,
        MoveQ,
        Lea,
        Rts,
        Jmp,
        Jsr,
        Bgt,
        Beq,
        Bne,
        Blt,
        Tst,
        Adda,
        Add,
        Sub,
        Cmp,
        Divs,
        Lsr
    }

    public enum M68kAddressingMode
    {
        Register,
        AddressRegister,
        Address, // (An)
        AddressWithPostIncrement, // (An)+
        AddressWithPreIncrement, // +(An)
        AddressWithPreDecrement, // -(An)
        AddressWithOffset, // #x(An)
        Absolute,
        Immediate
    }

    public enum M68kRegister
    {
        D0 = 0,
        D1,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        A0,
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        SP,
        CCR
    }

    public enum M68Width
    {
        Byte,
        Word,
        Long
    }

    public enum RegType
    {
        Data,
        Address,
        ConditionCode
    }

    public class Register
    {
        public RegType Type { get; set; }
        public int Number { get; set; }
        public Token Condition { get; set; }

        public override string ToString()
        {
            string str = "";
            switch (Type)
            {
                case RegType.Address:
                    str += "A";
                    break;
                case RegType.Data:
                    str += "D";
                    break;
                case RegType.ConditionCode:
                    str += "CCR";
                    break;
            }
            return str + Number;
        }
    }

    public class M68kInstruction
    {
        public string Label { get; set; }
        public M68kOpcode Opcode { get; set; }
        public M68Width? Width { get; set; }
        //public M68kRegister? Register1 { get; set; }
        //public M68kRegister? Register2 { get; set; }
        public Register Register1 { get; set; }
        public Register Register2 { get; set; }
        public M68kRegister? FinalRegister1 { get; set; }
        public M68kRegister? FinalRegister2 { get; set; }
        public string RegOperand1 { get; set; }
        public string RegOperand2 { get; set; }
        public M68kAddressingMode AddressingMode1 { get; set; }
        public M68kAddressingMode AddressingMode2 { get; set; }
        public int? Immediate { get; set; }
        public int? Offset { get; set; }
        public string TargetLabel { get; set; }
        public string Variable { get; set; }
        public string Comment { get; set; }

        public M68kInstruction()
        { }

        public M68kInstruction(M68kOpcode opcode)
        {
            Opcode = opcode;
        }

        public M68kInstruction(M68kOpcode opcode, Register r1, Register r2)
        {
            Opcode = opcode;
            Register1 = r1;
            Register2 = r2;
            AddressingMode1 = M68kAddressingMode.Register;
            AddressingMode2 = M68kAddressingMode.Register;
        }

        public static M68kInstruction LoadFromMemory(Register addrReg, Register toreg)
        {
            return new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Register1 = addrReg,
                AddressingMode1 = M68kAddressingMode.Address,
                Register2 = toreg,
                AddressingMode2 = M68kAddressingMode.Register
            };
        }

        public static M68kInstruction StoreToMemory(Register sourceReg, Register addrReg)
        {
            return new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Register1 = sourceReg,
                AddressingMode1 = M68kAddressingMode.Register,
                Register2 = addrReg,
                AddressingMode2 = M68kAddressingMode.Address
            };
        }

        public M68kInstruction(M68kOpcode opcode, int immediate, Register r2)
        {
            Opcode = opcode;
            Register2 = r2;
            Immediate = immediate;
            AddressingMode1 = M68kAddressingMode.Immediate;
            AddressingMode2 = M68kAddressingMode.Register;
        }

        // @\01??_C@_09NKIIDDPL@argc?3?5?$CFd?6?$AA@
        public static string ConvertLabel(string label)
        {
            return label.Replace(".", "$").Replace("%", "").Replace("@", "_").Replace("?", "$").Replace("\\", "_");
        }

        int NumOperands()
        {
            switch (Opcode)
            {
                case M68kOpcode.Rts:
                case M68kOpcode.Jmp:
                case M68kOpcode.Beq:
                case M68kOpcode.Blt:
                case M68kOpcode.Bgt:
                case M68kOpcode.Bne:
                case M68kOpcode.Jsr:
                    return 0;
                default:
                    return 2;
            }
        }

        public bool IsBranch()
        {
            switch (Opcode)
            {
                case M68kOpcode.Beq:
                case M68kOpcode.Bgt:
                case M68kOpcode.Blt:
                case M68kOpcode.Bne:
                case M68kOpcode.Jmp:
                case M68kOpcode.Jsr:
                    return true;
            }
            return false;
        }

        public IEnumerable<string> Use
        {
            get
            {
                if (Register1 != null)
                {
                    yield return "D" + Register1.Number;
                }

                if ((Opcode == M68kOpcode.Add || Opcode == M68kOpcode.Sub) && Register2 != null)
                {
                    yield return "D" + Register2.Number;
                }

                if (Opcode == M68kOpcode.Rts)
                {
                    yield return "D0";
                }
            }
        }

        /*public IEnumerable<string> Def
        {
            get
            {
                if (Register2 != null)
                {
                    yield return "D" + Register2.Number;
                }
            }
        }*/

        string Size()
        {
            if (Width.HasValue)
            {
                return "." + Width.ToString().Substring(0, 1);
            }

            if (IsBranch())
            {
                return "";
            }

            switch (Opcode)
            {
                case M68kOpcode.MoveQ:
                case M68kOpcode.Rts:
                    return "";
                case M68kOpcode.Divs:
                    return ".W";
                default:
                    return ".L";
            }
        }

        public string Reg1ToString
        {
            get
            {
                if (FinalRegister1 != null)
                    return FinalRegister1.ToString();
                return Register1 + "_";
            }
        }

        public string Reg2ToString
        {
            get
            {
                if (FinalRegister2 != null)
                    return FinalRegister2.ToString();
                return Register2 + "_";
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Label != null)
            {
                sb.Append(ConvertLabel(Label) + ":\n");
            }

            if (Opcode == M68kOpcode.Label)
            {
                return sb.ToString();
            }

            sb.Append("    ");
            sb.Append(Opcode.ToString().ToLower() + Size().ToLower() + " ");

            if (NumOperands() > 0)
            {
                switch (AddressingMode1)
                {
                    case M68kAddressingMode.Address:
                        sb.Append("(" + Reg1ToString + ")");
                        break;
                    case M68kAddressingMode.AddressWithOffset:
                        sb.Append("" + Offset.Value + "(" + Reg1ToString + ")");
                        break;
                    case M68kAddressingMode.AddressWithPreIncrement:
                        sb.Append("+(" + Reg1ToString + ")");
                        break;
                    case M68kAddressingMode.AddressWithPreDecrement:
                        sb.Append("-(" + Reg1ToString + ")");
                        break;
                    case M68kAddressingMode.Register:
                        sb.Append(Reg1ToString);
                        break;
                    case M68kAddressingMode.Immediate:
                        sb.Append("#" + Immediate);
                        break;
                    case M68kAddressingMode.Absolute:
                        sb.Append(ConvertLabel(Variable));
                        break;
                }

                if (NumOperands() > 1)
                {
                    sb.Append(",");

                    switch (AddressingMode2)
                    {
                        case M68kAddressingMode.Address:
                            sb.Append("(" + Reg2ToString + ")");
                            break;
                        case M68kAddressingMode.AddressWithOffset:
                            sb.Append("" + Offset.Value + "(" + Reg2ToString + ")");
                            break;
                        case M68kAddressingMode.AddressWithPreIncrement:
                            sb.Append("+(" + Reg2ToString + ")");
                            break;
                        case M68kAddressingMode.AddressWithPreDecrement:
                            sb.Append("-(" + Reg2ToString + ")");
                            break;
                        case M68kAddressingMode.Register:
                            sb.Append(Reg2ToString);
                            break;
                        case M68kAddressingMode.Immediate:
                            sb.Append("#" + Immediate);
                            break;
                    }
                }
            }
            else
            {
                if (Opcode == M68kOpcode.Jmp || Opcode == M68kOpcode.Beq || Opcode == M68kOpcode.Jsr || Opcode == M68kOpcode.Bgt
                    || Opcode == M68kOpcode.Blt || Opcode == M68kOpcode.Bne)
                {
                    sb.Append(ConvertLabel(TargetLabel));
                }
            }

            if (Comment != null)
            {
                sb.Append(" ; ");
                sb.Append(Comment);
            }

            return sb.ToString();
        }
    }
}