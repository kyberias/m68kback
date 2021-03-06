using System;
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
        Pea,
        Rts,
        Jmp,
        Jsr,
        Bgt,
        Bge,
        Beq,
        Bne,
        Blt,
        Tst,
        Adda,
        Add,
        Sub,
        Cmp,
        Divs,
        Muls,
        Lsr,
        Lsl,
        Asr,
        Asl,
        Eor,
        And,
        Or,
        RegDef,
        RegUse
    }

    public enum M68kAddressingMode
    {
        Register,
        //AddressRegister,
        Address, // (An)
        AddressWithPostIncrement, // (An)+
        AddressWithPostDecrement, // (An)-
        AddressWithPreIncrement, // +(An)
        AddressWithPreDecrement, // -(An)
        AddressWithOffset, // #x(An)
        ProgramCounterIndirectWithIndex,
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
        SP,
        PC,
        CCR,
        CCR0,
        CCR1,
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

        public M68kRegister ConvertToPhysicalRegister()
        {
            if (Type == RegType.Data && Number >= 0 && Number <= 7)
            {
                return M68kRegister.D0 + Number;
            }
            if (Type == RegType.Address && Number >= 0 && Number <= 7)
            {
                return M68kRegister.A0 + Number;
            }
            throw new NotSupportedException();
        }

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
        public bool IgnoreLabelForPhi { get; set; }
        public M68kOpcode Opcode { get; set; }
        public M68Width? Width { get; set; }

        private bool? isTerminating;
        public bool IsTerminating
        {
            get { return isTerminating.HasValue ? isTerminating.Value : Opcode == M68kOpcode.Rts; }
            set { isTerminating = value; }
        }

        public int WidthInBytes
        {
            get
            {
                switch (Width)
                {
                    case M68Width.Byte:
                        return 1;
                    case M68Width.Word:
                        return 2;
                    case M68Width.Long:
                        return 4;
                    default:
                        return 4;
                }
            }
        }

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
        public IList<string> DefsUses { get; set; }

        /// <summary>
        /// New Label generated by Phi instruction
        /// </summary>
        public bool LabelFromPhi { get; set; }

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

        public M68kInstruction(M68kOpcode opcode, Register r1, M68kRegister r2)
        {
            Opcode = opcode;
            Register1 = r1;
            FinalRegister2 = r2;
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
                case M68kOpcode.Beq:
                case M68kOpcode.Blt:
                case M68kOpcode.Bgt:
                case M68kOpcode.Bge:
                case M68kOpcode.Bne:
                    return 0;
                case M68kOpcode.Jmp:
                case M68kOpcode.Jsr:
                    return Register1 != null ? 1 : 0;
                case M68kOpcode.Tst:
                case M68kOpcode.Pea:
                    return 1;
                default:
                    return 2;
            }
        }

        public bool IsJump()
        {
            switch (Opcode)
            {
                case M68kOpcode.Jmp:
                    return true;
            }
            return false;
        }
        public bool IsConditionalBranch()
        {
            switch (Opcode)
            {
                case M68kOpcode.Beq:
                case M68kOpcode.Bgt:
                case M68kOpcode.Bge:
                case M68kOpcode.Blt:
                case M68kOpcode.Bne:
                    return true;
            }
            return false;
        }

        public bool IsBranch()
        {
            return IsJump() || IsConditionalBranch();
        }

        public IEnumerable<string> Use(RegType regType)
        {
            if (Opcode == M68kOpcode.RegUse)
            {
                foreach (var du in DefsUses)
                {
                    yield return du;
                }
            }

            if (Opcode == M68kOpcode.Cmp || Opcode == M68kOpcode.Lsr)
            {
                if (Register1 != null)
                {
                    yield return Register1.ToString();
                }
                yield return Register2.ToString();
                yield break;
            }

            if (Register1 != null && Register1.Type == regType)
            {
                yield return Register1.ToString();
            }

            if ((Opcode == M68kOpcode.Add || Opcode == M68kOpcode.Sub || Opcode == M68kOpcode.Adda) && Register2 != null && Register2.Type == regType)
            {
                yield return Register2.ToString();
            }

            if (Register2 != null && Register2.Type == regType)
            {
                switch (AddressingMode2)
                {
                    case M68kAddressingMode.Address:
                    case M68kAddressingMode.AddressWithOffset:
                    case M68kAddressingMode.AddressWithPostIncrement:
                    case M68kAddressingMode.AddressWithPostDecrement:
                    case M68kAddressingMode.AddressWithPreDecrement:
                    case M68kAddressingMode.AddressWithPreIncrement:
                        yield return Register2.ToString();
                        break;
                }
            }

            if (Opcode == M68kOpcode.Rts)
            {
                if (regType == RegType.Data)
                {
                    if (FinalRegister1.HasValue)
                    {
                        yield return "D0";
                    }
                    yield return "D2";
                    yield return "D3";
                    yield return "D4";
                    yield return "D5";
                    yield return "D6";
                    yield return "D7";
                }
                if (regType == RegType.Address)
                {
                    yield return "A2";
                    yield return "A3";
                    yield return "A4";
                    yield return "A5";
                    yield return "A6";
                }
            }
        }

        public IEnumerable<string> Def(RegType regType)
        {
            // MOVE D2,D0   Def: D0
            // MOVE (A2)+,D0   Def: D0, A2
            // MOVE (A2)+,A1   Def: A2
            // MOVE (A2)+,(A1)-   Def: A2,A1

            if (Opcode == M68kOpcode.Jsr)
            {
                if (regType == RegType.Data)
                {
                    yield return "D0";
                    yield return "D1";
                }
                if (regType == RegType.Address)
                {
                    yield return "A0";
                    yield return "A1";
                }
            }

            if (Opcode == M68kOpcode.RegDef)
            {
                foreach (var du in DefsUses)
                {
                    if ((du[0] == 'D' && regType == RegType.Data) || (du[0] == 'A' && regType == RegType.Address))
                    {
                        yield return du;
                    }
                }
            }

            if (Opcode == M68kOpcode.Cmp)
            {
                yield break;
            }

            if (Register2 != null && AddressingMode2 == M68kAddressingMode.Register && Register2.Type == regType)
            {
                yield return Register2.ToString();
            }

            if (Register1 != null && Register1.Type == regType)
            {
                switch (AddressingMode1)
                {
                    case M68kAddressingMode.AddressWithPostIncrement:
                    case M68kAddressingMode.AddressWithPostDecrement:
                    case M68kAddressingMode.AddressWithPreDecrement:
                    case M68kAddressingMode.AddressWithPreIncrement:
                        yield return Register1.ToString();
                        break;
                }
            }

            if (Register2 != null && Register2.Type == regType)
            {
                switch (AddressingMode2)
                {
                    case M68kAddressingMode.AddressWithPostIncrement:
                    case M68kAddressingMode.AddressWithPostDecrement:
                    case M68kAddressingMode.AddressWithPreDecrement:
                    case M68kAddressingMode.AddressWithPreIncrement:
                        yield return Register2.ToString();
                        break;
                }
            }
        }

        string Size()
        {
            if (IsBranch() || Opcode == M68kOpcode.Jsr)
            {
                return "";
            }

            if (Width.HasValue)
            {
                return "." + Width.ToString().Substring(0, 1);
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
                if (!string.IsNullOrEmpty(TargetLabel))
                {
                    sb.Append(ConvertLabel(TargetLabel));
                }
                else
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
            }
            else
            {
                if (Opcode == M68kOpcode.Jmp || Opcode == M68kOpcode.Beq || Opcode == M68kOpcode.Jsr || Opcode == M68kOpcode.Bgt
                    || Opcode == M68kOpcode.Blt || Opcode == M68kOpcode.Bne || Opcode == M68kOpcode.Bge)
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