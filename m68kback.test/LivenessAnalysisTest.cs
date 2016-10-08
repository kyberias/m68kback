using System.Collections.Generic;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    public class LivenessAnalysisTest
    {
        Register d0 = new Register { Type = RegType.Data, Number = 0 };
        Register d1 = new Register { Type = RegType.Data, Number = 1 };
        Register d2 = new Register { Type = RegType.Data, Number = 2 };
        Register d3 = new Register { Type = RegType.Data, Number = 3 };

        [Test]
        public void BranchesTest()
        {
            var d20 = new Register {Type = RegType.Data, Number = 20};
            var d21 = new Register { Type = RegType.Data, Number = 21 };
            var d22 = new Register { Type = RegType.Data, Number = 22 };
            var d23 = new Register { Type = RegType.Data, Number = 23 };
            var d24 = new Register { Type = RegType.Data, Number = 24 };
            var d25 = new Register { Type = RegType.Data, Number = 25 };

            var a7 = new Register { Type = RegType.Address, Number = 7 };
            var a8 = new Register { Type = RegType.Address, Number = 7 };
            var a15 = new Register { Type = RegType.Address, Number = 15 };
            var a16 = new Register { Type = RegType.Address, Number = 15 };

            var instructions = new[]
            {
                new M68kInstruction(M68kOpcode.Move, d20, d21),
                new M68kInstruction(M68kOpcode.Add, -1, d21),
                new M68kInstruction(M68kOpcode.Jmp) { TargetLabel = "for$body0$7"},
                new M68kInstruction(M68kOpcode.Label) {Label = "for$body0"},
                new M68kInstruction(M68kOpcode.Label) {Label = "for$body0$7"},
                new M68kInstruction(M68kOpcode.MoveQ, 0, d22),
                new M68kInstruction(M68kOpcode.Jmp) { TargetLabel = "for$body0$end"},
                new M68kInstruction(M68kOpcode.Label) {Label = "for$body0$8"},
                new M68kInstruction(M68kOpcode.Jmp) { TargetLabel = "for$body0$end"},
                new M68kInstruction(M68kOpcode.Label) {Label = "for$body0$end"},
                new M68kInstruction(M68kOpcode.MoveQ, d21, d23),
                new M68kInstruction(M68kOpcode.Sub, d22, d23),
                new M68kInstruction(M68kOpcode.Move, a7, a15),
                new M68kInstruction(M68kOpcode.Adda, d23, a15),
                new M68kInstruction(M68kOpcode.Move, a15, d24),
                new M68kInstruction(M68kOpcode.Move, a8, a16),

                new M68kInstruction(M68kOpcode.Adda, d22, a16),
                new M68kInstruction(M68kOpcode.Move, d24, a16),
                new M68kInstruction(M68kOpcode.Move, d22, d25),
                new M68kInstruction(M68kOpcode.Adda, 1, d25),
                new M68kInstruction(M68kOpcode.Cmp, d20, d25),
                new M68kInstruction(M68kOpcode.Beq) { TargetLabel = "for$end$loopexit0"},
                new M68kInstruction(M68kOpcode.Move, d25, d22),
                new M68kInstruction(M68kOpcode.Jmp) { TargetLabel = "for$body0$8"},
                new M68kInstruction(M68kOpcode.Label) {Label = "for$end$loopexit0"},
            };

            var la = new LivenessAnalysis(instructions);

            var gr = InterferenceGraphGenerator.MakeGraph(la.Nodes, RegType.Data, new List<string>());

            Assert.IsTrue(gr.IsEdgeBetween("D21", "D20"));
            Assert.IsTrue(gr.IsEdgeBetween("D21", "D22"));
            Assert.IsTrue(gr.IsEdgeBetween("D21", "D23"));
            Assert.IsTrue(gr.IsEdgeBetween("D21", "D24"));
            Assert.IsTrue(gr.IsEdgeBetween("D21", "D25"));

            Assert.IsFalse(gr.IsEdgeBetween("D23", "D24"));
            Assert.IsFalse(gr.IsEdgeBetween("D23", "D25"));
            Assert.IsFalse(gr.IsEdgeBetween("D24", "D25"));

            Assert.IsTrue(gr.IsEdgeBetween("D22", "D20"));
            Assert.IsTrue(gr.IsEdgeBetween("D22", "D23"));
            Assert.IsTrue(gr.IsEdgeBetween("D22", "D24"));
            Assert.IsFalse(gr.IsEdgeBetween("D22", "D25"));

            /*
                add.l #1,D25_
                cmp.l D20_,D25_
                beq for$end$loopexit0
                move.l D25_,D22_
                jmp for$body0$8
            for$end$loopexit0:
             */
        }

        [Test]
        public void PredSuccWithBranch()
        {
            var instructions = new[]
            {
                new M68kInstruction(M68kOpcode.Move, 4, d0),                    // 0
                new M68kInstruction(M68kOpcode.Bgt) { TargetLabel = "end" },    // 1
                new M68kInstruction(M68kOpcode.Label) { Label = "foo" },        // 2
                new M68kInstruction(M68kOpcode.Move, 1, d0),                    // 3
                new M68kInstruction(M68kOpcode.Label) { Label = "end" },        // 4
                new M68kInstruction(M68kOpcode.Move, 3, d0),                    // 5
                //new M68kInstruction(M68kOpcode.Rts) // This would introduce D2-D7
                new M68kInstruction(M68kOpcode.Move)                            // 6
                {
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = d0,
                    AddressingMode2 = M68kAddressingMode.Address,
                    FinalRegister2 = M68kRegister.SP
                },
                new M68kInstruction(M68kOpcode.Jmp) {TargetLabel = "foo"},       // 7
                new M68kInstruction(M68kOpcode.Move, 3, d0),                    // 8
            };

             //TODO: Conditional branches have 2 successors!!!

            var la = new LivenessAnalysis(instructions);

            CollectionAssert.AreEquivalent(la.Nodes[0].Pred, new CfgNode[] { });
            CollectionAssert.AreEquivalent(la.Nodes[0].Succ, new[] { la.Nodes[1]});

            CollectionAssert.AreEquivalent(la.Nodes[1].Pred, new[] { la.Nodes[0] });
            CollectionAssert.AreEquivalent(la.Nodes[1].Succ, new[] { la.Nodes[2], la.Nodes[4] });

            CollectionAssert.AreEquivalent(la.Nodes[2].Pred, new[] { la.Nodes[1], la.Nodes[7] });
            CollectionAssert.AreEquivalent(la.Nodes[2].Succ, new[] { la.Nodes[3] });

            CollectionAssert.AreEquivalent(la.Nodes[3].Pred, new[] { la.Nodes[2] });
            CollectionAssert.AreEquivalent(la.Nodes[3].Succ, new[] { la.Nodes[4] });

            CollectionAssert.AreEquivalent(la.Nodes[4].Pred, new[] { la.Nodes[1], la.Nodes[3] });
            CollectionAssert.AreEquivalent(la.Nodes[4].Succ, new[] { la.Nodes[5] });

            CollectionAssert.AreEquivalent(la.Nodes[5].Pred, new[] { la.Nodes[4] });
            CollectionAssert.AreEquivalent(la.Nodes[5].Succ, new[] { la.Nodes[6] });

            CollectionAssert.AreEquivalent(la.Nodes[6].Pred, new[] { la.Nodes[5] });
            CollectionAssert.AreEquivalent(la.Nodes[6].Succ, new[] { la.Nodes[7] });

            CollectionAssert.AreEquivalent(la.Nodes[7].Pred, new[] { la.Nodes[6] });
            CollectionAssert.AreEquivalent(la.Nodes[7].Succ, new[] { la.Nodes[2] });

            CollectionAssert.AreEquivalent(la.Nodes[8].Pred, new CfgNode[] { });
            CollectionAssert.AreEquivalent(la.Nodes[8].Succ, new CfgNode[] { });
        }

        [Test]
        public void Liveness()
        {
            var instructions = new[]
            {
                new M68kInstruction( M68kOpcode.Move, 4, d0),
                new M68kInstruction( M68kOpcode.Move, d0, d1),
                new M68kInstruction( M68kOpcode.Move, 1, d3),
                new M68kInstruction( M68kOpcode.Move, d1, d2),
                new M68kInstruction( M68kOpcode.Move, d2, d0),
                //new M68kInstruction(M68kOpcode.Rts) // This would introduce D2-D7
                new M68kInstruction(M68kOpcode.Move)
                {
                    AddressingMode1 = M68kAddressingMode.Register, Register1 = d0, AddressingMode2 = M68kAddressingMode.Address,
                    FinalRegister2 = M68kRegister.SP
                }
            };

            var la = new LivenessAnalysis(instructions);

            CollectionAssert.AreEquivalent(new[] { "D0" }, la.Nodes[0].Def);
            CollectionAssert.AreEquivalent(new[] { "D2" }, la.Nodes[3].Def);

            AssertOuts(la.Nodes[0], "D0");

            AssertIns(la.Nodes[1], "D0");
            AssertOuts(la.Nodes[1], "D1");

            AssertIns(la.Nodes[2], "D1");
            AssertOuts(la.Nodes[2], "D1");

            AssertIns(la.Nodes[3], "D1");
            AssertOuts(la.Nodes[3], "D2");

            AssertIns(la.Nodes[4], "D2");
            AssertOuts(la.Nodes[4], "D0");
        }

        void AssertOuts(CfgNode node, params string[] regs)
        {
            CollectionAssert.AreEquivalent(regs, node.Out);
        }

        void AssertIns(CfgNode node, params string[] regs)
        {
            CollectionAssert.AreEquivalent(regs, node.In);
        }
    }
}
