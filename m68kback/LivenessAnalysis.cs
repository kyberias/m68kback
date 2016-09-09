using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class CfgNode
    {
        public M68kInstruction Instruction { get; set; }
        public List<CfgNode> Succ { get; } = new List<CfgNode>();
        public List<CfgNode> Pred { get; } = new List<CfgNode>();

        public HashSet<string> Out { get; set; } = new HashSet<string>();
        public HashSet<string> In { get; set; } = new HashSet<string>();

        public List<string> Def { get; } = new List<string>();
    }

    public class LivenessAnalysis
    {
        public LivenessAnalysis(IList<M68kInstruction> code)
        {
            CfgNode prev = null;

            var labels = new Dictionary<string,int>();
            var currLabels = new List<string>();
            var defs = new List<string>();

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Opcode == M68kOpcode.Label)
                {
                    currLabels.Add(code[i].Label);
                }
                else if (currLabels.Count > 0)
                {
                    foreach (var label in currLabels)
                    {
                        labels[label] = i;
                    }
                    currLabels.Clear();
                }
            }

            var nodes = new List<CfgNode>();

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Opcode == M68kOpcode.Label)
                {
                    nodes.Add(null);
                    continue;
                }

                var node = new CfgNode();
                nodes.Add(node);
                node.Instruction = code[i];

                if (code[i].Register2 != null)
                {
                    var def = "D" + code[i].Register2.Number;
                    if (!defs.Contains(def))
                    {
                        defs.Add(def);
                    }
                    node.Def.Add(def);
                }

                if (prev != null)
                {
                    prev.Succ.Add(node);
                    node.Pred.Add(prev);
                }

                if (code[i].Opcode != M68kOpcode.Rts)
                {
                    prev = node;
                }
                else
                {
                    prev = null;
                }
            }

            for (int i = 0; i < code.Count; i++)
            {
                if (nodes[i] != null && nodes[i].Instruction.IsBranch())
                {
                    if (labels.ContainsKey(code[i].TargetLabel.Replace("%", "")))
                    {
                        var label = labels[code[i].TargetLabel.Replace("%", "")];
                        nodes[label].Pred.Add(nodes[i]);
                    }
                }
            }

            IterativeDataFlowAnalysis(nodes);
//            var graph = InterferenceGraph(nodes);

            _nodes = nodes;
        }

        private IList<CfgNode> _nodes;
        public IList<CfgNode> Nodes => _nodes;

        void IterativeDataFlowAnalysis(IList<CfgNode> nodes)
        {
            bool changes;
            do
            {
                changes = false;
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    var n = nodes[i];
                    var newout = new HashSet<string>(n.Out);

                    foreach (var s in n.Succ)
                    {
                        foreach (var sn in s.In)
                        {
                            newout.Add(sn);
                        }
                    }

                    var use = n.Instruction.Use.ToList();
                    var def = n.Def;

                    var newin = new HashSet<string>(use.Union(newout.Where(o => !def.Contains(o))));

                    if (!newin.SetEquals(n.In))
                    {
                        changes = true;
                        n.In = newin;
                    }

                    if (!newout.SetEquals(n.Out))
                    {
                        changes = true;
                        n.Out = newout;
                    }
                }
            } while (changes);
        }
    }
}