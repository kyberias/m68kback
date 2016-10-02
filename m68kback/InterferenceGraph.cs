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
            return Graph.Where(e => e.Item1 == n || e.Item2 == n)
                .Select(e => e.Item1 == n ? e.Item2 : e.Item1).ToList();
        }
    }

    public class InterferenceGraphGenerator
    {
        public static InterferenceGraph MakeGraph(IList<CfgNode> nodes, RegType regType, IList<string> preColored)
        {
            //var graph = new bool[nodes.Count, nodes.Count];
            var graph = new HashSet<Tuple<string, string>>();
            var g = new InterferenceGraph { Graph = graph };

            // Print nodes
            foreach (var node in nodes.Where(n => n != null))
            {
//                Console.WriteLine($"{node.Instruction} def:{string.Join(",", node.Def)} in: {string.Join(",", node.In)} out: {string.Join(",", node.Out)}");
            }

            int added = 0;
            foreach (var p1 in preColored)
            {
                foreach (var p2 in preColored)
                {
                    if (p1 != p2 && !g.IsEdgeBetween(p1, p2) && !g.IsEdgeBetween(p2, p1))
                    {
                        g.Graph.Add(new Tuple<string, string>(p1, p2));
                        added++;
                    }
                }
                g.Nodes.Add(p1);
            }

            foreach (var node in nodes.Where(n => n != null))
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

                var inst = node.Instruction;

                if (inst.Register1 != null && inst.Register2 != null 
                    && inst.Register1.Type == regType && inst.Register2.Type == regType
                    && inst.AddressingMode1 == M68kAddressingMode.Register && inst.AddressingMode2 == M68kAddressingMode.Register)
                {
                    var a = node.Instruction.Register1.ToString();
                    var c = node.Instruction.Register2.ToString();

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