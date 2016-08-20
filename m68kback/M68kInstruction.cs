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
    }

    public enum M68Width
    {
        Byte,
        Word,
        Long
    }

    public class M68kInstruction
    {
        public string Label { get; set; }
        public M68kOpcode Opcode { get; set; }
        public M68Width? Width { get; set; }
        public M68kRegister? Register1 { get; set; }
        public M68kRegister? Register2 { get; set; }
        public string RegOperand1 { get; set; }
        public string RegOperand2 { get; set; }
        public M68kAddressingMode AddressingMode1 { get; set; }
        public M68kAddressingMode AddressingMode2 { get; set; }
        public int? Immediate { get; set; }
        public int? Offset { get; set; }
        public string TargetLabel { get; set; }
        public string Variable { get; set; }
        public string Comment { get; set; }

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

        bool IsBranch()
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
                        sb.Append("(" + Register1 + ")");
                        break;
                    case M68kAddressingMode.AddressWithOffset:
                        sb.Append("" + Offset.Value + "(" + Register1 + ")");
                        break;
                    case M68kAddressingMode.AddressWithPreIncrement:
                        sb.Append("+(" + Register1 + ")");
                        break;
                    case M68kAddressingMode.AddressWithPreDecrement:
                        sb.Append("-(" + Register1 + ")");
                        break;
                    case M68kAddressingMode.Register:
                        sb.Append(Register1);
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
                            sb.Append("(" + Register2 + ")");
                            break;
                        case M68kAddressingMode.AddressWithOffset:
                            sb.Append("" + Offset.Value + "(" + Register2 + ")");
                            break;
                        case M68kAddressingMode.AddressWithPreIncrement:
                            sb.Append("+(" + Register2 + ")");
                            break;
                        case M68kAddressingMode.AddressWithPreDecrement:
                            sb.Append("-(" + Register2 + ")");
                            break;
                        case M68kAddressingMode.Register:
                            sb.Append(Register2);
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