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
