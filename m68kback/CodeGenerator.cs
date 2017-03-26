using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace m68kback
{
    public class CodeGenerator : IVisitor
    {
        private readonly bool _printOutput;

        public CodeGenerator(bool printoutput = false)
        {
            _printOutput = printoutput;
        }

        public List<M68kInstruction> AllInstructions { get; } = new List<M68kInstruction>();

        public object Visit(Program el)
        {
            foreach (var d in el.Declarations)
            {
                d.Visit(this);
            }

            foreach (var f in el.Functions)
            {
                f.Visit(this);
                AllInstructions.AddRange(Instructions);
            }
            return null;
        }

        public IList<M68kInstruction> Instructions { get; set; } = new List<M68kInstruction>();

        M68kInstruction Emit(M68kInstruction i, int pos = -1)
        {
            if (pos == -1)
            {
                Instructions.Add(i);
            }
            else
            {
                Instructions.Insert(pos, i);
            }
            return i;
        }

        private Dictionary<string, int> frame;
        private Dictionary<string, TypeReference> vars;
        private int frameOffset;

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

        public static void RemoveRedundantMoves(IList<M68kInstruction> instructions)
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
            return;
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

        public static void RemoveInstructions(IList<M68kInstruction> instructions, params M68kOpcode[] opcodes)
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
            vars = new Dictionary<string, TypeReference>();
            frameOffset = 0;

            foreach (var s in el.Statements.Where(s => s is VariableAssignmentStatement))
            {
                var va = (VariableAssignmentStatement) s;

                frame[va.Variable] = frameOffset;

                frameStored[va.Variable] = va.Expr is GetElementPtr || va.Expr is LoadExpression;

                if (va.Expr is AllocaExpression)
                {
                    if (va.Expr.Type is PointerReference)
                    {
                        frameOffset += 4;
                    }
                    else
                    {
                        frameOffset += va.Expr.Type.Width;
                    }
                }
            }

            var parLoadsThatMustBeFixed = new List<M68kInstruction>();

            int parIx = 1;
            foreach (var parameter in el.Parameters)
            {
                Register parReg;
                if (parameter.Type is PointerReference)
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
                func.Instructions.Insert(/*func.PrologueLen +*/ 2, new M68kInstruction
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
                DefsUses = Enumerable.Range(2, 6).Select(r => "D" + r)
                                        .Union(Enumerable.Range(2,5).Select(r => "A" + r))
                                        .ToList()
                /*                DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r)
                                        .Union(Enumerable.Range(0,7).Select(r => "A" + r))
                                        .ToList()*/
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

            // Fix temp registers used before definition
            foreach (var i in func.Instructions)
            {
                if (i.Register1 != null && undefinedRegisters.ContainsKey(i.Register1))
                {
                    i.Register1 = GetVarRegister(undefinedRegisters[i.Register1]);
                }

                if (i.Register2 != null && undefinedRegisters.ContainsKey(i.Register2))
                {
                    i.Register2 = GetVarRegister(undefinedRegisters[i.Register2]);
                }
            }
            undefinedRegisters.Clear();

            foreach (var ph in phiFixRegs)
            {
                var reg = GetVarRegister(ph.Value);

                if (ph.Key.Opcode == M68kOpcode.RegDef)
                {
                    ph.Key.DefsUses = new List<string> {reg.ToString()};
                }
                else
                {
                    ph.Key.Register1 = reg;
                }
            }
            phiFixRegs.Clear();

            foreach (var ph in phiFixLabels)
            {
                var ix = func.Instructions.IndexOf(func.Instructions.First(i => i.Opcode == M68kOpcode.Label && i.Label == ph.BlockLabel.Substring(1))) + 1;
                for (var i=ix;i<func.Instructions.Count;i++)
                {
                    var inst = func.Instructions[i];

                    if (inst.Opcode == M68kOpcode.Label && !inst.LabelFromPhi && !inst.IgnoreLabelForPhi)
                    {
                        break;
                    }

                    if (inst.TargetLabel != null && inst.TargetLabel.Substring(1) == ph.OldLabel.Label)
                    {
                        // Insert a move from the variable referenced by the Phi instruction to the Phi target.

                        foreach (
                            var ph2 in
                                phiFixLabels.Where(
                                    p =>
                                        p.BlockLabel == ph.BlockLabel &&
                                        inst.TargetLabel.Substring(1) == p.OldLabel.Label && p.SourceVariable != null))
                        {
                            var move = new M68kInstruction
                            {
                                Opcode = M68kOpcode.Move,
                                AddressingMode1 = M68kAddressingMode.Register,
                                Register1 = GetVarRegister(ph2.SourceVariable),
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = ph2.TargetRegister
                            };

                            func.Instructions.Insert(i, move);
                            i++;
                        }

                        inst.TargetLabel = "%" + ph.NewLabel.Label;
                    }
                }
            }
            phiFixLabels.Clear();

            varRegs.Clear();

            // Restore all callee saved before returning

            var returns = func.Instructions.Where(i => i.Opcode == M68kOpcode.Rts).ToList();

            foreach (var ret in returns)
            {
                var retLoc = func.Instructions.IndexOf(ret);

                foreach (var cs in calleeSavedTemporaries)
                {
                    func.Instructions.Insert(retLoc - 1/*func.Instructions.Count - 2*/, new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        Register1 = cs.Value,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register2 = cs.Key,
                        //FinalRegister2 = cs.Key.ConvertToPhysicalRegister(),
                        AddressingMode2 = M68kAddressingMode.Register
                    });
                }
            }

            if (_printOutput)
            {
                Console.WriteLine("Before register allocation:");
                foreach (var i in func.Instructions)
                {
                    Console.WriteLine(i);
                }
            }

            var gcD = new GraphColoring(func.Instructions, spillStart: frameOffset);
            gcD.Main();
            gcD.FinalRewrite();
            frameOffset += gcD.SpillCount * 4;

            var gcA = new GraphColoring(gcD.Instructions, 7, RegType.Address, frameOffset);
            gcA.Main();
            gcA.FinalRewrite(RegType.Address);
            frameOffset += gcA.SpillCount * 4;

            var gcC = new GraphColoring(gcA.Instructions, 1, RegType.ConditionCode, frameOffset);
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

            Instructions = func.Instructions;
            if (_printOutput)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("AFTER register allocation and fixes:");
                foreach (var i in func.Instructions)
                {
                    Console.WriteLine(i);
                }
            }

            return null;
        }

        public object Visit(StructExpression expr)
        {
            return null;
        }

        public object Visit(AllocaExpression allocaExpression)
        {
            //throw new NotImplementedException();
            return null;
        }

        public object Visit(CastExpression expression)
        {
            return expression.Value.Visit(this);

            //throw new NotImplementedException();
        }

        public object Visit(CallExpression callExpression)
        {
            if (callExpression.FunctionName.Contains("llvm.lifetime.start") ||
                callExpression.FunctionName.Contains("llvm.lifetime.end"))
            {
                return null;
            }

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
                else if (parReg is Declaration)
                {
                    var decl = parReg as Declaration;
                    var addReg = NewAddressReg();

                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Lea,
                        Variable = decl.Name,
                        AddressingMode1 = M68kAddressingMode.Absolute,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = addReg,
                    });

                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = addReg,
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

        M68kOpcode OperatorToOpcode(Token oper)
        {
            switch (oper)
            {
                case Token.And:
                    return M68kOpcode.And;
                case Token.Add:
                    return M68kOpcode.Add;
                case Token.Sub:
                    return M68kOpcode.Sub;
                case Token.Xor:
                    return M68kOpcode.Eor;
                case Token.Ashr:
                    return M68kOpcode.Asr;
                case Token.Lshr:
                    return M68kOpcode.Lsr;
                default:
                    throw new NotSupportedException();
            }
        }

        public object Visit(ArithmeticExpression arithmeticExpression)
        {
            var resultReg = NewDataReg();

            var reg = (Register)arithmeticExpression.Operand1.Visit(this);

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = reg,//GetVarRegister(((VariableReference)arithmeticExpression.Operand1).Variable),
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
                            Opcode = OperatorToOpcode(arithmeticExpression.Operator),
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ((IntegerConstant) arithmeticExpression.Operand2).Constant,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                            Width = ByteWidthToM68Width(arithmeticExpression.Type.Width)
                        });
                    }
                    else
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = OperatorToOpcode(arithmeticExpression.Operator),
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = GetVarRegister(((VariableReference)arithmeticExpression.Operand2).Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                            Width = ByteWidthToM68Width(arithmeticExpression.Type.Width)
                        });
                    }
                    break;
                case Token.And:
                case Token.Xor:
                case Token.Ashr:
                case Token.Lshr:
                    if (arithmeticExpression.Operand2 is IntegerConstant)
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = OperatorToOpcode(arithmeticExpression.Operator),
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ((IntegerConstant)arithmeticExpression.Operand2).Constant,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                            Width = ByteWidthToM68Width(arithmeticExpression.Type.Width)
                        });
                    }
                    else if (arithmeticExpression.Operand2 is BooleanConstant)
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = OperatorToOpcode(arithmeticExpression.Operator),
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ((BooleanConstant)arithmeticExpression.Operand2).Constant ? 1 : 0,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                            Width = ByteWidthToM68Width(arithmeticExpression.Type.Width)
                        });
                    }
                    else
                    {
                        Emit(new M68kInstruction
                        {
                            Opcode = OperatorToOpcode(arithmeticExpression.Operator),
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = GetVarRegister(((VariableReference)arithmeticExpression.Operand2).Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                            Width = ByteWidthToM68Width(arithmeticExpression.Type.Width)
                        });
                    }
                    break;
                case Token.Mul:
                    {
                        if (arithmeticExpression.Operand2 is VariableReference)
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Muls,
                                Width = M68Width.Long,
                                AddressingMode1 = M68kAddressingMode.Register,
                                Register1 = GetVarRegister(((VariableReference)arithmeticExpression.Operand2).Variable),
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = resultReg
                            });
                        }
                        else
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Muls,
                                Width = M68Width.Long,
                                AddressingMode1 = M68kAddressingMode.Immediate,
                                Immediate = ((IntegerConstant)arithmeticExpression.Operand2).Constant,
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = resultReg
                            });
                        }
                    }
                    break;
                case Token.Sdiv:
                    {
                        if (arithmeticExpression.Operand2 is VariableReference)
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Divs,
                                AddressingMode1 = M68kAddressingMode.Register,
                                Register1 = GetVarRegister(((VariableReference) arithmeticExpression.Operand2).Variable),
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = resultReg
                            });
                        }
                        else
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Divs,
                                AddressingMode1 = M68kAddressingMode.Immediate,
                                Immediate = ((IntegerConstant)arithmeticExpression.Operand2).Constant,
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = resultReg
                            });
                        }
                        // resultReg = ResultReg & 0xFFFF
                        Emit(new M68kInstruction
                        {
                            Width = M68Width.Long,
                            Opcode = M68kOpcode.And,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = 0xFFFF,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg
                        });
                    }
                    break;
                case Token.Srem:
                {
                    var ic = arithmeticExpression.Operand2 as IntegerConstant;
                    Register divReg;
                    if (ic != null)
                    {
                        divReg = NewDataReg();
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Immediate = ic.Constant,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = divReg
                        });
                    }
                    else
                    {
                        divReg = GetVarRegister(((VariableReference) arithmeticExpression.Operand2).Variable);
                    }
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Divs,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = divReg,
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
                case Token.Zext:
                    {
                        // TODO
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = GetVarRegister(((VariableReference)arithmeticExpression.Operand1).Variable),
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = resultReg,
                        });

                        /*var t = arithmeticExpression.Operand1.Type as DefinedTypeReference;
                        var it = t?.Type as InternalTypeDefinition;
                        if (it != null)
                        {
                            switch (it.Type)
                            {
                                
                            }
                        }*/
                    }
                    break;
                default:
                    throw new NotSupportedException(arithmeticExpression.Operator.ToString());
            }
            return resultReg;
        }

        public object Visit(SelectExpression expr)
        {
            var reg = (Register)expr.Expr.Visit(this);

            var resultReg = NewDataReg();

            var falseLabel = "false" + Guid.NewGuid().ToString().Replace("-", "");
            var doneLabel = "done" + Guid.NewGuid().ToString().Replace("-", "");

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Cmp,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = 0,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = reg
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Beq,
                TargetLabel = falseLabel
            });

            var trueVal = (Register)expr.TrueExpression.Visit(this);
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = trueVal,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resultReg
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                TargetLabel = doneLabel
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Label,
                Label = falseLabel
            });

            var falseVal = (Register)expr.FalseExpression.Visit(this);
            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = falseVal,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resultReg
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Label,
                Label = doneLabel
            });

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
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Immediate,
                Immediate = integerConstant.Constant,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = reg
            });
            return reg;
        }

        public object Visit(BooleanConstant constant)
        {
            throw new NotImplementedException();
        }

        public class Reloc
        {
            public int Index { get; set; }
            public Declaration Declaration { get; set; }
        }

        //public List<Reloc> Relocs { get; set; } = new List<Reloc>();

        void CalculatePtr(GetElementPtr getElementPtr, Register addr)
        {
            var t = getElementPtr.Type;

            foreach (var ix in getElementPtr.Indices)
            {
                Debug.Assert(t != null);
                var ptr = t as IndirectTypeReference;
                if (ptr != null)
                {
                    /*if (ptr.BaseType is ArrayReference)
                    {
                        ptr = (IndirectTypeReference)ptr.BaseType;
                    }*/

                    var elSize = ptr.BaseType.Width;

                    if (ix is IntegerConstant)
                    {
                        var i = ((IntegerConstant)ix).Constant;
                        i *= elSize;

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            Immediate = i,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Register2 = addr
                        });
                    }
                    else
                    {
                        var ixReg = (Register)ix.Visit(this);

                        var newTemp = NewDataReg();

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = ixReg,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = newTemp
                        });

                        Debug.Assert(elSize == 1 || elSize == 2 || elSize == 4); // Don't support anything else yet

                        if (elSize > 1)
                        {
                            Emit(new M68kInstruction
                            {
                                Opcode = M68kOpcode.Lsl,
                                AddressingMode1 = M68kAddressingMode.Immediate,
                                Immediate = elSize == 2 ? 1 : 2,
                                AddressingMode2 = M68kAddressingMode.Register,
                                Register2 = newTemp
                            });
                        }

                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            Register1 = newTemp,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register2 = addr
                        });
                    }
                    t = ptr.BaseType;
                }
                else
                {
                    var defType = t as DefinedTypeReference;
                    var userType = defType?.Type as UserTypeDefinition;
                    if (userType != null)
                    {
                        var i = ((IntegerConstant) ix).Constant;
                        var index = userType.Members.Select(m => m.Width).Take(i).Sum();
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            Immediate = index,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Register2 = addr
                        });

                        t = userType.Members[i];
                    }
                    else
                    {
                        throw new NotSupportedException();
                        /*var intType = ptr.PtrBaseType.Type as InternalTypeDefinition;
                        var index = intType.Width*t.Type.Width;
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Adda,
                            Immediate = index,
                            AddressingMode1 = M68kAddressingMode.Immediate,
                            Register2 = addr
                        });*/
                    }
                }
            }
        }

        public object Visit(GetElementPtr getElementPtr)
        {
            var ptrval = getElementPtr.PtrVal.Visit(this);
//            var ptrvar = (VariableReference) getElementPtr.PtrVal;
//            var varref = ptrval as VariableReference;

            //if(varref != null && varref.Variable[0] == '%')
            if(ptrval is Register)
            {
                var newTemp = NewAddressReg();
                //var varOrOffset = GetVarRegisterOrStackOffset(varref.Variable);
                //var varReg = varOrOffset as Register;
                var varReg = ptrval as Register;

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    Register1 = varReg,
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register2 = newTemp,
                    AddressingMode2 = M68kAddressingMode.Register
                });
                CalculatePtr(getElementPtr, newTemp);
                return newTemp;
            }

            if (ptrval is Declaration)
            {
                var decl = ptrval as Declaration;
                var newReg = NewAddressReg();

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Lea,
                    AddressingMode1 = M68kAddressingMode.Absolute,
                    Variable = decl.Name, //varref.Variable,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = newReg,
                    //Comment = $"getelementptr (declaration {decl.Name})"
                });
                CalculatePtr(getElementPtr, newReg);
                /*Relocs.Add(new Reloc
                {
                    Index = Instructions.Count - 1,
                    Declaration = Globals[ decl.Name]
                });*/
                return newReg;
            }

            if(ptrval is int)
            {
                var newTemp = NewAddressReg();

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Lea,
                    FinalRegister1 = M68kRegister.SP,
                    AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                    Offset =  (int)ptrval,
                    Register2 = newTemp,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Comment = "getelementptr (none)"
                });
                CalculatePtr(getElementPtr, newTemp);
                return newTemp;
            }

            throw new NotSupportedException();
        }

        Dictionary<Register, string> undefinedRegisters = new Dictionary<Register, string>();

        public object Visit(VariableReference variableReference)
        {
            if (currentFunction.VarsInStack.ContainsKey(variableReference.Variable))
            {
                return currentFunction.VarsInStack[variableReference.Variable];
            }

            if (variableReference.Variable[0] == '@')
            {
                return Globals[variableReference.Variable];
            }

            var varReg = GetVarRegister(variableReference.Variable);
            if (varReg == null)
            {
                // Variable not yet defined
                varReg = new Register();
                undefinedRegisters[varReg] = variableReference.Variable;
            }
            return varReg;
        }

        public object Visit(ExpressionStatement expressionStatement)
        {
            return expressionStatement.Expression.Visit(this);
        }

        public object Visit(TypeReference typeReference)
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

            var width = retStatement.Type.Width;

            if (width > 0)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Rts,
                    FinalRegister1 = M68kRegister.D0
                });
            }
            else
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Rts,
                });
            }

            return null;
        }

        public object Visit(SwitchStatement statement)
        {
            var reg = (Register)statement.Value.Visit(this);

            foreach (var e in statement.Switches)
            {
                var c = e.Value as IntegerConstant;

                if (c != null)
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Cmp,
                        Width = TypeToWidth(statement.Type),
                        Register2 = reg,
                        AddressingMode2 = M68kAddressingMode.Register,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = c.Constant
                    });
                }
                else
                {
                    throw new NotSupportedException();
                }

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Beq,
                    TargetLabel = e.Label + functionIx
                });
            }

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                TargetLabel = statement.DefaultLabel + functionIx
            });

            return null;
        }

        public object Visit(IcmpExpression icmpExpression)
        {
            //statusReg = icmpExpression.Condition;

            if (icmpExpression.Value is IntegerConstant || icmpExpression.Value == null)
            {
                var imm = (icmpExpression.Value as IntegerConstant)?.Constant ?? 0;

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Cmp,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.Immediate,
                    Immediate = imm,
                    Register2 = GetVarRegister(icmpExpression.Var),
                    AddressingMode2 = M68kAddressingMode.Register
                });
            }
            else if (icmpExpression.Value is BooleanConstant)
            {
                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Cmp,
                    Width = TypeToWidth(icmpExpression.Type),
                    AddressingMode1 = M68kAddressingMode.Immediate,
                    Immediate = (icmpExpression.Value as BooleanConstant).Constant ? 1 : 0,
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
                
            }

            var resReg = NewDataReg();

            var trueLabel = "icmp_true_" + Instructions.Count;
            var falseLabel = "icmp_false_" + Instructions.Count;
            var endLabel = "icmp_end_" + Instructions.Count;

            Emit(new M68kInstruction
            {
                Opcode = ConditionToOpcode(icmpExpression.Condition),
                IgnoreLabelForPhi = true,
                TargetLabel = "%" + trueLabel
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                IgnoreLabelForPhi = true,
                TargetLabel = "%" + falseLabel
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Label,
                Label = trueLabel,
                IgnoreLabelForPhi = true
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Immediate = 1,
                AddressingMode1 = M68kAddressingMode.Immediate,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resReg,
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                IgnoreLabelForPhi = true,
                TargetLabel = "%" + endLabel
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Label,
                Label = falseLabel,
                IgnoreLabelForPhi = true
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                Immediate = 0,
                AddressingMode1 = M68kAddressingMode.Immediate,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = resReg,
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Label,
                Label = endLabel,
                IgnoreLabelForPhi = true
            });

            return resReg;
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

        M68kOpcode ConditionToOpcode(Token condition)
        {
            M68kOpcode opc;
            switch (condition)
            {
                case Token.Eq:
                    opc = M68kOpcode.Beq;
                    break;
                case Token.Ne:
                    opc = M68kOpcode.Bne;
                    break;
                case Token.Slt:
                case Token.Ult:
                    opc = M68kOpcode.Blt;
                    break;
                case Token.Sgt:
                    opc = M68kOpcode.Bgt;
                    break;
                case Token.Sge:
                    opc = M68kOpcode.Bge;
                    break;
                default:
                    throw new NotSupportedException(condition.ToString());
            }
            return opc;
        }

        public object Visit(ConditionalBrStatement conditionalBrStatement)
        {
            var reg = GetVarRegister(conditionalBrStatement.Identifier);

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Tst,
                Register1 = reg,
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Bne,
                TargetLabel = conditionalBrStatement.Label1 + functionIx
            });

            Emit(new M68kInstruction
            {
                Opcode = M68kOpcode.Jmp,
                TargetLabel = conditionalBrStatement.Label2 + functionIx
            });
            return null;
        }

        M68Width TypeToWidth(TypeReference type)
        {
            switch (type.Width)
            {
                case 4:
                    return M68Width.Long;
                case 2:
                    return M68Width.Word;
                case 1:
                    return M68Width.Byte;
            }

            throw new NotSupportedException();
        }

        public object Visit(LoadExpression loadExpression)
        {
            //var ptrReg = (Register)loadExpression.Value.Visit(this);
            var offset = loadExpression.Value.Visit(this);

            var reg = loadExpression.Type is PointerReference ? NewAddressReg() : NewDataReg();

            if (offset is Register)
            {
                if (((Register) offset).ToString()[0] != 'A')
                {
                    throw new Exception("Must be Address!");
                }

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

            if (offset is Declaration)
            {
                var decl = offset as Declaration;
                var addrReg = NewAddressReg();

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Lea,
                    AddressingMode1 = M68kAddressingMode.Absolute,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Variable = decl.Name,
                    Register2 = addrReg,
                });

                Emit(new M68kInstruction
                {
                    Opcode = M68kOpcode.Move,
                    AddressingMode1 = M68kAddressingMode.Address,
                    Register1 = addrReg,
                    AddressingMode2 = M68kAddressingMode.Register,
                    Register2 = reg,
                });

                return reg;
            }
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

        M68Width ByteWidthToM68Width(int byteWidth)
        {
            switch (byteWidth)
            {
                case 1:
                    return M68Width.Byte;
                case 2:
                    return M68Width.Word;
                case 4:
                    return M68Width.Long;
                default:
                    throw new NotSupportedException();
            }
        }

        public object Visit(StoreStatement storeStatement)
        {
            var valueToSave = storeStatement.Value.Visit(this);
            var ptrVal = storeStatement.PtrExpr.Visit(this);
            //var decl = (Declaration) ptrVal;
            //var ptrReg = ptrVal as Register;

            if (valueToSave is Register)
            {
                //var varReg = GetVarRegister(storeStatement.Variable);
                //var ptr = (PointerReference) storeStatement.Type;
                //var intType = ptr.PtrBaseType.Type as InternalTypeDefinition;

                //var varReg = (Register) ptrVal;

                if (ptrVal is Register)
                {
                    var varReg = (Register) ptrVal;
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Register1 = (Register) valueToSave,
                        AddressingMode2 = M68kAddressingMode.Address,
                        Register2 = varReg,
                        Comment = "Store to reg",
                        Width = ByteWidthToM68Width(storeStatement.ExprType.Width)
                    });
                }
                else
                {
                    //if (storeStatement.Variable[0] == '@')
                    //if(decl != null)
                    if(ptrVal is Declaration)
                    {
                        var decl = (Declaration) ptrVal;
                        // Global
                        var tempAddr = NewAddressReg();
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Lea,
                            AddressingMode1 = M68kAddressingMode.Absolute,
                            Variable = decl.Name,
                            AddressingMode2 = M68kAddressingMode.Register,
                            Register2 = tempAddr,
                        });
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = (Register)valueToSave,
                            AddressingMode2 = M68kAddressingMode.Address,
                            Register2 = tempAddr
                        });
                    }
                    else if(ptrVal is int)
                    {
                        var offset = (int)ptrVal; // currentFunction.VarsInStack[storeStatement.Variable];
                        Emit(new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register1 = (Register) valueToSave,
                            AddressingMode2 = M68kAddressingMode.AddressWithOffset,
                            FinalRegister2 = M68kRegister.SP,
                            Offset = offset,
                            Comment = $"Store to stack"
                        });
                    }
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
                currentFunction.FrameSize += alloca.Type.Width;
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
                if (variableAssignmentStatement.Expr.Type is PointerReference)
                {
                    var newTemp = NewAddressReg();
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        FinalRegister1 = M68kRegister.SP,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Offset = (int) valReg,
                        Register2 = newTemp
                    });
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Adda,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = (int) valReg,
                        Register2 = newTemp
                    });

                    varRegs[variableAssignmentStatement.Variable] = newTemp;
                    return newTemp;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public Dictionary<string, Declaration> Globals = new Dictionary<string, Declaration>();

        public object Visit(Declaration declaration)
        {
            Globals.Add(declaration.Name, declaration);
            return null;
        }

        /// <summary>
        /// Branch labels we need to fix because the target label has changed because of PHI instruction
        /// </summary>
        class PhiLabelFix
        {
            /// <summary>
            /// The label that starts the block containing the branches we need to fix.
            /// </summary>
            public string BlockLabel { get; set; }

            /// <summary>
            /// Old target label instruction (opcode == Label)
            /// </summary>
            public M68kInstruction OldLabel { get; set; }

            /// <summary>
            /// Newly generated target label instruction (opcode == Label)
            /// </summary>
            public M68kInstruction NewLabel { get; set; }

            /// <summary>
            /// The label that ends the code generated by the Phi instruction
            /// </summary>
            public M68kInstruction EndLabel { get; set; }

            /// <summary>
            /// The temporary register generated by the Phi instruction
            /// </summary>
            public Register TargetRegister { get; set; }

            /// <summary>
            /// If the Phi block used a variable, this is the name of the variable
            /// </summary>
            public string SourceVariable { get; set; }
        }

        List<PhiLabelFix> phiFixLabels = new List<PhiLabelFix>();
        Dictionary<M68kInstruction, string> phiFixRegs = new Dictionary<M68kInstruction, string>();

        public object Visit(PhiExpression phiExpression)
        {
            M68kInstruction lab;
            M68kInstruction endlabel;

            var lastLabel = Instructions.Last();
            Debug.Assert(lastLabel.Opcode == M68kOpcode.Label);

            var lastFix = phiFixLabels.LastOrDefault(f => f.EndLabel == lastLabel);

            if (lastFix != null)
            {
                lab = lastFix.OldLabel;
                endlabel = lastLabel;
            }
            else
            {
                lab = Instructions.Last(i => i.Opcode == M68kOpcode.Label);
                var endLabelName = lab.Label + "." + "end";
                endlabel = Emit(new M68kInstruction {Opcode = M68kOpcode.Label, Label = endLabelName, LabelFromPhi = true},
                    Instructions.IndexOf(lab) + 1);
            }

            var tempReg = phiExpression.Type is PointerReference ? NewAddressReg() : NewDataReg();

            Debug.Assert(lab != null);

            int ix = Instructions.IndexOf(lab) + 1;

            foreach (var phiBranch in phiExpression.Values)
            {
                var newLabel = lab.Label + "." + phiFixLabels.Count;
                if (lastFix != null)
                {
                    ix = Instructions.IndexOf(phiFixLabels.First(f => f.BlockLabel == phiBranch.Label + functionIx).NewLabel) + 1;
                }
               
                M68kInstruction newL;

                if (lastFix == null)
                {
                    newL = Emit(new M68kInstruction {Opcode = M68kOpcode.Label, Label = newLabel, LabelFromPhi = true }, ix++);
                }
                else
                {
                    newL = phiFixLabels.First(f => f.BlockLabel == phiBranch.Label + functionIx).NewLabel;
                }

                string sourceVar = null;

                if (phiBranch.Expr is IntegerConstant)
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Move,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = ((IntegerConstant) phiBranch.Expr).Constant,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = tempReg,
                        Comment = "from Phi"
                    }, ix++);
                }
                else if (phiBranch.Expr is BooleanConstant)
                {
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.MoveQ,
                        AddressingMode1 = M68kAddressingMode.Immediate,
                        Immediate = ((BooleanConstant)phiBranch.Expr).Constant ? 1 : 0,
                        AddressingMode2 = M68kAddressingMode.Register,
                        Register2 = tempReg,
                        Comment = "from Phi"
                    }, ix++);
                }

                else if (phiBranch.Expr is VariableReference)
                {
                    // We will handle this later by inserting a move before each branch!
                    var varref = phiBranch.Expr as VariableReference;
                    sourceVar = varref.Variable;
                }
                else
                {
                    throw new NotSupportedException();
                }

                if (lastFix == null)
                {
                    // Only the first PHI generates the jump.
                    Emit(new M68kInstruction
                    {
                        Opcode = M68kOpcode.Jmp,
                        TargetLabel = "%" + endlabel.Label,
                        Comment = "from Phi"
                    }, ix++);
                }
                phiFixLabels.Add(new PhiLabelFix
                    {
                        BlockLabel = phiBranch.Label + functionIx,
                        OldLabel = lab,
                        NewLabel = newL,
                        EndLabel = endlabel,
                        TargetRegister = tempReg,
                        SourceVariable = sourceVar
                    });
            }

            return tempReg;
        }

        public object Visit(TypeDefinition typeDef)
        {
            throw new NotImplementedException();
        }
    }
}