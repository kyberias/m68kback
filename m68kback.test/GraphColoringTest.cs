using System;
using System.Collections.Generic;
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
        Register d8 = new Register { Type = RegType.Data, Number = 8 };
        Register d9 = new Register { Type = RegType.Data, Number = 9 };
        Register d10 = new Register { Type = RegType.Data, Number = 10 };
        Register d11 = new Register { Type = RegType.Data, Number = 11 };

        [Test]
        public void Test()
        {
            var b = d1;
            var c = d2;
            var d = d3;
            var e = d4;
            var f = d5;
            var g = d6;
            var h = d7;
            var j = d9;
            var k = d10;
            var m = d11;
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

            var gc = new GraphColoring(instructions, 4);

            //var stack = gc.Simplify();

            //var newgraph = GraphColoring.Select(gc.Graph, stack);

            gc.Main();

            gc.FinalRewrite();
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
