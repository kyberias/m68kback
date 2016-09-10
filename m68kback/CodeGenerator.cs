using System;
using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class CodeGenerator : IVisitor
    {
        public object Visit(Program el)
        {
            foreach (var d in el.Declarations)
            {
                d.Visit(this);
            }

            foreach (var f in el.Functions)
            {
                f.Visit(this);
            }
            return null;
        }

        public List<M68kInstruction> Instructions { get; set; } = new List<M68kInstruction>();

        void Emit(M68kInstruction i)
        {
            Instructions.Add(i);
        }

        private Dictionary<string, int> frame;
        private Dictionary<string, TypeDeclaration> vars;
        private int frameOffset;

        //public List<string> Functions { get; set; } = new List<string>();

        public Dictionary<string,List<M68kInstruction>> Functions { get; set; } = new Dictionary<string, List<M68kInstruction>>();

        // Whether variable was generated as a result of GetElementPtr or Load expression
        // In that case the frame contains the pointer
        private Dictionary<string, bool> frameStored;

        private int functionIx;

        public object Visit(FunctionDefinition el)
        {
            Instructions = new List<M68kInstruction>();
            Functions[el.Name] = Instructions;

            Emit(new M68kInstruction { Label = el.Name });

            frame = new Dictionary<string, int>();
            frameStored = new Dictionary<string, bool>();
            vars = new Dictionary<string, TypeDeclaration>();
            frameOffset = 0;

            foreach (var s in el.Statements.Where(s => s is VariableAssignmentStatement))
            {
                var va = (VariableAssignmentStatement) s;

                frame[va.Variable] = frameOffset;

                frameStored[va.Variable] = va.Expr is GetElementPtr || va.Expr is LoadExpression;

                if (va.Expr.Type.IsPointer)
                {
                    frameOffset += 4;
                }
                else 
                {
                    switch (va.Expr.Type.Type)
                    {
                        case Token.I8:
                        case Token.I32:
                            frameOffset += 4* (va.Expr.Type.IsArray ? va.Expr.Type.ArrayX : 1);
                            break;
                        /*case Token.I8:
                            frameOffset += 1 * (va.Expr.Type.IsArray ? va.Expr.Type.ArrayX : 1);
                            break;*/
                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            int parIx = 1;
            foreach (var parameter in el.Parameters)
            {
                Register parReg;
                if (parameter.Type.IsPointer)
                {
                    parReg = NewAddressReg();
                }
                else
                {
                    parReg = NewDataReg();
                }

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    FinalRegister1 = M68kRegister.SP,
                    Offset = parIx * 4,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = parReg,
                });

                varRegs[parameter.Name] = parReg;

                frame[parameter.Name] = parIx*4 + frameOffset;
                parIx++;

                vars[parameter.Name] = parameter.Type;
            }

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Sub,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = frameOffset,
                AddressingMode2 = M68kAddressingMode.Register,
                FinalRegister2 = M68kRegister.SP,
            });

            foreach (var s in el.Statements)
            {
                if (s.Label != null)
                {
                    Emit(new M68kInstruction { Label = s.Label + functionIx });
                }
                s.Visit(this);
            }
            functionIx++;

            regD = 0;
            regA = 0;
            varRegs.Clear();

            return null;
        }

        public object Visit(AllocaExpression allocaExpression)
        {
            //throw new NotImplementedException();
            return NewDataReg();
        }

        public object Visit(CallExpression callExpression)
        {
            for (int i=callExpression.Parameters.Count-1; i >= 0; i--)
            {
                var parExpr = callExpression.Parameters[i];
                var parReg = (Register)parExpr.Visit(this);

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = parReg,
                    AddressingMode2 = M68kAddressingMode.AddressWithPreDecrement,
                    FinalRegister2 = M68kRegister.SP
                });
                frameTune += 4;
            }

            // TODO: Save registers

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jsr,
                TargetLabel = callExpression.FunctionName
            });

            var resultReg = NewDataReg();
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                FinalRegister1 = M68kRegister.D0,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resultReg
            });

            // TODO: Restore registers

            // Fix frame
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Adda,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = callExpression.Parameters.Count * 4,
                AddressingMode2 = M68kAddressingMode.Register,
                FinalRegister2 = M68kRegister.SP
            });
            frameTune = 0;

            return resultReg;
        }

        private int frameTune;

        int FrameIx(string name)
        {
            return frame[name] + frameTune;
        }

        public object Visit(ArithmeticExpression arithmeticExpression)
        {
            var resultReg = NewDataReg();

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = GetVarRegister(((VariableReference)arithmeticExpression.Operand1).Variable),
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resultReg
            });

            switch (arithmeticExpression.Operator)
            {
                case Token.Add:
                case Token.Sub:
                    if (arithmeticExpression.Operand2 is IntegerConstant)
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = arithmeticExpression.Operator == Token.Add ? M68kOpcode.Add : M68kOpcode.Sub,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ((IntegerConstant) arithmeticExpression.Operand2).Constant,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                        });
                    }
                    else
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = arithmeticExpression.Operator == Token.Add ? M68kOpcode.Add : M68kOpcode.Sub,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = GetVarRegister(((VariableReference)arithmeticExpression.Operand2).Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                        });
                    }

                    break;
                case Token.Srem:
                    {
                    /*Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                        Offset = FrameIx(((VariableReference)arithmeticExpression.Operand2).Variable),
                        Register1 = M68kRegister.SP,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.D1
                    });*/
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Divs,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = GetVarRegister(((VariableReference) arithmeticExpression.Operand2).Variable),
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = resultReg
                    });
                    var tempReg = NewDataReg();
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.MoveQ,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = 16,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = tempReg
                    });
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Lsr,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = tempReg,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = resultReg
                    });
                    }
                    break;
                default:
                    throw new NotSupportedException(arithmeticExpression.Operator.ToString());
            }
            return resultReg;
        }

        private int regD = 0;
        private int regA = 0;
        private int regC = 0;

        Register NewDataReg()
        {
            return new Register
            {
                Type = RegType.Data,
                Number = regD++
            };
        }

        Register NewAddressReg()
        {
            return new Register
            {
                Type = RegType.Address,
                Number = regA++
            };
        }

        Register NewConditionReg()
        {
            return new Register
            {
                Type = RegType.ConditionCode,
                Number = regC++
            };
        }

        Dictionary<string,Register> varRegs = new Dictionary<string, Register>();

        Register GetVarRegister(string varname)
        {
            return varRegs[varname];
        }

        public object Visit(IntegerConstant integerConstant)
        {
            var reg = NewDataReg();
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.MoveQ,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = integerConstant.Constant,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = reg
            });
            return reg;
        }

        public class Reloc
        {
            public int Index { get; set; }
            public Declaration Declaration { get; set; }
        }

        public List<Reloc> Relocs { get; set; } = new List<Reloc>();

        public object Visit(GetElementPtr getElementPtr)
        {
            if (getElementPtr.PtrVar[0] == '%')
            {
                if (getElementPtr.PtrType.IsArray)
                {
                    throw new NotImplementedException();
                    /*Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = M68kRegister.SP,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.A0,
                        Comment = "GetElementPtr (array)"
                    });

                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Adda,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = FrameIx(getElementPtr.PtrVar),
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.A0
                    });


                    var indexExpr = getElementPtr.Indices[0];

                    if (indexExpr is IntegerConstant)
                    {
                        var intconst = (IntegerConstant) indexExpr;
                        var ix = getElementPtr.PtrType.IsPointer ? intconst.Constant*4 : intconst.Constant;

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ix,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = M68kRegister.A0
                        });
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }*/
                }
                else
                {
                    var varReg = GetVarRegister(getElementPtr.PtrVar);

                    var indexExpr = getElementPtr.Indices[0];
                    var indexInteger = (indexExpr as IntegerConstant);

                    if (indexInteger != null && indexInteger.Constant == 0)
                    {
                        return varReg;
                    }

                    var newReg = NewAddressReg();
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = varReg,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = newReg
                    });

                    if (indexInteger != null)
                    {
                        var intconst = indexInteger;
                        var ix = getElementPtr.PtrType.IsPointer ? intconst.Constant*4 : intconst.Constant;

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ix,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = varReg
                        });
                    }
                    else if (indexExpr is VariableReference)
                    {
                        var varref = (VariableReference) indexExpr;
//                    var ix = getElementPtr.PtrType.IsPointer ? intconst.Constant * 4 : intconst.Constant;
                        var varreg = GetVarRegister(varref.Variable);

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = varreg,
                            Offset = FrameIx(varref.Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = newReg
                        });

                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    return newReg;
                }
            }
            else
            {
                var newReg = NewAddressReg();

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Lea,
                    AddressingMode1 = M68kAddressingMode.Absolute,
                    Variable = getElementPtr.PtrVar,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = newReg
                });
                Relocs.Add(new Reloc
                {
                    Index = Instructions.Count-1,
                    Declaration = Globals[getElementPtr.PtrVar]
                });
                return newReg;
            }

            /*Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = M68kRegister.A0,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = M68kRegister.D0
            });*/
        }

        public object Visit(VariableReference variableReference)
        {
            /*variableReference.Type = vars[variableReference.Variable];

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                Register1 = M68kRegister.SP,
                Offset = FrameIx(variableReference.Variable),
                AddressingMode2 = M68kAddressingMode.Register,
                Width = TypeToWidth(variableReference.Type),
                Register2 = M68kRegister.D0,
                Comment = "Variable reference"
            });*/
            return GetVarRegister(variableReference.Variable);
        }

        public object Visit(ExpressionStatement expressionStatement)
        {
            return expressionStatement.Expression.Visit(this);
        }

        public object Visit(TypeDeclaration typeDeclaration)
        {
            throw new NotImplementedException();
        }

        public object Visit(RetStatement retStatement)
        {
            if (retStatement.Value != null)
            {
                retStatement.Value.Visit(this);
            }

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Add,
                Immediate = frameOffset,
                AddressingMode1 = M68kAddressingMode.Immediate,
                AddressingMode2 = M68kAddressingMode.Register,
                FinalRegister2 = M68kRegister.SP
            });

            Emit(new M68kInstruction { Opcode = M68kOpcode.Rts});
            return null;
        }

        //private Token? statusReg;

        public object Visit(IcmpExpression icmpExpression)
        {
            //statusReg = icmpExpression.Condition;

            if (icmpExpression.Value is IntegerConstant)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Cmp,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.Immediate,
                    Immediate = (icmpExpression.Value as IntegerConstant).Constant,
                    Register2 = GetVarRegister(icmpExpression.Var),
                    AddressingMode2 = M68kAddressingMode.Register
                });
            }
            else if (icmpExpression.Value is VariableReference)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Cmp,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = GetVarRegister((icmpExpression.Value as VariableReference).Variable),
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = GetVarRegister(icmpExpression.Var),
                });
            }
            else
            {
                throw new NotSupportedException();
            }


            var r = NewConditionReg();
            r.Condition = icmpExpression.Condition;

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Width = TypeToWidth(icmpExpression.Type),
                AddressingMode1 = M68kAddressingMode.Register,
                FinalRegister1 = M68kRegister.CCR,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = r,
            });

            return r;
        }

        public object Visit(LabelBrStatement labelBrStatement)
        {
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                TargetLabel = labelBrStatement.TargetLabel + functionIx
            });
            return null;
        }

        public object Visit(ConditionalBrStatement conditionalBrStatement)
        {
            var reg = GetVarRegister(conditionalBrStatement.Identifier);

            M68kOpcode opc;
            switch (reg.Condition)
            {
                case Token.Eq:
                    opc = M68kOpcode.Beq;
                    break;
                case Token.Ne:
                    opc = M68kOpcode.Bne;
                    break;
                case Token.Slt:
                    opc = M68kOpcode.Blt;
                    break;
                case Token.Sgt:
                    opc = M68kOpcode.Bgt;
                    break;
                default:
                    throw new NotSupportedException(reg.Condition.ToString());
            }

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Register1 = reg,
                AddressingMode1 = M68kAddressingMode.Register,
                AddressingMode2 = M68kAddressingMode.Register,
                FinalRegister2 = M68kRegister.CCR
            });

            Emit(new M68kInstruction
            {
                Opcode = opc,
                TargetLabel = conditionalBrStatement.Label1 + functionIx
            });
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                TargetLabel = conditionalBrStatement.Label2 + functionIx
            });
            return null;
        }

        M68Width TypeToWidth(TypeDeclaration type)
        {
            if (type.IsPointer)
            {
                return M68Width.Long;
            }

            switch (type.Type)
            {
                case Token.I32:
                    return M68Width.Long;
                case Token.I8:
                    return M68Width.Byte;
                default:
                    throw new NotSupportedException();
            }
        }

        public object Visit(LoadExpression loadExpression)
        {
            var reg = NewDataReg();

            var ptrReg = (Register)loadExpression.Value.Visit(this);

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = ptrReg,
                AddressingMode2 = M68kAddressingMode.Register,
                Width = TypeToWidth(loadExpression.Type),
                Register2 = reg
            });

            return reg;
        }

        public object Visit(StoreStatement storeStatement)
        {
            var reg = (Register)storeStatement.Value.Visit(this);

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = reg,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = GetVarRegister(storeStatement.Variable),
                Comment = "Store"
            });

            return null;
        }

        public object Visit(VariableAssignmentStatement variableAssignmentStatement)
        {
            var valReg = (Register)variableAssignmentStatement.Expr.Visit(this);
            varRegs[variableAssignmentStatement.Variable] = valReg;
            return valReg;
            /*var newReg = NewDataReg();
            var valReg = (Register)variableAssignmentStatement.Expr.Visit(this);
            // TODO: Couldn't we just use the valReg from the statement?
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Width = TypeToWidth(variableAssignmentStatement.Expr.Type),
                Register1 = valReg,
                AddressingMode1 = M68kAddressingMode.Register,
                Register2 = newReg,
                AddressingMode2 = M68kAddressingMode.Register,
            });

            varRegs[variableAssignmentStatement.Variable] = newReg;

            return newReg;*/
        }

        public Dictionary<string,Declaration> Globals = new Dictionary<string, Declaration>();

        public object Visit(Declaration declaration)
        {
            Globals.Add(declaration.Name, declaration);
            return null;
        }
    }
}