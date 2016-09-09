using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace m68kback
{
    public class GraphColoring
    {
        private InterferenceGraph _graph;
        private IList<M68kInstruction> _code;
        private int K;

        public GraphColoring(IList<M68kInstruction> code, int k = 8)
        {
            K = k;
            _code = code;

//            Build(Liveness());

            // Build
            // Simplify
            // Spill
            // Select
        }

        public GraphColoring(IList<CfgNode> nodes)
        {
            Build(nodes);
        }

        public InterferenceGraph Graph => _graph;

        List<string> simplifyWorklist = new List<string>();

        IDictionary<string,List<M68kInstruction>> moveList = new Dictionary<string, List<M68kInstruction>>();

        List<M68kInstruction> worklistMoves = new List<M68kInstruction>();
        List<M68kInstruction> activeMoves = new List<M68kInstruction>();
        List<M68kInstruction> coalescedMoves = new List<M68kInstruction>();
        List<M68kInstruction> constrainedMoves = new List<M68kInstruction>();
        List<M68kInstruction> frozenMoves = new List<M68kInstruction>();

        List<string> freezeWorklist = new List<string>();
        List<string> spillWorklist = new List<string>();
        private ISet<string> spilledNodes = new HashSet<string>();
        ISet<string> coloredNodes = new HashSet<string>();
        List<string> initial = new List<string>();

        ISet<string> coalescedNodes = new HashSet<string>();

        private List<string> precolored = new List<string>();

        public void Init()
        {
            
        }

        void MakeInitial()
        {
            foreach (var node in _graph.Nodes)
            {
                if (!initial.Contains(node))
                {
                    initial.Add(node);
                }
            }
        }

        void DegreeInvariant()
        {
            foreach (var u in simplifyWorklist.Union(freezeWorklist.Union(spillWorklist)))
            {
                var adjList = _graph.AdjListFor(u).ToList();
                var d = adjList.Intersect(precolored.Union(simplifyWorklist.Union(freezeWorklist.Union(spillWorklist)))).Count();
                Debug.Assert(d == degree[u]);
            }
        }

        void SimplifyWorklistInvariant()
        {
            foreach (var u in simplifyWorklist)
            {
                Debug.Assert(degree[u] < K);
                Debug.Assert(!MoveList(u).Intersect(activeMoves.Union(worklistMoves)).Any());
            }
        }

        void FreezeWorklistInvariant()
        {
            foreach (var u in freezeWorklist)
            {
                Debug.Assert(degree[u] < K);
                Debug.Assert(MoveList(u).Intersect(activeMoves.Union(worklistMoves)).Any());
            }
        }

        void SpillWorklistInvariant()
        {
            foreach (var u in spillWorklist)
            {
                Debug.Assert(degree[u] >= K);
            }
        }

        [Conditional("DEBUG")]
        void CheckInvariants()
        {
            return;
            DegreeInvariant();
            SimplifyWorklistInvariant();
            FreezeWorklistInvariant();
            SpillWorklistInvariant();
        }

        public void FinalRewrite()
        {
            List<M68kInstruction> newCode = new List<M68kInstruction>();

            foreach (var i in _code)
            {
                if (!i.FinalRegister1.HasValue && i.Register1 != null)
                {
                    i.FinalRegister1 = M68kRegister.D0 + Color[i.Register1.ToString()];
                }

                if (!i.FinalRegister2.HasValue && i.Register2 != null)
                {
                    i.FinalRegister2 = M68kRegister.D0 + Color[i.Register2.ToString()];
                }
            }
        }

        public void Main()
        {
            var live = Liveness();
            Build(live);
            MakeInitial();

            CheckInvariants();
            MakeWorklist();
            CheckInvariants();
            do
            {
                if (simplifyWorklist.Count > 0)
                {
                    Simplify();
                }
                else if (worklistMoves.Count > 0)
                {
                    Coalesce();
                }
                else if (freezeWorklist.Count > 0)
                {
                    Freeze();
                }
                else if (spillWorklist.Count > 0)
                {
                    SelectSpill();
                }
                //CheckInvariants();
            } while (simplifyWorklist.Count > 0 || worklistMoves.Count > 0 || freezeWorklist.Count > 0 || spillWorklist.Count > 0);

            AssignColors();
            if (spilledNodes.Count > 0)
            {
                RewriteProgram();
                Main();
            }
        }

        private IList<CfgNode> Liveness()
        {
            LivenessAnalysis liveness = new LivenessAnalysis(_code);
            return liveness.Nodes;
        }

        List<string> frameSpills = new List<string>();

        Register GenerateRegLoad(int ix, IList<M68kInstruction> newCode, Register targetReg)
        {
            var newTemp = new Register { Type = RegType.Data, Number = ix };

            // Load from stack to new temporary
            newCode.Add(new M68kInstruction
            {
                FinalRegister1 = M68kRegister.SP,
                AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                Offset = frameSpills.IndexOf(targetReg.ToString()) * 4,
                Register2 = newTemp,
                AddressingMode2 = M68kAddressingMode.Register,
                Opcode = M68kOpcode.Move,
            });

            // Move from new temporary to target
            /*newCode.Add(new M68kInstruction
            {
                Register1 = newTemp,
                AddressingMode1 = M68kAddressingMode.Register,
                Register2 = targetReg,
                AddressingMode2 = M68kAddressingMode.Register,
                Opcode = M68kOpcode.Move,
            });*/
            return newTemp;
        }

        private void RewriteProgram()
        {
            List<M68kInstruction> newCode = new List<M68kInstruction>();
            List<string> newTemps = new List<string>();

            int ix = _graph.Nodes.Count + 1;

            foreach (var i in _code)
            {
                bool doOriginal = true;
                Register reg1newtemp = null;
                Register reg2newtemp = null;

                if (i.Register1 != null && spilledNodes.Contains(i.Register1.ToString()))
                {
                    // Use
                    var newTemp = GenerateRegLoad(ix, newCode, i.Register1);
                    ix++;
                    newTemps.Add(newTemp.ToString());
                    frameSpills.Add(i.Register1.ToString());
                    reg1newtemp = newTemp;
                }

                bool isMoveRegReg = i.Opcode == M68kOpcode.Move && i.AddressingMode1 == M68kAddressingMode.Register &&
                                    i.AddressingMode2 == M68kAddressingMode.Register;

                if (i.Register2 != null && spilledNodes.Contains(i.Register2.ToString()) && !isMoveRegReg)
                {
                    // Use
                    var newTemp = GenerateRegLoad(ix, newCode, i.Register2);
                    ix++;
                    newTemps.Add(newTemp.ToString());
                    frameSpills.Add(i.Register2.ToString());
                    reg2newtemp = newTemp;
                }

                if (i.Register2 != null && spilledNodes.Contains(i.Register2.ToString()))
                {
                    // Def
                    Register newTemp;
                    if (reg2newtemp == null)
                    {
                        newTemp = new Register {Type = RegType.Data, Number = ix};
                        ix++;
                        newTemps.Add(newTemp.ToString());
                        reg2newtemp = newTemp;
                    }
                }

                newCode.Add(new M68kInstruction
                {
                    Opcode = i.Opcode,
                    Register1 = reg1newtemp ?? i.Register1,
                    Register2 = reg2newtemp ?? i.Register2,
                    FinalRegister1 = i.FinalRegister1,
                    FinalRegister2 = i.FinalRegister2,
                    AddressingMode1 = i.AddressingMode1,
                    AddressingMode2 = i.AddressingMode2,
                    Immediate = i.Immediate,
                    Offset = i.Offset
                });

                if (i.Register2 != null && spilledNodes.Contains(i.Register2.ToString()))
                {
                    // Def
                    // Save temporary to stack
                    newCode.Add(new M68kInstruction
                    {
                        Register1 = reg2newtemp,
                        AddressingMode1 = M68kAddressingMode.Register,
                        Opcode = M68kOpcode.Move,
                        FinalRegister2 = M68kRegister.SP,
                        AddressingMode2 = M68kAddressingMode.AddressWithOffset,
                        Offset = frameSpills.Count * 4
                    });
                    frameSpills.Add(i.Register2.ToString());
                }
            }

            _code = newCode;

            spilledNodes.Clear();
            initial.Clear();
            initial.AddRange(coloredNodes.Union(coalescedNodes.Union(newTemps)));
            coloredNodes.Clear();
            coalescedNodes.Clear();
        }

        Dictionary<string,int> Color = new Dictionary<string, int>();

        private void AssignColors()
        {
            while (selectStack.Count > 0)
            {
                var n = selectStack.Pop();
                var okColors = Enumerable.Range(0, K - 1).ToList();
                foreach (var w in _graph.AdjListFor(n))
                {
                    var cp = coloredNodes.Union(precolored);
                    if (cp.Contains(GetAlias(w)))
                    {
                        okColors.Remove(Color[GetAlias(w)]);
                    }
                }
                if (okColors.Count == 0)
                {
                    spilledNodes.Add(n);
                }
                else
                {
                    coloredNodes.Add(n);
                    Color[n] = okColors.First();
                }
            }
            foreach (var n in coalescedNodes)
            {
                Color[n] = Color[GetAlias(n)];
            }
        }

        double SpillPriority(string node)
        {
            return 1.0;
        }

        private void SelectSpill()
        {
            var m = spillWorklist.OrderBy(SpillPriority).First();
            spillWorklist.Remove(m);
            simplifyWorklist.Add(m);
            FreezeMoves(m);
        }

        void FreezeMoves(string u)
        {
            foreach (var m in NodeMoves(u))
            {
                var x = m.Register1.ToString();
                var y = m.Register1.ToString();

                string v;
                if (GetAlias(x) == GetAlias(y))
                {
                    v = GetAlias(x);
                }
                else
                {
                    v = GetAlias(y);
                }
                activeMoves.Remove(m);
                frozenMoves.Add(m);
                if (freezeWorklist.Contains(v) && !NodeMoves(v).Any())
                {
                    freezeWorklist.Remove(v);
                    simplifyWorklist.Add(v);
                }
            }
        }

        private void Freeze()
        {
            var u = freezeWorklist.First();
            freezeWorklist.Remove(u);
            simplifyWorklist.Add(u);
            FreezeMoves(u);
        }

        public static InterferenceGraph Select(InterferenceGraph graph, Stack<string> stack)
        {
            var newgraph = new InterferenceGraph();

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                var edges = graph.Graph.Where(e => (newgraph.Nodes.Contains(e.Item1) && e.Item2 == node) ||
                                                   (newgraph.Nodes.Contains(e.Item2) && e.Item1 == node));
                foreach (var edge in edges)
                {
                    newgraph.Graph.Add(edge);
                }

                newgraph.Nodes.Add(node);

                // Select color (register) for node
                // Try to find existing color (register)
                int? register = null;

                for (var i=0;i<newgraph.Allocation.Count;i++)
                {
                    var alloc = newgraph.Allocation[i];

                    var interfered =
                        newgraph.Graph.Any(
                            e => ((e.Item2 == node && alloc.Contains(e.Item1)) || (e.Item1 == node && alloc.Contains(e.Item2))));
                    if (!interfered)
                    {
                        register = i;
                        alloc.Add(node);
                        break;
                    }
                }

                if (!register.HasValue)
                {
                    // Must allocate new

                    var set = new HashSet<string>();
                    set.Add(node);
                    newgraph.Allocation.Add(set);
                    register = newgraph.Allocation.Count - 1;
                }

                newgraph.AllocationMap[node] = register.Value;
            }

            return newgraph;
        }

        Dictionary<string,int> degree = new Dictionary<string, int>();

        private void MakeWorklist()
        {
            foreach (var n in initial)
            {
                if (degree[n] >= K)
                {
                    spillWorklist.Add(n);
                }
                else if (MoveRelated(n))
                {
                    freezeWorklist.Add(n);
                }
                else
                {
                    simplifyWorklist.Add(n);
                }
            }
            initial.Clear();
        }

        void AddEdge(string u, string v)
        {
            if (!_graph.IsEdgeBetween(u, v) && u != v)
            {
                _graph.Graph.Add(new Tuple<string, string>(u, v));
                Debug.Assert(!_graph.Graph.Contains(new Tuple<string, string>(v,u)));
                if (!precolored.Contains(u))
                {
                    degree[u]++;
                }
                if (!precolored.Contains(v))
                {
                    degree[v]++;
                }
            }
        }

        Stack<string> selectStack = new Stack<string>();

        IEnumerable<string> Adjacent(string n)
        {
            return _graph.AdjListFor(n).Except(selectStack.Union(coalescedNodes));
        }

        void Simplify()
        {
            var n = simplifyWorklist.First();
            simplifyWorklist.Remove(n);
            selectStack.Push(n);
            var adjacents = Adjacent(n).ToList();

            foreach (var m in adjacents)
            {
                DecrementDegree(m);
            }
        }

        void EnableMoves(IEnumerable<string> nodes)
        {
            foreach (var n in nodes)
            {
                foreach (var m in NodeMoves(n))
                {
                    if (activeMoves.Contains(m))
                    {
                        activeMoves.Remove(m);
                        worklistMoves.Add(m);
                    }
                }
            }
        }

        List<M68kInstruction> MoveList(string n)
        {
            if (moveList.ContainsKey(n))
            {
                return moveList[n];
            }
            else
            {
                var list = new List<M68kInstruction>();
                moveList[n] = list;
                return list;
            }
        }

        IEnumerable<M68kInstruction> NodeMoves(string n)
        {
            return MoveList(n).Intersect(activeMoves.Union(worklistMoves));
        }

        bool MoveRelated(string n)
        {
            return NodeMoves(n).Any();
        }

        void DecrementDegree(string m)
        {
            var d = degree[m];
            degree[m] = d - 1;

            if (d == K)
            {
                EnableMoves(Adjacent(m));
                EnableMoves(new [] { m });
                spillWorklist.Remove(m);

                if (MoveRelated(m))
                {
                    freezeWorklist.Add(m);
                }
                else
                {
                    simplifyWorklist.Add(m);
                }
            }
        }

        Dictionary<string,string> alias = new Dictionary<string, string>();

        string GetAlias(string n)
        {
            if (coalescedNodes.Contains(n))
            {
                return GetAlias(alias[n]);
            }
            return n;
        }

        void AddWorkList(string u)
        {
            if (!precolored.Contains(u) && !MoveRelated(u) && degree[u] < K)
            {
                freezeWorklist.Remove(u);
                simplifyWorklist.Add(u);
            }
        }

        /// <summary>
        /// OK implements the heuristic used for coalescing a precolored register.
        /// </summary>
        bool OK(string t, string r)
        {
            return degree[t] < K || precolored.Contains(t) || _graph.IsEdgeBetween(t, r);
        }

        /// <summary>
        /// Conservative implements the conservative coalescing heuristic.
        /// </summary>
        bool Conservative(IEnumerable<string> nodes)
        {
            var k = 0;
            foreach (var n in nodes)
            {
                if (degree[n] >= K)
                {
                    k++;
                }
            }
            return k < K;
        }

        void Coalesce()
        {
            var m = worklistMoves.First();
            var x = m.Register1.ToString();
            x = GetAlias(x);
            var y = m.Register2.ToString();
            y = GetAlias(y);

            string u;
            string v;

            if (precolored.Contains(y))
            {
                u = y;
                v = x;
            }
            else
            {
                u = x;
                v = y;
            }

            worklistMoves.Remove(m);

            if (u == v)
            {
                coalescedMoves.Add(m);
                AddWorkList(u);
            }
            else if (precolored.Contains(v) || _graph.IsEdgeBetween(u,v))
            {
                constrainedMoves.Add(m);
                AddWorkList(u);
                AddWorkList(v);
            }
            else if (precolored.Contains(u) && Adjacent(v).Any(t => OK(t, u)) && !precolored.Contains(u) &&
                     Conservative(Adjacent(u).Union(Adjacent(v))))
            {
                coalescedMoves.Add(m);
                Combine(u, v);
                AddWorkList(u);
            }
            else
            {
                activeMoves.Add(m);
            }
        }

        void Combine(string u, string v)
        {
            if (freezeWorklist.Contains(v))
            {
                freezeWorklist.Remove(v);
            }
            else
            {
                spillWorklist.Remove(v);
            }
            coalescedNodes.Add(v);
            alias[v] = u;
            MoveList(u).AddRange(MoveList(v));
            EnableMoves(new [] { v });

            foreach (var t in Adjacent(v))
            {
                AddEdge(t, u);
                DecrementDegree(t);
            }
            if (degree[u] >= K && freezeWorklist.Contains(u))
            {
                freezeWorklist.Remove(u);
                spillWorklist.Add(u);
            }
        }

        void Build(IList<CfgNode> nodes)
        {
            worklistMoves.Clear();

            _graph = InterferenceGraphGenerator.MakeGraph(nodes);
            worklistMoves.AddRange(_graph.Moves);

            foreach (var node in _graph.Nodes.Where(n => !precolored.Contains(n)))
            {
                degree[node] = _graph.Graph.Count(e => e.Item1 == node || e.Item2 == node);
            }
        }

        public Stack<string> Simplify(int K = 8)
        {
            var stack = new Stack<string>();

            var edges = new HashSet<Tuple<string,string>>(_graph.Graph);
            var nodes = new HashSet<string>(_graph.Nodes);
            var workingList = new List<string>();

            bool removed;
            do
            {
                removed = false;

                var nodesWithLessThanK = nodes.Where(n => edges.Count(u => u.Item1 == n || u.Item2 == n) < K && !workingList.Contains(n)).ToList();

                workingList.AddRange(nodesWithLessThanK);

                // Select one with most edges
                var node = workingList.OrderByDescending(n => edges.Count(u => u.Item1 == n || u.Item2 == n)).FirstOrDefault();

                if (node != null)
                {
                    var edgesToRemove = edges.Where(e => e.Item1 == node || e.Item2 == node).ToList();
                    foreach (var edge in edgesToRemove)
                    {
                        edges.Remove(edge);
                    }

                    stack.Push(node);
                    nodes.Remove(node);
                    workingList.Remove(node);
                    removed = true;
                }
            } while (removed);

            return stack;
        }
    }
}