using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;

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

        public Dictionary<string, FunctionDef> Functions { get; set; } = new Dictionary<string, FunctionDef>();

        // Whether variable was generated as a result of GetElementPtr or Load expression
        // In that case the frame contains the pointer
        private Dictionary<string, bool> frameStored;

        public class FunctionDef
        {
            public IList<M68kInstruction> Instructions { get; set; }
            // Temporary reg numbering should start after actual register numbers
            public int regD { get; set; } = 8;
            public int regA { get; set; } = 7;
            public int regC { get; set; } = 0;

            public int PrologueLen { get; set; }

            public Dictionary<string, int> VarsInStack { get; } = new Dictionary<string, int>();
            public int FrameSize { get; set; }

            public Register NewDataReg()
            {
                return new Register
                {
                    Type = RegType.Data,
                    Number = regD++
                };
            }

            public Register NewAddressReg()
            {
                return new Register
                {
                    Type = RegType.Address,
                    Number = regA++
                };
            }

            public Register NewConditionReg()
            {
                return new Register
                {
                    Type = RegType.ConditionCode,
                    Number = regC++
                };
            }
        }

        private static void RemoveRedundantMoves(IList<M68kInstruction> instructions)
        {
            var redundantMoves = instructions.Where(i =>
                i.Opcode == M68kOpcode.Move &&
                i.AddressingMode1 == M68kAddressingMode.Register &&
                i.AddressingMode2 == M68kAddressingMode.Register &&
                i.FinalRegister1 == i.FinalRegister2).ToList();

            foreach (var move in redundantMoves)
            {
                instructions.Remove(move);
            }
        }

        private static void RemoveRedundantCCR(IList<M68kInstruction> instructions)
        {
            // TODO: Should check this properly: use correct pattern to recognize the instructions:
            // - instruction that sets CCR (e.g. CMP)
            // - move from CCR to CCR0
            // - move from CCR0 to CCR
            // - conditional branch
            //
            // => Can remove the middle moves. Otherwise no.
            var redundantMoves = instructions.Where(i =>
                i.Opcode == M68kOpcode.Move &&
                i.AddressingMode1 == M68kAddressingMode.Register &&
                i.AddressingMode2 == M68kAddressingMode.Register &&
                (i.FinalRegister1 == M68kRegister.CCR || i.FinalRegister2 == M68kRegister.CCR)).ToList();

            foreach (var move in redundantMoves)
            {
                instructions.Remove(move);
            }
        }

        private static void RemoveInstructions(IList<M68kInstruction> instructions, params M68kOpcode[] opcodes)
        {
            var toRemove = instructions.Where(i => opcodes.Contains(i.Opcode)).ToList();

            foreach (var move in toRemove)
            {
                instructions.Remove(move);
            }
        }

        private FunctionDef currentFunction;

        private int functionIx;

        public object Visit(FunctionDefinition el)
        {
            Instructions = new List<M68kInstruction>();
            currentFunction = new FunctionDef
            {
                Instructions = Instructions
            };
            Functions[el.Name] = currentFunction;

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

            var parLoadsThatMustBeFixed = new List<M68kInstruction>();

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

                var i = new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    FinalRegister1 = M68kRegister.SP,
                    Offset = parIx*4, // TODO: These offsets are incorrect and must be fixed!
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = parReg,
                };
                Emit(i);
                parLoadsThatMustBeFixed.Add(i);

                currentFunction.PrologueLen++;

                varRegs[parameter.Name] = parReg;

                frame[parameter.Name] = parIx*4 + frameOffset;
                parIx++;

                vars[parameter.Name] = parameter.Type;
            }

            var subSPi = new M68kInstruction
            {
                Opcode = M68kOpcode.Sub,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = frameOffset, // TODO: Offset not really known at this time.
                AddressingMode2 = M68kAddressingMode.Register,
                FinalRegister2 = M68kRegister.SP,
            };

            Instructions.Insert(1, subSPi);

            currentFunction.PrologueLen++;

            var func = currentFunction;

            var calleeSavedTemporaries = new Dictionary<Register, Register>();

            // callee-saved: D2-D7
            foreach (var d in Enumerable.Range(2, 6).Select(i => new Register {Number = i, Type = RegType.Data}))
            {
                calleeSavedTemporaries[d] = func.NewDataReg();
            }
            // callee-saved: A2-A6
            foreach (var d in Enumerable.Range(2, 5).Select(i => new Register { Number = i, Type = RegType.Address }))
            {
                calleeSavedTemporaries[d] = func.NewAddressReg();
            }
            foreach (var cs in calleeSavedTemporaries)
            {
                func.Instructions.Insert(func.PrologueLen + 1, new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    Register1 = cs.Key,
                    //FinalRegister1 = cs.Key.ConvertToPhysicalRegister(),
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register2 = cs.Value,
                    AddressingMode2 = M68kAddressingMode.Register
                });
            }

            func.Instructions.Insert(1, new M68kInstruction
            {
                Opcode = M68kOpcode.RegDef,
                DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r)
                        .Union(Enumerable.Range(0,7).Select(r => "A" + r))
                        .ToList()
            });

            offsetsToFix.Clear();
            // Actually generate code
            foreach (var s in el.Statements)
            {
                if (s.Label != null)
                {
                    Emit(new M68kInstruction { Label = s.Label + functionIx });
                }
                s.Visit(this);
            }
            functionIx++;

            varRegs.Clear();

            // Restore all callee saved
            foreach (var cs in calleeSavedTemporaries)
            {
                func.Instructions.Insert(func.Instructions.Count - 2, new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    Register1 = cs.Value,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register2 = cs.Key,
                    //FinalRegister2 = cs.Key.ConvertToPhysicalRegister(),
                    AddressingMode2 = M68kAddressingMode.Register
                });
            }

            var gcD = new GraphColoring(func.Instructions, spillStart: frameOffset);
            gcD.Main();
            gcD.FinalRewrite();
            frameOffset += gcD.SpillCount * 4;

            var gcA = new GraphColoring(gcD.Instructions, 7, RegType.Address, frameOffset);
            gcA.Main();
            gcA.FinalRewrite(RegType.Address);
            frameOffset += gcA.SpillCount * 4;

            var gcC = new GraphColoring(gcA.Instructions, 2, RegType.ConditionCode, frameOffset);
            gcC.Main();
            gcC.FinalRewrite(RegType.ConditionCode);
            frameOffset += gcC.SpillCount * 4;

            RemoveRedundantMoves(gcC.Instructions);
            RemoveRedundantCCR(gcC.Instructions);
            RemoveInstructions(gcC.Instructions, M68kOpcode.RegDef, M68kOpcode.RegUse);

            func.Instructions = gcC.Instructions;

            subSPi.Immediate = frameOffset;
            foreach (var of in offsetsToFix)
            {
                of.Immediate = frameOffset;
            }
            foreach (var of in parLoadsThatMustBeFixed)
            {
                of.Offset += frameOffset;
            }

            return null;
        }

        public object Visit(AllocaExpression allocaExpression)
        {
            //throw new NotImplementedException();
            return null;
        }

        public object Visit(CallExpression callExpression)
        {
            for (int i=callExpression.Parameters.Count-1; i >= 0; i--)
            {
                var parExpr = callExpression.Parameters[i];
                var parReg = parExpr.Visit(this);

                if (parReg is Register)
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = (Register) parReg,
                        AddressingMode2 = M68kAddressingMode.AddressWithPreDecrement,
                        FinalRegister2 = M68kRegister.SP
                    });
                    frameTune += 4;
                }
                else
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                        FinalRegister1 = M68kRegister.SP,
                        Offset = (int)parReg + frameTune,
                        //Register1 = (Register)parReg,
                        AddressingMode2 = M68kAddressingMode.AddressWithPreDecrement,
                        FinalRegister2 = M68kRegister.SP
                    });

                }
            }

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
                //FinalRegister1 = M68kRegister.D0,
                Register1 = new Register { Number = 0, Type = RegType.Data},
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resultReg
            });

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

        public Register NewDataReg()
        {
            return currentFunction.NewDataReg();
        }

        public Register NewAddressReg()
        {
            return currentFunction.NewAddressReg();
        }

        public Register NewConditionReg()
        {
            return currentFunction.NewConditionReg();
        }

        Dictionary<string, Register> varRegs = new Dictionary<string, Register>();

        Register GetVarRegister(string varname)
        {
            if (varRegs.ContainsKey(varname))
            {
                return varRegs[varname];
            }
            return null;
        }

        object GetVarRegisterOrStackOffset(string varname)
        {
            var r = GetVarRegister(varname);
            if (r != null)
            {
                return r;
            }
            return currentFunction.VarsInStack[varname];
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
                    var arrPtrReg = GetVarRegisterOrStackOffset(getElementPtr.PtrVar);
                    var indexExpr = getElementPtr.Indices[0];
                    var indexInteger = (indexExpr as IntegerConstant);

                    if (indexInteger != null && indexInteger.Constant == 0)
                    {
                        return arrPtrReg;
                    }

                    throw new NotImplementedException();
                    /*var newTempReg = NewAddressReg();
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = arrPtrReg,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = newTempReg,
                        Comment = "GetElementPtr (array)"
                    });
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Add,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = getElementPtr.Indices[0].,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = newTempReg,
                        Comment = "GetElementPtr (array)"
                    });*/

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
                    var varReg = GetVarRegisterOrStackOffset(getElementPtr.PtrVar);

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
                        Register1 = (Register)varReg,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = newReg
                    });

                    if (indexInteger != null)
                    {
                        var intconst = indexInteger;
                        var ix = getElementPtr.PtrType.IsPointer ? intconst.Constant*4 : intconst.Constant;

                        if (newReg.Type == RegType.Data)
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Add,
                                AddressingMode1 = M68kAddressingMode.Immediate,
                                Immediate = ix,
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = newReg
                            });
                        }
                        else
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Adda,
                                AddressingMode1 = M68kAddressingMode.Immediate,
                                Immediate = ix,
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = newReg
                            });
                        }
                    }
                    else if (indexExpr is VariableReference)
                    {
                        var varref = (VariableReference) indexExpr;
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

            if (currentFunction.VarsInStack.ContainsKey(variableReference.Variable))
            {
                return currentFunction.VarsInStack[variableReference.Variable];
            }

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

        IList<M68kInstruction> offsetsToFix = new List<M68kInstruction>();

        public object Visit(RetStatement retStatement)
        {
            if (retStatement.Value != null)
            {
                var reg = (Register)retStatement.Value.Visit(this);
                if (reg != null)
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = reg,
                        AddressingMode2 = M68kAddressingMode.Register,
                        //FinalRegister2 = M68kRegister.D0
                        Register2 = new Register { Number = 0, Type = RegType.Data },
                    });
                }
            }

            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.Add,
                Immediate = frameOffset,
                AddressingMode1 = M68kAddressingMode.Immediate,
                AddressingMode2 = M68kAddressingMode.Register,
                FinalRegister2 = M68kRegister.SP
            };
            Emit(i);
            offsetsToFix.Add(i);

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
            //var ptrReg = (Register)loadExpression.Value.Visit(this);
            var offset = loadExpression.Value.Visit(this);

            var reg = loadExpression.Type.IsPointer ? NewAddressReg() : NewDataReg();

            if (offset is Register)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Address,
                    Register1 = (Register)offset,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Width = TypeToWidth(loadExpression.Type),
                    Register2 = reg,
                    Comment = "Load by register address"
                });
                return reg;
            }
            else
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    FinalRegister1 = M68kRegister.SP,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Offset = (int)offset,
                    Width = TypeToWidth(loadExpression.Type),
                    Register2 = reg,
                    Comment = "Load from stack"
                });
                return reg;
            }

            /*if (ptrReg.Type == RegType.Data)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = ptrReg,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Width = TypeToWidth(loadExpression.Type),
                    Register2 = reg,
                    Comment = "Load from Data reg"
                });
            }
            else
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Address,
                    Register1 = ptrReg,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Width = TypeToWidth(loadExpression.Type),
                    Register2 = reg,
                    Comment = "Load from pointer"
                });
            }

            return reg;*/
        }

        public object Visit(StoreStatement storeStatement)
        {
            var reg = storeStatement.Value.Visit(this);
            if (reg is Register)
            {
                var varReg = GetVarRegister(storeStatement.Variable);

                if (varReg != null)
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = (Register) reg,
                        AddressingMode2 = M68kAddressingMode.Address,
                        Register2 = varReg,
                        Comment = "Store to reg"
                    });
                }
                else
                {
                    var offset = currentFunction.VarsInStack[storeStatement.Variable];
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = (Register)reg,
                        AddressingMode2 = M68kAddressingMode.AddressWithOffset,
                        FinalRegister2 = M68kRegister.SP,
                        Offset = offset,
                        Comment = "Store to stack"
                    });
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return null;
        }

        public object Visit(VariableAssignmentStatement variableAssignmentStatement)
        {
            if (variableAssignmentStatement.Expr is AllocaExpression)
            {
                var alloca = variableAssignmentStatement.Expr as AllocaExpression;
                currentFunction.VarsInStack[variableAssignmentStatement.Variable] = currentFunction.FrameSize;

                if (alloca.Type.IsArray)
                {
                    var elSize = alloca.Type.Type == Token.I8 ? 1 : 4;
                    currentFunction.FrameSize = alloca.Type.ArrayX*elSize;
                }
                else
                {
                    // TODO: FIX!!!
                    currentFunction.FrameSize = 4;
                }
                return null;
            }

            var valReg = variableAssignmentStatement.Expr.Visit(this);

            Debug.Assert(valReg != null);

            if (valReg is Register)
            {
                varRegs[variableAssignmentStatement.Variable] = (Register) valReg;
                return valReg;
            }
            else
            {
                var newTemp = variableAssignmentStatement.Expr.Type.IsPointer ? NewAddressReg() : NewDataReg();
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    FinalRegister1 = M68kRegister.SP,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Offset = (int)valReg,
                    Register2 = newTemp
                });
                varRegs[variableAssignmentStatement.Variable] = newTemp;
                return newTemp;
            }
        }

        public Dictionary<string, Declaration> Globals = new Dictionary<string, Declaration>();

        public object Visit(Declaration declaration)
        {
            Globals.Add(declaration.Name, declaration);
            return null;
        }
    }
}