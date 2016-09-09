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
        public void Test()
        {
            var instructions = new[]
            {
                new M68kInstruction( M68kOpcode.Move, 4, d0),
                new M68kInstruction( M68kOpcode.Move, d0, d1),
                new M68kInstruction( M68kOpcode.Move, 1, d3),
                new M68kInstruction( M68kOpcode.Move, d1, d2),
                new M68kInstruction( M68kOpcode.Move, d2, d0),
                new M68kInstruction(M68kOpcode.Rts)
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
