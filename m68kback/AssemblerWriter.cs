using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace m68kback
{
    class AssemblerWriter : OutputFileWriter
    {
        string FixName(string name)
        {
            return name.Replace("@", "_").Replace(".", "$");
        }

        public void WriteFile(string fileName, CodeGenerator codeGenerator)
        {
            using (var writer = new StreamWriter(fileName))
            {

                foreach (var decl in codeGenerator.Globals.Where(d => d.Value.Declare || d.Value.External))
                {
                    //if (decl.Value.Value == null && decl.Value.Expr == null)
                    {
                        writer.WriteLine("    xref " + FixName(decl.Value.Name));
                    }
                }

                foreach (var func in codeGenerator.Functions.Keys)
                {
                    writer.WriteLine("    xdef " + FixName(func));
                }

                writer.WriteLine("        section text,code");

                foreach (var func in codeGenerator.Functions)
                {
                    foreach (var inst in func.Value.Instructions)
                    {
                        writer.WriteLine(inst);
                    }
                }

                writer.WriteLine("         section data,DATA");

                foreach (
                    var decl in
                        codeGenerator.Globals.Where(d => (d.Value.Global || d.Value.Constant) && !d.Value.External))
                {
                    if (decl.Value.Value != null)
                    {
                        writer.WriteLine("{0}    dc.b {1}",
                            M68kInstruction.ConvertLabel(decl.Value.Name),
                            string.Join(",", ToBytes(decl.Value.Value)));
                    }
                    else if (decl.Value.Type != null)
                    {
                        if (decl.Value.InitializeToZero || decl.Value.Expr == null)
                        {
                            writer.WriteLine(
                                $"{M68kInstruction.ConvertLabel(decl.Value.Name)}:    dcb.b {decl.Value.Type.Width},0");
                        }
                        else
                        {
                            writer.Write($"{M68kInstruction.ConvertLabel(decl.Value.Name)}:    ");
                            var sexpr = decl.Value.Expr as StructExpression;
                            if (sexpr != null)
                            {
                                GenerateStructExprData(sexpr, writer);
                            }
                            else if (decl.Value.Expr is ArrayExpression)
                            {
                                var arre = (ArrayExpression) decl.Value.Expr;

                                foreach (var vl in arre.Values)
                                {
                                    if (vl is VariableReference)
                                    {
                                        var e = (VariableReference) vl;
                                        writer.WriteLine("\tDC.L " + FixName(e.Variable));
                                    }
                                    else if (vl is GetElementPtr)
                                    {
                                        var gep = (GetElementPtr) vl;
                                        writer.WriteLine("\tDC.L " + FixName(((VariableReference)gep.PtrVal).Variable));
                                    }
                                }
                            }
                            else
                            {
                                var constant = decl.Value.Expr as IntegerConstant;
                                writer.WriteLine("DC." +
                                                  (decl.Value.Type.Width == 1
                                                      ? "B"
                                                      : (decl.Value.Type.Width == 2 ? "W" : "L") + "   " +
                                                        constant.Constant));
                            }
                        }
                    }
                }

                writer.WriteLine("         end");
            }
        }

        static void GenerateStructExprData(StructExpression sexpr, TextWriter writer)
        {
            foreach (var val in sexpr.Values)
            {
                if (val.Value is StructExpression)
                {
                    GenerateStructExprData((StructExpression)val.Value, writer);
                }
                else if (val.Value is IntegerConstant)
                {
                    var constant = val.Value as IntegerConstant;
                    writer.WriteLine("    DC." + (val.Type.Width == 1 ? "B" : (val.Type.Width == 2 ? "W" : "L")) +
                                      "   " + constant.Constant);
                }
                else if (val.Value == null)
                {
                    writer.WriteLine("    DC." + (val.Type.Width == 1 ? "B" : (val.Type.Width == 2 ? "W" : "L")) +
                                      "   0");
                }
                else
                {
                    writer.Write("    DC." + (val.Type.Width == 1 ? "B" : (val.Type.Width == 2 ? "W" : "L")));
                    writer.WriteLine($"    {M68kInstruction.ConvertLabel(((VariableReference)((GetElementPtr)val.Value).PtrVal).Variable)}");
                }
            }
        }

        static IEnumerable<byte> ToBytes(string data)
        {
            var latin1 = Encoding.GetEncoding("ISO-8859-1");

            for (int i = 0; i < data.Length; i++)
            {
                var c = data[i];
                if (c == '\\')
                {
                    yield return Convert.ToByte(data.Substring(i + 1, 2), 16);
                    i += 2;
                    continue;
                }

                yield return latin1.GetBytes(c.ToString())[0];
            }
        }
    }
}