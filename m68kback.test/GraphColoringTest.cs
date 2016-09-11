using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    class GraphColoringTest
    {
        Register d0 = new Register { Type = RegType.Data, Number = 0 };

        Register d12 = new Register { Type = RegType.Data, Number = 12 };
        Register d13 = new Register { Type = RegType.Data, Number = 13 };
        Register d14 = new Register { Type = RegType.Data, Number = 14 };
        Register d15 = new Register { Type = RegType.Data, Number = 15 };
        Register d16 = new Register { Type = RegType.Data, Number = 16 };
        Register d17 = new Register { Type = RegType.Data, Number = 17 };
        Register d18 = new Register { Type = RegType.Data, Number = 18 };
        Register d19 = new Register { Type = RegType.Data, Number = 19 };
        Register d20 = new Register { Type = RegType.Data, Number = 20 };
        Register d21 = new Register { Type = RegType.Data, Number = 21 };
        Register d22 = new Register { Type = RegType.Data, Number = 22 };

        [Test]
        public void Test()
        {
            var tempreg = d12;

            var instructions = new[]
            {
                new M68kInstruction(M68kOpcode.Move, 42, tempreg),
                new M68kInstruction(M68kOpcode.Move, tempreg, d0),
                new M68kInstruction(M68kOpcode.Rts)
            }.ToList();

            instructions.Insert(0, new M68kInstruction
            {
                Opcode = M68kOpcode.RegDef,
                DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r).ToList()
            });

            var gc = new GraphColoring(instructions, 8);

            //var stack = gc.Simplify();

            //var newgraph = GraphColoring.Select(gc.Graph, stack);

            gc.Main();

            gc.FinalRewrite();

            var code = gc.Instructions;
        }

        [Test]
        public void SelectTest()
        {
            var ig = new InterferenceGraph();
            var stack = new Stack<string>();

            foreach (var n in "ghkdjefbcm")
            {
                stack.Push(n.ToString());
                ig.Nodes.Add(n.ToString());
            }

            ig.Graph.Add(new Tuple<string, string>("j", "f"));
            ig.Graph.Add(new Tuple<string, string>("j", "e"));
            ig.Graph.Add(new Tuple<string, string>("j", "k"));
            ig.Graph.Add(new Tuple<string, string>("j", "d"));
            ig.Graph.Add(new Tuple<string, string>("j", "h"));
            ig.Graph.Add(new Tuple<string, string>("j", "g"));

            ig.Graph.Add(new Tuple<string, string>("f", "m"));
            ig.Graph.Add(new Tuple<string, string>("f", "e"));

            ig.Graph.Add(new Tuple<string, string>("e", "b"));
            ig.Graph.Add(new Tuple<string, string>("e", "m"));

            ig.Graph.Add(new Tuple<string, string>("k", "b"));
            ig.Graph.Add(new Tuple<string, string>("k", "d"));
            ig.Graph.Add(new Tuple<string, string>("k", "g"));

            ig.Graph.Add(new Tuple<string, string>("b", "c"));
            ig.Graph.Add(new Tuple<string, string>("b", "m"));
            ig.Graph.Add(new Tuple<string, string>("b", "d"));

            ig.Graph.Add(new Tuple<string, string>("m", "c"));
            ig.Graph.Add(new Tuple<string, string>("m", "d"));

            ig.Graph.Add(new Tuple<string, string>("h", "g"));

            var newgraph = GraphColoring.Select(ig, stack);

            Assert.AreEqual(4, newgraph.Allocation.Count);
        }
    }
}
