using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace m68kback
{
    class M68kBackend
    {
        static void Main(string[] args)
        {
            var llFilename = args[0];

            var tokenizer = new Tokenizer();

            using (var stream = File.OpenRead(llFilename))
            {
                var elements = tokenizer.Lex(stream);
                var parser = new Parser(elements);
                var prg = parser.ParseProgram();

                var codeGenerator = new CodeGenerator();
                codeGenerator.Visit(prg);

                foreach (var decl in codeGenerator.Globals)
                {
                    if (decl.Value.Value == null)
                    {
                        Console.WriteLine("    xref " + decl.Value.Name.Replace("@", "_"));
                    }
                }

                foreach (var func in codeGenerator.Functions.Keys)
                {
                    Console.WriteLine("    xdef " + func.Replace("@", "_"));
                }

                Console.WriteLine("        section text,code");

                foreach (var func in codeGenerator.Functions)
                {
                    /*foreach (var inst in func.Value.Instructions)
                    {
                        Console.WriteLine(inst);
                    }*/

                    /*func.Value.Instructions.Insert(1, new M68kInstruction
                    {
                        Opcode = M68kOpcode.RegDef,
                        DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r).ToList()
                    });*/
                    /*func.Value.Instructions.Insert(1, new M68kInstruction
                    {
                        Opcode = M68kOpcode.RegUse,
                        DefsUses = Enumerable.Range(2, 6).Select(r => "D" + r).ToList()
                    });*/

                    // callee-saved: D2-D7
                    foreach (var d in Enumerable.Range(2, 6).Select(i => new Register {Number = i, Type = RegType.Data}))
                    {
                        var newtemp = func.Value.NewDataReg();

                        func.Value.Instructions.Insert(/*func.Value.PrologueLen + */1, new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            Register1 = d,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register2 = newtemp,
                            AddressingMode2 = M68kAddressingMode.Register
                        });

                        func.Value.Instructions.Insert(func.Value.Instructions.Count-1, new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            Register1 = newtemp,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register2 = d,
                            AddressingMode2 = M68kAddressingMode.Register
                        });
                    }

                    // callee-saved: A2-A6
                    foreach (var d in Enumerable.Range(2, 5).Select(i => new Register { Number = i, Type = RegType.Address }))
                    {
                        var newtemp = func.Value.NewAddressReg();

                        func.Value.Instructions.Insert(/*func.Value.PrologueLen + 1*/1, new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            Register1 = d,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register2 = newtemp,
                            AddressingMode2 = M68kAddressingMode.Register
                        });

                        func.Value.Instructions.Insert(func.Value.Instructions.Count - 1, new M68kInstruction
                        {
                            Opcode = M68kOpcode.Move,
                            Register1 = newtemp,
                            AddressingMode1 = M68kAddressingMode.Register,
                            Register2 = d,
                            AddressingMode2 = M68kAddressingMode.Register
                        });
                    }

                    /*func.Value.Instructions.Insert(func.Value.Instructions.Count-1, new M68kInstruction
                    {
                        Opcode = M68kOpcode.RegUse,
                        DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r).ToList()
                    });*/

                    var gcD = new GraphColoring(func.Value.Instructions, 8, RegType.Data);
                    gcD.Main();
                    gcD.FinalRewrite();

                    var gcA = new GraphColoring(gcD.Instructions, 7, RegType.Address);
                    gcA.Main();
                    gcA.FinalRewrite(RegType.Address);

                    var gcC = new GraphColoring(gcA.Instructions, 2, RegType.ConditionCode);
                    gcC.Main();
                    gcC.FinalRewrite(RegType.ConditionCode);

                    foreach (var inst in gcC.Instructions)
                    {
                        Console.WriteLine(inst);
                    }
                }

                Console.WriteLine("         section __MERGED,DATA");

                foreach (var decl in codeGenerator.Globals)
                {
                    if (decl.Value.Value != null)
                    {
                        Console.WriteLine("{0}    dc.b {1}", 
                            M68kInstruction.ConvertLabel(decl.Value.Name),
                            string.Join(",", ToBytes(decl.Value.Value)));
                    }
                }

                Console.WriteLine("         end");
            }
        }

        static IEnumerable<byte> ToBytes(string data)
        {
            var latin1 = Encoding.GetEncoding("ISO-8859-1");

            for(int i=0;i<data.Length;i++)
            {
                var c = data[i];
                if (c == '\\')
                {
                    yield return Convert.ToByte(data.Substring(i+1, 2), 16);
                    i += 2;
                    continue;
                }

                yield return latin1.GetBytes(c.ToString())[0];
            }
        }
    }
}
