using System;
using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class InterferenceGraph
    {
        public ISet<string> Nodes { get; } = new HashSet<string>();
        public ISet<Tuple<string, string>> Graph { get; set; } = new HashSet<Tuple<string, string>>();

        // Node is either source or destination of a move instruction
        //public ISet<string> Moves { get; } = new HashSet<string>();
        public IList<M68kInstruction> Moves = new List<M68kInstruction>();

        public IList<ISet<string>> Allocation = new List<ISet<string>>();
        public IDictionary<string,int> AllocationMap = new Dictionary<string, int>();

        public void RemoveNode(string n)
        {
            Nodes.Remove(n);
            var edges = Graph.Where(e => e.Item1 == n || e.Item2 == n).ToList();
            foreach (var e in edges)
            {
                Graph.Remove(e);
            }
        }

        public bool IsEdgeBetween(string x, string y)
        {
            return Graph.Any(e => (e.Item1 == x && e.Item2 == y) || (e.Item1 == y && e.Item2 == x));
        }

        public IEnumerable<string> AdjListFor(string n)
        {
            return Graph.Where(e => (e.Item1 == n || e.Item2 == n))
                .Select(e => e.Item1 == n ? e.Item2 : e.Item1);
        }
    }

    public class InterferenceGraphGenerator
    {
        public static InterferenceGraph MakeGraph(IList<CfgNode> nodes)
        {
            //var graph = new bool[nodes.Count, nodes.Count];
            var graph = new HashSet<Tuple<string, string>>();
            var g = new InterferenceGraph { Graph = graph };

            foreach (var node in nodes)
            {
                foreach (var def in node.Def)
                {
                    foreach (var o in node.Out)
                    {
                        if (o != def && !graph.Contains(new Tuple<string, string>(def,o)) && !graph.Contains(new Tuple<string, string>(o, def)))
                        {
                            graph.Add(new Tuple<string, string>(def, o));
                        }
                        g.Nodes.Add(o);
                        g.Nodes.Add(def);
                        //                        graph.Add(new Tuple<string, string>(o, def));
                    }
                }

                if (node.Instruction.Register1 != null && node.Instruction.Register2 != null)
                {
                    var a = "D" + node.Instruction.Register1.Number;
                    var c = "D" + node.Instruction.Register2.Number;

                    if (node.Instruction.Opcode == M68kOpcode.Move)
                    {
                        //g.Moves.Add(a);
                        //g.Moves.Add(c);
                        g.Moves.Add(node.Instruction);
                    }

                    foreach (var b in node.Out)
                    {
                        if (b != c && !graph.Contains(new Tuple<string, string>(a,b)) && !graph.Contains(new Tuple<string, string>(b,a)))
                        {
                            graph.Add(new Tuple<string, string>(a, b));
                        }
                        g.Nodes.Add(b);
                        g.Nodes.Add(c);
                    }
                }
            }

            return g;
        }
    }
}