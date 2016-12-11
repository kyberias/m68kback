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
}