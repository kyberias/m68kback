using System;
using System.Collections.Generic;
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
            //Console.WriteLine(llFilename);

            var tokenizer = new Tokenizer();

            using (var stream = File.OpenRead(llFilename))
            {
                var elements = tokenizer.Lex(stream);
                var parser = new Parser(elements);
                var prg = parser.ParseProgram();

                var semanticAnalysis = new SemanticAnalysis();
                semanticAnalysis.Visit(prg);

                var codeGenerator = new CodeGenerator();
                codeGenerator.Visit(prg);

                foreach (var decl in codeGenerator.Globals.Where(d => d.Value.Declare || d.Value.External))
                {
                    //if (decl.Value.Value == null && decl.Value.Expr == null)
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
                    foreach (var inst in func.Value.Instructions)
                    {
                        Console.WriteLine(inst);
                    }
                }

                Console.WriteLine("         section data,DATA");

                foreach (var decl in codeGenerator.Globals.Where(d => (d.Value.Global || d.Value.Constant) && !d.Value.External))
                {
                    if (decl.Value.Value != null)
                    {
                        Console.WriteLine("{0}    dc.b {1}",
                            M68kInstruction.ConvertLabel(decl.Value.Name),
                            string.Join(",", ToBytes(decl.Value.Value)));
                    }
                    else if(decl.Value.Type != null)
                    {
                        if (decl.Value.InitializeToZero || decl.Value.Expr == null)
                        {
                            Console.WriteLine(
                                $"{M68kInstruction.ConvertLabel(decl.Value.Name)}:    dcb.b {decl.Value.Type.Width},0");
                        }
                        else
                        {
                            Console.Write($"{M68kInstruction.ConvertLabel(decl.Value.Name)}:    ");
                            var sexpr = decl.Value.Expr as StructExpression;
                            if (sexpr != null)
                            {
                                GenerateStructExprData(sexpr);
                            }
                            else
                            {
                                var constant = decl.Value.Expr as IntegerConstant;
                                Console.WriteLine("DC." + (decl.Value.Type.Width == 1 ? "B" : (decl.Value.Type.Width == 2 ? "W" : "L") + "   " + constant.Constant));
                            }
                        }
                    }
                }

                Console.WriteLine("         end");
            }
        }

        static void GenerateStructExprData(StructExpression sexpr)
        {
            foreach (var val in sexpr.Values)
            {
                if (val.Value is StructExpression)
                {
                    GenerateStructExprData((StructExpression)val.Value);
                }
                else if (val.Value is IntegerConstant)
                {
                    var constant = val.Value as IntegerConstant;
                    Console.WriteLine("    DC." + (val.Type.Width == 1 ? "B" : (val.Type.Width == 2 ? "W" : "L")) +
                                      "   " + constant.Constant);
                }
                else if(val.Value == null)
                {
                    Console.WriteLine("    DC." + (val.Type.Width == 1 ? "B" : (val.Type.Width == 2 ? "W" : "L")) +
                                      "   0");
                }
                else
                {
                    Console.Write("    DC." + (val.Type.Width == 1 ? "B" : (val.Type.Width == 2 ? "W" : "L")));
                    Console.WriteLine($"    {M68kInstruction.ConvertLabel(((VariableReference)((GetElementPtr)val.Value).PtrVal).Variable)}");
                }
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
