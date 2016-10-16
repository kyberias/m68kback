using System;
using System.Collections.Generic;
//using System.Configuration;
using System.IO;
//using System.Linq;
using System.Text;

namespace m68kback
{
    class M68kBackend
    {
        static void Main(string[] args)
        {
            var llFilename = args[0];
            Console.WriteLine(llFilename);

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
                    foreach (var inst in func.Value.Instructions)
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
