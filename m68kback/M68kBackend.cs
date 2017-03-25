using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace m68kback
{
    interface OutputFileWriter
    {
        void WriteFile(string fileName, CodeGenerator code);
    }

    class M68kBackend
    {
        static void Main(string[] args)
        {
            var llFilename = args[0];
            var outputFileName = args[1];
            //Console.WriteLine(llFilename);

            var tokenizer = new Tokenizer();
            IList<TokenElement> elements;

            using (var stream = File.OpenRead(llFilename))
            {
                elements = tokenizer.Lex(stream).ToList();
            }

            var parser = new Parser(elements);
            var prg = parser.ParseProgram();

            var semanticAnalysis = new SemanticAnalysis();
            semanticAnalysis.Visit(prg);

            var codeGenerator = new CodeGenerator();
            codeGenerator.Visit(prg);

            OutputFileWriter writer = new AssemblerWriter();

            writer.WriteFile(outputFileName, codeGenerator);
         }
    }
}
