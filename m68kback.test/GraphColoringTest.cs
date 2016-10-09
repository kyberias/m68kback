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
        Register d1 = new Register { Type = RegType.Data, Number = 1 };
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

        Register d30 = new Register { Type = RegType.Data, Number = 30 };
        Register d31 = new Register { Type = RegType.Data, Number = 31 };
        Register d32 = new Register { Type = RegType.Data, Number = 32 };
        Register d33 = new Register { Type = RegType.Data, Number = 33 };
        Register d34 = new Register { Type = RegType.Data, Number = 34 };
        Register d35 = new Register { Type = RegType.Data, Number = 35 };
        Register d36 = new Register { Type = RegType.Data, Number = 36 };
        Register d37 = new Register { Type = RegType.Data, Number = 37 };

        Register a10 = new Register { Type = RegType.Address, Number = 10 };

        [Test]
        public void Test()
        {
            var instructions = new[]
            {
                new M68kInstruction(M68kOpcode.Move, d2, d12),
                new M68kInstruction(M68kOpcode.Move, d3, d13),
                new M68kInstruction(M68kOpcode.Move, 42, d18),
                new M68kInstruction(M68kOpcode.Move, d18, d0),
                new M68kInstruction(M68kOpcode.Move, d12, d2),
                new M68kInstruction(M68kOpcode.Move, d13, d3),
                new M68kInstruction(M68kOpcode.Rts) { FinalRegister1 = M68kRegister.D0}
            }.ToList();

            instructions.Insert(0, new M68kInstruction
            {
                Opcode = M68kOpcode.RegDef,
                DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r).ToList()
            });

            var gc = new GraphColoring(instructions);
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
        public void Test2()
        {
            var instructions = new[]
            {
                new M68kInstruction(M68kOpcode.Sub)
                {
                    AddressingMode1 = M68kAddressingMode.Immediate,
                    Immediate = 100,
                    FinalRegister2 = M68kRegister.SP,
                    AddressingMode2 = M68kAddressingMode.Register,
                },

                new M68kInstruction(M68kOpcode.Move, d2, d32),
                new M68kInstruction(M68kOpcode.Move, d3, d33),
                new M68kInstruction(M68kOpcode.Move, d4, d34),
                new M68kInstruction(M68kOpcode.Move, d5, d35),
                new M68kInstruction(M68kOpcode.Move, d6, d36),
                new M68kInstruction(M68kOpcode.Move, d7, d37),

                //new M68kInstruction(M68kOpcode.RegDef) { DefsUses = new List<string> { "D15"} },
                new M68kInstruction(M68kOpcode.Move, 2, d15),
                new M68kInstruction(M68kOpcode.Move, 3, d16),
                new M68kInstruction(M68kOpcode.Move, 4, d17),
                new M68kInstruction(M68kOpcode.Move, 5, d18),

                new M68kInstruction(M68kOpcode.Move, d15, d20),
                new M68kInstruction(M68kOpcode.Add, d16, d20),
                new M68kInstruction(M68kOpcode.Add, d17, d20),
                new M68kInstruction(M68kOpcode.Add, d18, d20),
                new M68kInstruction(M68kOpcode.Move, d20, d0),

                new M68kInstruction(M68kOpcode.Move, d32, d2),
                new M68kInstruction(M68kOpcode.Move, d33, d3),
                new M68kInstruction(M68kOpcode.Move, d34, d4),
                new M68kInstruction(M68kOpcode.Move, d35, d5),
                new M68kInstruction(M68kOpcode.Move, d36, d6),
                new M68kInstruction(M68kOpcode.Move, d37, d7),

                new M68kInstruction(M68kOpcode.Add)
                {
                    AddressingMode1 = M68kAddressingMode.Immediate,
                    Immediate = 100,
                    FinalRegister2 = M68kRegister.SP,
                    AddressingMode2 = M68kAddressingMode.Register,
                },
                new M68kInstruction(M68kOpcode.Rts) { FinalRegister1 = M68kRegister.D0}
            }.ToList();

            instructions.Insert(0, new M68kInstruction
            {
                Opcode = M68kOpcode.RegDef,
                DefsUses = Enumerable.Range(0, 8).Select(r => "D" + r).ToList()
            });

            var gc = new GraphColoring(instructions);
            gc.Main();

            gc.FinalRewrite();

            CodeGenerator.RemoveRedundantMoves(gc.Instructions);
            CodeGenerator.RemoveInstructions(gc.Instructions, M68kOpcode.RegDef);

            var emul = new Emulator(gc.Instructions, new Dictionary<string, Declaration>());
            emul.RunFunction(null);

            Assert.AreEqual(14, emul.Regs[0]);
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
