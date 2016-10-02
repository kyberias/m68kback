using System;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    public class InterferenceGraphTest
    {
        Register d0 = new Register {Type = RegType.Data, Number = 0};
        Register d1 = new Register {Type = RegType.Data, Number = 1};
        Register d2 = new Register {Type = RegType.Data, Number = 2};
        Register d3 = new Register {Type = RegType.Data, Number = 3};
        Register d4 = new Register { Type = RegType.Data, Number = 4 };
        Register d5 = new Register { Type = RegType.Data, Number = 5 };
        Register d6 = new Register { Type = RegType.Data, Number = 6 };
        Register d7 = new Register { Type = RegType.Data, Number = 7 };
        Register d8 = new Register { Type = RegType.Data, Number = 8 };
        Register d9 = new Register { Type = RegType.Data, Number = 9 };
        Register d10 = new Register { Type = RegType.Data, Number = 10 };
        Register d11 = new Register { Type = RegType.Data, Number = 11 };
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

            var g = InterferenceGraphGenerator.MakeGraph(la.Nodes, RegType.Data, "D0,D1,D2,D3,D4,D5,D6,D7".Split(',').ToList());
            var graph = g.Graph;

            Assert.IsTrue(graph.Contains(new Tuple<string, string>("D1", "D3")) || graph.Contains(new Tuple<string, string>("D3", "D1")));
            Assert.IsFalse(graph.Contains(new Tuple<string, string>("D1", "D1")));
        }

        [Test]
        public void Test2()
        {
            var b = d11;
            var c = d12;
            var d = d13;
            var e = d14;
            var f = d15;
            var g = d16;
            var h = d17;
            var j = d19;
            var k = d20;
            var m = d21;
            /*
             g := mem[j+12]
h := k - 1
f := g * h
e := mem[j+8]
m := mem[j+16]
b := mem[f]
c := e + 8
d := c
k := m + 4
j := b*/

            var instructions = new[]
            {
                M68kInstruction.LoadFromMemory(j, g),

                new M68kInstruction( M68kOpcode.Move, k, d0),
                new M68kInstruction( M68kOpcode.Sub, 1, d0),
                new M68kInstruction( M68kOpcode.Move, d0, h),

                new M68kInstruction( M68kOpcode.Move, g, d0),
                new M68kInstruction( M68kOpcode.Add, h, d0),
                new M68kInstruction( M68kOpcode.Move, d0, f),

                M68kInstruction.LoadFromMemory(j, e),
                M68kInstruction.LoadFromMemory(j, m),
                M68kInstruction.LoadFromMemory(f, b),

                new M68kInstruction( M68kOpcode.Move, e, d0),
                new M68kInstruction( M68kOpcode.Add, 8, d0),
                new M68kInstruction( M68kOpcode.Move, d0, c),

                new M68kInstruction( M68kOpcode.Move, c, d),

                new M68kInstruction( M68kOpcode.Move, m, d0),
                new M68kInstruction( M68kOpcode.Add, 4, d0),
                new M68kInstruction( M68kOpcode.Move, d0, k),

                new M68kInstruction( M68kOpcode.Move, b, j),

                // Cause Live out: d k j

                M68kInstruction.StoreToMemory(d, j),
                M68kInstruction.StoreToMemory(k, j),
                M68kInstruction.StoreToMemory(j, j), 

                //new M68kInstruction(M68kOpcode.Rts)
            };

            var la = new LivenessAnalysis(instructions);

            var gr = InterferenceGraphGenerator.MakeGraph(la.Nodes, RegType.Data, "D0,D1,D2,D3,D4,D5,D6,D7".Split(',').ToList());

            AssertEdge(gr, j, f);
            AssertEdge(gr, j, e);
            AssertEdge(gr, j, k);
            AssertEdge(gr, j, h);
            AssertEdge(gr, j, g);

            AssertEdge(gr, f, e);
            AssertEdge(gr, f, m);
            AssertEdge(gr, f, m);

            AssertEdge(gr, k, b);
            AssertEdge(gr, k, d);
            AssertEdge(gr, k, g);

            AssertEdge(gr, h, g);

            AssertEdge(gr, b, m);
            AssertEdge(gr, b, c);
            AssertEdge(gr, b, d);

            AssertNoEdge(gr, j, b);
            AssertNoEdge(gr, d, c);
        }

        void AssertEdge(InterferenceGraph graph, Register r1, Register r2)
        {
            Assert.IsTrue(
                graph.Graph.Contains(new Tuple<string, string>(r1.ToString(), r2.ToString())) || 
                graph.Graph.Contains(new Tuple<string, string>(r2.ToString(), r1.ToString())));
        }

        void AssertNoEdge(InterferenceGraph graph, Register r1, Register r2)
        {
            Assert.IsTrue(
                !graph.Graph.Contains(new Tuple<string, string>(r1.ToString(), r2.ToString())) &&
                !graph.Graph.Contains(new Tuple<string, string>(r2.ToString(), r1.ToString())));
        }
    }
}