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
                new M68kInstruction(M68kOpcode.Move, 4, d0),
                new M68kInstruction(M68kOpcode.Jmp) { TargetLabel = "end" },
                new M68kInstruction(M68kOpcode.Label) { Label = "foo" },
                new M68kInstruction(M68kOpcode.Move, 1, d0),
                new M68kInstruction(M68kOpcode.Label) { Label = "end" },
                new M68kInstruction(M68kOpcode.Move, 3, d0),
                //new M68kInstruction(M68kOpcode.Rts) // This would introduce D2-D7
                new M68kInstruction(M68kOpcode.Move)
                {
                    AddressingMode1 = M68kAddressingMode.Register,
                    Register1 = d0,
                    AddressingMode2 = M68kAddressingMode.Address,
                    FinalRegister2 = M68kRegister.SP
                }
            };

            var la = new LivenessAnalysis(instructions);

            Assert.IsTrue(la.Nodes[0].Succ.Contains(la.Nodes[1]));
            Assert.IsTrue(la.Nodes[1].Pred.Contains(la.Nodes[0]));

            Assert.IsFalse(la.Nodes[1].Succ.Contains(la.Nodes[3]));
            Assert.IsFalse(la.Nodes[3].Pred.Contains(la.Nodes[1]));
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
