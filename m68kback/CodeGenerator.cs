using System;
using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class CodeGenerator : IVisitor
    {
        public void Visit(Program el)
        {
            foreach (var d in el.Declarations)
            {
                d.Visit(this);
            }

            foreach (var f in el.Functions)
            {
                f.Visit(this);
            }
        }

        public List<M68kInstruction> Instructions { get; set; } = new List<M68kInstruction>();

        void Emit(M68kInstruction i)
        {
            Instructions.Add(i);
        }

        private Dictionary<string, int> frame;
        private Dictionary<string, TypeDeclaration> vars;
        private int frameOffset;

        public List<string> Functions { get; set; } = new List<string>();

        // Whether variable was generated as a result of GetElementPtr or Load expression
        // In that case the frame contains the pointer
        private Dictionary<string, bool> frameStored;

        private int functionIx;

        public void Visit(FunctionDefinition el)
        {
            Functions.Add(el.Name);
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
                Register2 = M68kRegister.SP,
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
        }

        public void Visit(AllocaExpression allocaExpression)
        {
            //throw new NotImplementedException();
        }

        public void Visit(CallExpression callExpression)
        {
            for (int i=callExpression.Parameters.Count-1; i >= 0; i--)
            {
                var parExpr = callExpression.Parameters[i];
                parExpr.Visit(this);
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = M68kRegister.D0,
                    AddressingMode2 = M68kAddressingMode.AddressWithPreDecrement,
                    Register2 = M68kRegister.SP
                });
                frameTune += 4;
            }

            // TODO: Save registers

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jsr,
                TargetLabel = callExpression.FunctionName
            });

            // TODO: Restore registers

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Adda,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = callExpression.Parameters.Count * 4,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = M68kRegister.SP
            });
            frameTune = 0;
        }

        private int frameTune;

        int FrameIx(string name)
        {
            return frame[name] + frameTune;
        }

        public void Visit(ArithmeticExpression arithmeticExpression)
        {
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                Register1 = M68kRegister.SP,
                Offset = FrameIx(((VariableReference)arithmeticExpression.Operand1).Variable),
                Register2 = M68kRegister.D0,
                AddressingMode2 = M68kAddressingMode.Register
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
                            Register2 = M68kRegister.D0,
                        });
                    }
                    else
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = arithmeticExpression.Operator == Token.Add ? M68kOpcode.Add : M68kOpcode.Sub,
                            AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                            Register1 = M68kRegister.SP,
                            Offset = FrameIx(((VariableReference)arithmeticExpression.Operand2).Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = M68kRegister.D0,
                        });
                    }

                    break;
                case Token.Srem:
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                        Offset = FrameIx(((VariableReference)arithmeticExpression.Operand2).Variable),
                        Register1 = M68kRegister.SP,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.D1
                    });
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Divs,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = M68kRegister.D1,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.D0
                    });
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.MoveQ,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = 16,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.D1
                    });
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Lsr,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = M68kRegister.D1,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = M68kRegister.D0
                    });
                    break;
                default:
                    throw new NotSupportedException(arithmeticExpression.Operator.ToString());
            }
        }

        public void Visit(IntegerConstant integerConstant)
        {
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.MoveQ,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = integerConstant.Constant,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = M68kRegister.D0
            });
        }

        public class Reloc
        {
            public int Index { get; set; }
            public Declaration Declaration { get; set; }
        }

        public List<Reloc> Relocs { get; set; } = new List<Reloc>();

        public void Visit(GetElementPtr getElementPtr)
        {
            if (getElementPtr.PtrVar[0] == '%')
            {
                if (getElementPtr.PtrType.IsArray)
                {
                    Emit(new M68kInstruction
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
                    }
                }
                else
                {

                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                        Offset = FrameIx(getElementPtr.PtrVar),
                        Register1 = M68kRegister.SP,
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
                    else if (indexExpr is VariableReference)
                    {
                        var varref = (VariableReference) indexExpr;
//                    var ix = getElementPtr.PtrType.IsPointer ? intconst.Constant * 4 : intconst.Constant;

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                            Register1 = M68kRegister.SP,
                            Offset = FrameIx(varref.Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = M68kRegister.A0
                        });

                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            else
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Lea,
                    AddressingMode1 = M68kAddressingMode.Absolute,
                    Variable = getElementPtr.PtrVar,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.A0
                });
                Relocs.Add(new Reloc
                {
                    Index = Instructions.Count-1,
                    Declaration = Globals[getElementPtr.PtrVar]
                });
            }

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = M68kRegister.A0,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = M68kRegister.D0
            });
        }

        public void Visit(VariableReference variableReference)
        {
            variableReference.Type = vars[variableReference.Variable];

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
            });
        }

        public void Visit(ExpressionStatement expressionStatement)
        {
            expressionStatement.Expression.Visit(this);
        }

        public void Visit(TypeDeclaration typeDeclaration)
        {
            throw new NotImplementedException();
        }

        public void Visit(RetStatement retStatement)
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
                Register2 = M68kRegister.SP
            });

            Emit(new M68kInstruction { Opcode = M68kOpcode.Rts});
        }

        private Token? statusReg;

        public void Visit(IcmpExpression icmpExpression)
        {
            statusReg = icmpExpression.Condition;

            if (icmpExpression.Value is IntegerConstant)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    Width = TypeToWidth(icmpExpression.Type),
                    Register1 = M68kRegister.SP,
                    Offset = FrameIx(icmpExpression.Var),
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.D0
                });
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Cmp,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.Immediate,
                    Immediate = (icmpExpression.Value as IntegerConstant).Constant,
                    Register2 = M68kRegister.D0,
                    AddressingMode2 = M68kAddressingMode.Register
                });
            }
            else if (icmpExpression.Value is VariableReference)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Register1 = M68kRegister.SP,
                    Offset = FrameIx(icmpExpression.Var),
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.D0
                });
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Cmp,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Register1 = M68kRegister.SP,
                    Offset = FrameIx((icmpExpression.Value as VariableReference).Variable),
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.D0,
                });
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Visit(LabelBrStatement labelBrStatement)
        {
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                TargetLabel = labelBrStatement.TargetLabel + functionIx
            });
        }

        public void Visit(ConditionalBrStatement conditionalBrStatement)
        {
            M68kOpcode opc;
            switch (statusReg.Value)
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
                    throw new NotSupportedException(statusReg.Value.ToString());
            }

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

        public void Visit(LoadExpression loadExpression)
        {
            if (frameStored[(loadExpression.Value as VariableReference).Variable])
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.MoveA,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Register1 = M68kRegister.SP,
                    Offset = FrameIx((loadExpression.Value as VariableReference).Variable),
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.A0
                });
                Emit(new M68kInstruction
                {
                    Width = TypeToWidth(loadExpression.Type),
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Address,
                    Register1 = M68kRegister.A0,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.D0
                });
            }
            else
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    Width = TypeToWidth(loadExpression.Type),
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Register1 = M68kRegister.SP,
                    Offset = FrameIx((loadExpression.Value as VariableReference).Variable),
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.D0
                });
            }
        }

        public void Visit(StoreStatement storeStatement)
        {
            storeStatement.Value.Visit(this);

            if (frameStored[storeStatement.Variable])
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Register1 = M68kRegister.SP,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = M68kRegister.A0,
                    Offset = FrameIx(storeStatement.Variable),
                    Comment = "Store"
                });

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = M68kRegister.D0,
                    Width = TypeToWidth(storeStatement.ExprType),
                    AddressingMode2 = M68kAddressingMode.Address,
                    Register2 = M68kRegister.A0,
                    Comment = "Store"
                });
            }
            else
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = M68kRegister.D0,
                    Width = TypeToWidth(storeStatement.ExprType),
                    AddressingMode2 = M68kAddressingMode.AddressWithOffset,
                    Register2 = M68kRegister.SP,
                    Offset = FrameIx(storeStatement.Variable),
                });
            }
        }

        public void Visit(VariableAssignmentStatement variableAssignmentStatement)
        {
            vars[variableAssignmentStatement.Variable] = variableAssignmentStatement.Expr.Type;

            variableAssignmentStatement.Expr.Visit(this);

            if (variableAssignmentStatement.Expr is IcmpExpression || variableAssignmentStatement.Expr is AllocaExpression)
            {
                // TODO: This does not need a variable register
                return;
            }

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Width = TypeToWidth(variableAssignmentStatement.Expr.Type),
                Register1 = M68kRegister.D0,
                AddressingMode1 = M68kAddressingMode.Register,
                Register2 = M68kRegister.SP,
                AddressingMode2 = M68kAddressingMode.AddressWithOffset,
                Offset = FrameIx(variableAssignmentStatement.Variable)
            });
        }

        public Dictionary<string,Declaration> Globals = new Dictionary<string, Declaration>();

        public void Visit(Declaration declaration)
        {
            Globals.Add(declaration.Name, declaration);
        }
    }
}