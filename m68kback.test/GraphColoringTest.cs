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
        Register d2 = new Register { Type = RegType.Data, Number = 2 };
        Register d3 = new Register { Type = RegType.Data, Number = 3 };
        Register d4 = new Register { Type = RegType.Data, Number = 4 };
        Register d5 = new Register { Type = RegType.Data, Number = 5 };
        Register d6 = new Register { Type = RegType.Data, Number = 6 };
        Register d7 = new Register { Type = RegType.Data, Number = 7 };

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
            var instructions = new[]
            {
                new M68kInstruction(M68kOpcode.Move, d2, d12),
                new M68kInstruction(M68kOpcode.Move, d3, d13),
/*                new M68kInstruction(M68kOpcode.Move, d4, d14),
                new M68kInstruction(M68kOpcode.Move, d5, d15),
                new M68kInstruction(M68kOpcode.Move, d6, d16),
                new M68kInstruction(M68kOpcode.Move, d7, d17),*/
                new M68kInstruction(M68kOpcode.Move, 42, d18),
                new M68kInstruction(M68kOpcode.Move, d18, d0),
                new M68kInstruction(M68kOpcode.Move, d12, d2),
                new M68kInstruction(M68kOpcode.Move, d13, d3),
/*                new M68kInstruction(M68kOpcode.Move, d14, d4),
                new M68kInstruction(M68kOpcode.Move, d15, d5),
                new M68kInstruction(M68kOpcode.Move, d16, d6),
                new M68kInstruction(M68kOpcode.Move, d17, d7),*/
                new M68kInstruction(M68kOpcode.Rts) { FinalRegister1 = M68kRegister.D0}
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

            Assert.IsTrue(gc.Graph.IsEdgeBetween("D0", "D2"));
            Assert.IsTrue(gc.Graph.IsEdgeBetween("D0", "D3"));
            Assert.IsTrue(gc.Graph.IsEdgeBetween("D0", "D13"));
            Assert.IsTrue(gc.Graph.IsEdgeBetween("D0", "D12"));

            Assert.IsTrue(gc.Graph.IsEdgeBetween("D2", "D3"));
            Assert.IsTrue(gc.Graph.IsEdgeBetween("D2", "D13"));

            Assert.IsTrue(gc.Graph.IsEdgeBetween("D3", "D12"));

            Assert.IsTrue(gc.Graph.IsEdgeBetween("D13", "D12"));
            Assert.IsTrue(gc.Graph.IsEdgeBetween("D13", "D18"));

            Assert.IsTrue(gc.Graph.IsEdgeBetween("D12", "D18"));

            Assert.IsFalse(gc.Graph.IsEdgeBetween("D2", "D12"));
            Assert.IsFalse(gc.Graph.IsEdgeBetween("D3", "D13"));
            Assert.IsFalse(gc.Graph.IsEdgeBetween("D0", "D18"));

            gc.FinalRewrite();

            CodeGenerator.RemoveRedundantMoves(gc.Instructions);

            var code = gc.Instructions;

            Assert.AreEqual(3, gc.Instructions.Count);
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
